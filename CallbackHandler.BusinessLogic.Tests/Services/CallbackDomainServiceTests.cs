using SecurityService.Client;
using System;
using System.Threading.Tasks;
using TransactionProcessor.Client;
using Imposter.Abstractions;
using Shouldly;
using TransactionProcessing.Shared.DomainDrivenDesign.EventSourcing;
using TransactionProcessing.Shared.EventStore.Aggregate;
using TransactionProcessing.Shared.General;
using TransactionProcessing.Shared.Logger;
using TransactionProcessing.UnionResults;

namespace CallbackHandler.BusinessLogic.Tests.Services;

using BusinessLogic.Services;
using CallbackHandler.Testing;
using CallbackMessageAggregate;
using Microsoft.Extensions.Configuration;
using System.Threading;
using Xunit;
using TransactionProcessorResult = TransactionProcessing.UnionResults.Result<TransactionProcessor.DataTransferObjects.Responses.Merchant.MerchantResponse>;

public class CallbackDomainServiceTests
{
    private readonly ICallbackDomainService DomainService;

    private readonly IAggregateRepositoryImposter<CallbackMessageAggregate, DomainEvent> AggregateRepository;
    private readonly ISecurityServiceClientImposter SecurityServiceClient;
    private readonly ITransactionProcessorClientImposter TransactionProcessorClient;
    public CallbackDomainServiceTests() {
        this.AggregateRepository = new();
        this.SecurityServiceClient = new();
        this.TransactionProcessorClient = new();
        this.DomainService = new CallbackDomainService(this.AggregateRepository.Instance(), this.SecurityServiceClient.Instance(),
            this.TransactionProcessorClient.Instance());
        this.AggregateRepository.GetLatestVersion(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Results.Ok(TestData.EmptyCallbackMessageAggregate()));
        this.AggregateRepository.SaveChanges(Arg<CallbackMessageAggregate>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Results.Ok());
        this.SecurityServiceClient.GetToken(Arg<String>.Any(), Arg<String>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Results.Ok(TestData.TokenResponse()));
        this.TransactionProcessorClient.GetMerchant(Arg<String>.Any(), Arg<Guid>.Any(), Arg<Guid>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Results.Ok(new TransactionProcessor.DataTransferObjects.Responses.Merchant.MerchantResponse()));

        IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
        ConfigurationReader.Initialise(configurationRoot);

        Logger.Initialise(NullLogger.Instance);
    }


    [Fact]
    public async Task CallbackDomainService_RecordCallback_CallbackRecorded() {
        Result<Guid> result = await this.DomainService.RecordCallback(TestData.RecordCallbackCommand, CancellationToken.None);
        result.IsSuccess().ShouldBeTrue();
        result.Value.ShouldBeOfType<Success<Guid>>().Value.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task CallbackDomainService_RecordCallback_EmptyReference_ResultFailed() {
        Result<Guid> result = await this.DomainService.RecordCallback(TestData.RecordCallbackCommandEmptyReference, CancellationToken.None);
        result.IsSuccess().ShouldBeFalse();
    }

    [Fact]
    public async Task CallbackDomainService_RecordCallback_InvalidReference_ResultFailed() {
        Result<Guid> result = await this.DomainService.RecordCallback(TestData.RecordCallbackCommandInvalidReference, CancellationToken.None);
        result.IsSuccess().ShouldBeFalse();
    }

    [Fact]
    public async Task CallbackDomainService_RecordCallback_EstateIdNotValidGuid_ResultFailed()
    {
        Result<Guid> result = await this.DomainService.RecordCallback(TestData.RecordCallbackCommandInvalidEstateIdInReference, CancellationToken.None);
        result.IsSuccess().ShouldBeFalse();
    }

    [Fact]
    public async Task CallbackDomainService_RecordCallback_MerchantIdNotValidGuid_ResultFailed()
    {
        Result<Guid> result = await this.DomainService.RecordCallback(TestData.RecordCallbackCommandInvalidMerchantIdInReference, CancellationToken.None);
        result.IsSuccess().ShouldBeFalse();
    }

    [Fact]
    public async Task CallbackDomainService_RecordCallback_GetTokenFailed_ResultFailed()
    {
        this.SecurityServiceClient.GetToken(Arg<String>.Any(), Arg<String>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync((Result<SecurityService.DataTransferObjects.TokenResponse>)new Error(ErrorType.Failure, "Token Failed"));
        Result<Guid> result = await this.DomainService.RecordCallback(TestData.RecordCallbackCommand, CancellationToken.None);
        result.IsSuccess().ShouldBeFalse();
    }

    [Fact]
    public async Task CallbackDomainService_RecordCallback_GetMerchantFailed_ResultFailed()
    {
        this.TransactionProcessorClient.GetMerchant(Arg<String>.Any(), Arg<Guid>.Any(), Arg<Guid>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync((TransactionProcessorResult)new Error(ErrorType.NotFound, "Merchant not found."));

        Result<Guid> result = await this.DomainService.RecordCallback(TestData.RecordCallbackCommand, CancellationToken.None);
        result.IsSuccess().ShouldBeFalse();
    }

    [Fact]
    public async Task CallbackDomainService_RecordCallback_SaveFailed_ResultFailed()
    {
        this.AggregateRepository.SaveChanges(Arg<CallbackMessageAggregate>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Results.Fail("Save Failed"));
        Result<Guid> result = await this.DomainService.RecordCallback(TestData.RecordCallbackCommand, CancellationToken.None);
        result.IsSuccess().ShouldBeFalse();
    }
}
