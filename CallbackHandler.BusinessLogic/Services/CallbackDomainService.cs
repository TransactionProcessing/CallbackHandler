using SecurityService.Client;
using SecurityService.DataTransferObjects;
using TransactionProcessing.Shared.DomainDrivenDesign.EventSourcing;
using TransactionProcessing.Shared.EventStore.Aggregate;
using TransactionProcessing.Shared.EventStore.Helpers;
using TransactionProcessing.Shared.General;
using TransactionProcessing.Shared.Logger;
using TransactionProcessing.UnionResults;
using TransactionProcessor.Client;
using TransactionProcessor.DataTransferObjects.Responses.Merchant;

namespace CallbackHandler.BusinessLogic.Services;

using CallbackMessageAggregate;
using Requests;
using System;
using System.Threading;
using System.Threading.Tasks;

public interface ICallbackDomainService
{
    Task<Result<Guid>> RecordCallback(CallbackCommands.RecordCallbackCommand command,
                                CancellationToken cancellationToken);
}

public class CallbackDomainService : ICallbackDomainService
{
    private readonly IAggregateRepository<CallbackMessageAggregate, DomainEvent> AggregateRepository;
    private readonly ISecurityServiceClient SecurityServiceClient;
    private readonly ITransactionProcessorClient TransactionProcessorClient;

    public CallbackDomainService(IAggregateRepository<CallbackMessageAggregate, DomainEvent> aggregateRepository,
                                 ISecurityServiceClient securityServiceClient,
                                 ITransactionProcessorClient transactionProcessorClient) {
        this.AggregateRepository = aggregateRepository;
        this.SecurityServiceClient = securityServiceClient;
        this.TransactionProcessorClient = transactionProcessorClient;
    }

    public async Task<Result<Guid>> RecordCallback(CallbackCommands.RecordCallbackCommand command,
                                             CancellationToken cancellationToken) {

        Guid callbackId = Guid.NewGuid();

        // validate the reference
        Result<(Guid estateId, Guid merchantId)> validateResult = await ValidateReference(command.Reference, cancellationToken);
        if (validateResult.Value is not Success<(Guid estateId, Guid merchantId)> reference)
            return new ErrorList(validateResult.GetErrors());
        Result<CallbackMessageAggregate> getResult = await this.AggregateRepository.GetLatestVersion(callbackId, cancellationToken);
        Result<CallbackMessageAggregate> callbackMessageAggregateResult =
            DomainServiceHelper.HandleGetAggregateResult(getResult, callbackId, false);

        if (callbackMessageAggregateResult.Value is not Success<CallbackMessageAggregate> aggregateResult)
            return new ErrorList(callbackMessageAggregateResult.GetErrors());

        CallbackMessageAggregate aggregate = aggregateResult.Value;
        Result stateResult = aggregate.RecordCallback(callbackId, command.TypeString, command.MessageFormat, command.CallbackMessage, command.Destinations,
            (command.Reference, reference.Value.estateId, reference.Value.merchantId));
        if (!stateResult.IsSuccess())
            return new ErrorList(stateResult.GetErrors());
        
        Result saveResult = await this.AggregateRepository.SaveChanges(aggregate, cancellationToken);
        if (!saveResult.IsSuccess())
            return new ErrorList(saveResult.GetErrors());

        return Results.Ok(callbackId);
    }

    private async Task<Result<(Guid estateId, Guid merchantId)>> ValidateReference(String reference, CancellationToken cancellationToken) {
        String[] referenceData = reference?.Split(['-'], StringSplitOptions.RemoveEmptyEntries) ?? [];


        if (referenceData.Length == 0)
            return new Error(ErrorType.Validation, "Reference cannot be empty.");
        if (referenceData.Length != 2)
            return new Error(ErrorType.Validation, "Reference must contain estate and merchant references separated by a hyphen.");

        // Element 0 is estate reference, Element 1 is merchant reference
        String estateReference = referenceData[0];
        String merchantReference = referenceData[1];

        // Validate the reference data
        // Validate the estate and merchant references are valid GUIDs
        if (!Guid.TryParse(estateReference, out Guid estateId) || !Guid.TryParse(merchantReference, out Guid merchantId))
        {
            return new Error(ErrorType.Validation, "Estate or Merchant reference is not a valid GUID.");
        }

        Result<TokenResponse> getTokenResult = await Helpers.GetToken(this.TokenResponse, this.SecurityServiceClient, cancellationToken);
        if (getTokenResult.Value is not Success<TokenResponse> token)
            return new ErrorList(getTokenResult.GetErrors());
        this.TokenResponse = token.Value;

        Result<MerchantResponse> getMerchantResult = await this.TransactionProcessorClient.GetMerchant(this.TokenResponse.AccessToken, estateId, merchantId, cancellationToken);
        if (!getMerchantResult.IsSuccess())
            return new ErrorList(getMerchantResult.GetErrors());

        return Results.Ok((estateId, merchantId));
    }

    private TokenResponse TokenResponse;
}

public static class Helpers {
    public static async Task<Result<TokenResponse>> GetToken(TokenResponse currentToken, ISecurityServiceClient securityServiceClient, CancellationToken cancellationToken)
    {
        // Get a token to talk to the estate service
        String clientId = ConfigurationReader.GetValue("AppSettings", "ClientId");
        String clientSecret = ConfigurationReader.GetValue("AppSettings", "ClientSecret");
        
        if (currentToken == null)
        {
            return await securityServiceClient.GetToken(clientId, clientSecret, cancellationToken);
        }

        if (currentToken.Expires.UtcDateTime.Subtract(DateTime.UtcNow) < TimeSpan.FromMinutes(2))
        {
            Logger.LogDebug("Token is about to expire at {tokenExpiry}", currentToken.Expires.DateTime);
            return await securityServiceClient.GetToken(clientId, clientSecret, cancellationToken);
        }

        return Results.Ok(currentToken);
    }
}
