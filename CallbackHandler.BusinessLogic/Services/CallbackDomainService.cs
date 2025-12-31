using SecurityService.Client;
using Shared.EventStore.Helpers;
using Shared.Logger;
using SimpleResults;
using TransactionProcessor.Client;
using TransactionProcessor.DataTransferObjects.Responses.Merchant;

namespace CallbackHandler.BusinessLogic.Services;

using CallbackMessageAggregate;
using Requests;
using SecurityService.DataTransferObjects.Responses;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using Shared.General;
using Shared.Results;
using System;
using System.Threading;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

public interface ICallbackDomainService
{
    Task<Result> RecordCallback(CallbackCommands.RecordCallbackCommand command,
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

    public async Task<Result> RecordCallback(CallbackCommands.RecordCallbackCommand command,
                                             CancellationToken cancellationToken) {

        // validate the reference
        Result<(Guid estateId, Guid merchantId)> validateResult = await ValidateReference(command.Reference, cancellationToken);
        if (validateResult.IsFailed)
            return ResultHelpers.CreateFailure(validateResult);

        Result<CallbackMessageAggregate> getResult = await this.AggregateRepository.GetLatestVersion(command.CallbackId, cancellationToken);
        Result<CallbackMessageAggregate> callbackMessageAggregateResult =
            DomainServiceHelper.HandleGetAggregateResult(getResult, command.CallbackId, false);

        CallbackMessageAggregate aggregate = callbackMessageAggregateResult.Data;
        Result stateResult = aggregate.RecordCallback(command.CallbackId, command.TypeString, command.MessageFormat, command.CallbackMessage, command.Destinations,
            (command.Reference,validateResult.Data.estateId, validateResult.Data.merchantId));
        if (stateResult.IsFailed)
            return stateResult;
        return await this.AggregateRepository.SaveChanges(aggregate, cancellationToken);
    }

    private async Task<Result<(Guid estateId, Guid merchantId)>> ValidateReference(String reference, CancellationToken cancellationToken) {
        String[] referenceData = reference?.Split(['-'], StringSplitOptions.RemoveEmptyEntries) ?? [];

        if (referenceData.Length == 0)
        {
            return Result.Invalid("Reference cannot be empty.");
        }

        if (referenceData.Length != 2)
        {
            return Result.Invalid("Reference must contain estate and merchant references separated by a hyphen.");
        }

        // Element 0 is estate reference, Element 1 is merchant reference
        String estateReference = referenceData[0];
        String merchantReference = referenceData[1];

        // Validate the reference data
        // Validate the estate and merchant references are valid GUIDs
        if (!Guid.TryParse(estateReference, out Guid estateId) || !Guid.TryParse(merchantReference, out Guid merchantId))
        {
            return Result.Invalid("Estate or Merchant reference is not a valid GUID.");
        }

        Result<TokenResponse> getTokenResult = await Helpers.GetToken(this.TokenResponse, this.SecurityServiceClient, cancellationToken);
        if (getTokenResult.IsFailed)
            return ResultHelpers.CreateFailure(getTokenResult);
        this.TokenResponse = getTokenResult.Data;

        Result<MerchantResponse> result = await this.TransactionProcessorClient.GetMerchant(this.TokenResponse.AccessToken, estateId, merchantId, cancellationToken);
        if (result.IsFailed)
            return ResultHelpers.CreateFailure(result);

        return Result.Success((estateId,merchantId));
    }

    private TokenResponse TokenResponse;
}

public static class Helpers {
    public static async Task<Result<TokenResponse>> GetToken(TokenResponse currentToken, ISecurityServiceClient securityServiceClient, CancellationToken cancellationToken)
    {
        // Get a token to talk to the estate service
        String clientId = ConfigurationReader.GetValue("AppSettings", "ClientId");
        String clientSecret = ConfigurationReader.GetValue("AppSettings", "ClientSecret");
        Logger.LogDebug($"Client Id is {clientId}");
        Logger.LogDebug($"Client Secret is {clientSecret}");

        if (currentToken == null)
        {
            Result<TokenResponse> tokenResult = await securityServiceClient.GetToken(clientId, clientSecret, cancellationToken);
            if (tokenResult.IsFailed)
                return ResultHelpers.CreateFailure(tokenResult);
            TokenResponse token = tokenResult.Data;
            Logger.LogDebug($"Token is {token.AccessToken}");
            return Result.Success(token);
        }

        if (currentToken.Expires.UtcDateTime.Subtract(DateTime.UtcNow) < TimeSpan.FromMinutes(2))
        {
            Logger.LogDebug($"Token is about to expire at {currentToken.Expires.DateTime:O}");
            Result<TokenResponse> tokenResult = await securityServiceClient.GetToken(clientId, clientSecret, cancellationToken);
            if (tokenResult.IsFailed)
                return ResultHelpers.CreateFailure(tokenResult);
            TokenResponse token = tokenResult.Data;
            Logger.LogDebug($"Token is {token.AccessToken}");
            return Result.Success(token);
        }

        return Result.Success(currentToken);
    }
}