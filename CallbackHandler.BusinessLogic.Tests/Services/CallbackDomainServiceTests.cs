using SecurityService.Client;
using SecurityService.DataTransferObjects.Responses;
using Shared.General;
using SimpleResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Logger;
using TransactionProcessor.Client;

namespace CallbackHandler.BusinessLogic.Tests.Services;

using BusinessLogic.Services;
using CallbackHander.Testing;
using CallbackHandlers.Models;
using CallbackMessageAggregate;
using Microsoft.Extensions.Configuration;
using Moq;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using Shouldly;
using System.Threading;
using Xunit;

public class CallbackDomainServiceTests
{
    private readonly ICallbackDomainService DomainService;

    private readonly Mock<IAggregateRepository<CallbackMessageAggregate, DomainEvent>> AggregateRepository;
    private readonly Mock<ISecurityServiceClient> SecurityServiceClient;
    private readonly Mock<ITransactionProcessorClient> TransactionProcessorClient;
    public CallbackDomainServiceTests() {
        this.AggregateRepository = new Mock<IAggregateRepository<CallbackMessageAggregate, DomainEvent>>();
        this.SecurityServiceClient = new Mock<ISecurityServiceClient>();
        this.TransactionProcessorClient = new Mock<ITransactionProcessorClient>();
        this.DomainService = new CallbackDomainService(this.AggregateRepository.Object, this.SecurityServiceClient.Object,
            this.TransactionProcessorClient.Object);
        this.AggregateRepository.Setup(a => a.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.EmptyCallbackMessageAggregate());
        this.AggregateRepository.Setup(a => a.SaveChanges(It.IsAny<CallbackMessageAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));
        this.TransactionProcessorClient.Setup(t => t.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success());

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
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure());
        Result result = await this.DomainService.RecordCallback(TestData.RecordCallbackCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task CallbackDomainService_RecordCallback_GetMerchantFailed_ResultFailed()
    {
        this.TransactionProcessorClient.Setup(t => t.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure());

        Result result = await this.DomainService.RecordCallback(TestData.RecordCallbackCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task CallbackDomainService_RecordCallback_SaveFailed_ResultFailed()
    {
        this.AggregateRepository.Setup(a => a.SaveChanges(It.IsAny<CallbackMessageAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure);
        Result result = await this.DomainService.RecordCallback(TestData.RecordCallbackCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }
}
