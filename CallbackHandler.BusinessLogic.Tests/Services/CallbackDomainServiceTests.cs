using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleResults;

namespace CallbackHandler.BusinessLogic.Tests.Services;

using System.Threading;
using BusinessLogic.Services;
using CallbackHander.Testing;
using CallbackHandlers.Models;
using CallbackMessageAggregate;
using Moq;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using Shouldly;
using Xunit;

public class CallbackDomainServiceTests
{
    private readonly ICallbackDomainService DomainService;

    private readonly Mock<IAggregateRepository<CallbackMessageAggregate, DomainEvent>> AggregateRepository;
    public CallbackDomainServiceTests() {
        this.AggregateRepository = new Mock<IAggregateRepository<CallbackMessageAggregate, DomainEvent>>();
        this.DomainService = new CallbackDomainService(this.AggregateRepository.Object);
        this.AggregateRepository.Setup(a => a.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.EmptyCallbackMessageAggregate());
        this.AggregateRepository.Setup(a => a.SaveChanges(It.IsAny<CallbackMessageAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);
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
}
