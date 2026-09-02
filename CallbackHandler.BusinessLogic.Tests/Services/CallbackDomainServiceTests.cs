using SecurityService.Client;
using Shared.General;
using SimpleResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Logger;
using TransactionProcessor.Client;
using Imposter.Abstractions;

namespace CallbackHandler.BusinessLogic.Tests.Services;

using BusinessLogic.Services;
using CallbackHander.Testing;
using CallbackHandlers.Models;
using CallbackMessageAggregate;
using Microsoft.Extensions.Configuration;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using Shouldly;
using System.Threading;
using Xunit;

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
            .ReturnsAsync(TestData.EmptyCallbackMessageAggregate());
        this.AggregateRepository.SaveChanges(Arg<CallbackMessageAggregate>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success());
        this.SecurityServiceClient.GetToken(Arg<String>.Any(), Arg<String>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.TokenResponse()));
        this.TransactionProcessorClient.GetMerchant(Arg<String>.Any(), Arg<Guid>.Any(), Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success());

        IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
        ConfigurationReader.Initialise(configurationRoot);

        Logger.Initialise(NullLogger.Instance);
    }


    [Fact]
    public async Task CallbackDomainService_RecordCallback_CallbackRecorded() {
        Result result = await this.DomainService.RecordCallback(TestData.RecordCallbackCommand, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task CallbackDomainService_RecordCallback_EmptyReference_ResultFailed() {
        Result result = await this.DomainService.RecordCallback(TestData.RecordCallbackCommandEmptyReference, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task CallbackDomainService_RecordCallback_InvalidReference_ResultFailed() {
        Result result = await this.DomainService.RecordCallback(TestData.RecordCallbackCommandInvalidReference, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task CallbackDomainService_RecordCallback_EstateIdNotValidGuid_ResultFailed()
    {
        Result result = await this.DomainService.RecordCallback(TestData.RecordCallbackCommandInvalidEstateIdInReference, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task CallbackDomainService_RecordCallback_MerchantIdNotValidGuid_ResultFailed()
    {
        Result result = await this.DomainService.RecordCallback(TestData.RecordCallbackCommandInvalidMerchantIdInReference, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task CallbackDomainService_RecordCallback_GetTokenFailed_ResultFailed()
    {
        this.SecurityServiceClient.GetToken(Arg<String>.Any(), Arg<String>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Failure());
        Result result = await this.DomainService.RecordCallback(TestData.RecordCallbackCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task CallbackDomainService_RecordCallback_GetMerchantFailed_ResultFailed()
    {
        this.TransactionProcessorClient.GetMerchant(Arg<String>.Any(), Arg<Guid>.Any(), Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Failure());

        Result result = await this.DomainService.RecordCallback(TestData.RecordCallbackCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task CallbackDomainService_RecordCallback_SaveFailed_ResultFailed()
    {
        this.AggregateRepository.SaveChanges(Arg<CallbackMessageAggregate>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Failure());
        Result result = await this.DomainService.RecordCallback(TestData.RecordCallbackCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }
}
