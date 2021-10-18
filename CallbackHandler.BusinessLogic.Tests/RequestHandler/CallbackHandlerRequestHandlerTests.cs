namespace CallbackHandler.BusinessLogic.Tests.RequestHandler
{
    using System;
    using System.Threading;
    using BusinessLogic.RequestHandler;
    using BusinessLogic.Requests;
    using CallbackHander.Testing;
    using CallbackMessageAggregate;
    using Moq;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.Aggregate;
    using Shouldly;
    using Xunit;

    public class CallbackHandlerRequestHandlerTests
    {
        [Fact]
        public void CallbackHandlerRequestHandlerTests_RecordCallbackRequest_IsHandled()
        {
            Mock<IAggregateRepository<CallbackMessageAggregate, DomainEventRecord.DomainEvent>> aggregateRepository =
                new Mock<IAggregateRepository<CallbackMessageAggregate, DomainEventRecord.DomainEvent>>();
            aggregateRepository.Setup(a => a.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.EmptyCallbackMessageAggregate);
            CallbackHandlerRequestHandler handler = new CallbackHandlerRequestHandler(aggregateRepository.Object);
            
            RecordCallbackRequest request = TestData.RecordCallbackRequest;

            Should.NotThrow(async () =>
                            {
                                await handler.Handle(request, CancellationToken.None);
                            });
        }
    }
}
