using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        
    }

    [Fact]
    public async Task CallbackDomainService_RecordCallback_CallbackRecorded() {
        this.AggregateRepository.Setup(a => a.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                               .ReturnsAsync(TestData.EmptyCallbackMessageAggregate());

        Should.NotThrow(async () => await this.DomainService.RecordCallback(TestData.RecordCallbackCommand,
                                                                            CancellationToken.None));
    }
}
