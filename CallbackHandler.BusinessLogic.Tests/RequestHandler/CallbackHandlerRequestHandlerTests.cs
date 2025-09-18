using SimpleResults;

namespace CallbackHandler.BusinessLogic.Tests.RequestHandler
{
    using System;
    using System.Threading;
    using BusinessLogic.RequestHandler;
    using BusinessLogic.Requests;
    using BusinessLogic.Services;
    using CallbackHander.Testing;
    using CallbackHandlers.Models;
    using CallbackMessageAggregate;
    using MediatR;
    using Moq;
    using Services;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.Aggregate;
    using Shouldly;
    using Xunit;

    public class CallbackHandlerRequestHandlerTests
    {
        [Fact]
        public void CallbackHandlerRequestHandlerTests_RecordCallbackRequest_IsHandled()
        {
            Mock<ICallbackDomainService> domainService =
                new Mock<ICallbackDomainService>();
            domainService.Setup(a => a.RecordCallback(It.IsAny<Guid>(),
                                                      It.IsAny<String>(),
                                                      It.IsAny<MessageFormat>(),
                                                      It.IsAny<String>(),
                                                      It.IsAny<String>(),
                                                      It.IsAny<String[]>(),
                                                      It.IsAny<CancellationToken>()));
            Mock<IAggregateRepository<CallbackMessageAggregate, DomainEvent>> aggregateRepository =
                new Mock<IAggregateRepository<CallbackMessageAggregate, DomainEvent>>();

            CallbackHandlerRequestHandler handler = new CallbackHandlerRequestHandler(domainService.Object, aggregateRepository.Object);
            
            CallbackCommands.RecordCallbackRequest request = TestData.RecordCallbackRequest;

            Should.NotThrow(async () => await handler.Handle(request, CancellationToken.None));
        }

        [Fact]
        public void CallbackHandlerRequestHandlerTests_GetCallbackQuery_IsHandled()
        {
            Mock<ICallbackDomainService> domainService =
                new Mock<ICallbackDomainService>();
            domainService.Setup(a => a.RecordCallback(It.IsAny<Guid>(),
                It.IsAny<String>(),
                It.IsAny<MessageFormat>(),
                It.IsAny<String>(),
                It.IsAny<String>(),
                It.IsAny<String[]>(),
                It.IsAny<CancellationToken>()));
            Mock<IAggregateRepository<CallbackMessageAggregate, DomainEvent>> aggregateRepository =
                new Mock<IAggregateRepository<CallbackMessageAggregate, DomainEvent>>();

            aggregateRepository.Setup(a => a.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.RecordedCallbackMessageAggregate());

            CallbackHandlerRequestHandler handler = new CallbackHandlerRequestHandler(domainService.Object, aggregateRepository.Object);

            CallbackQueries.GetCallbackQuery query = TestData.GetCallbackQuery;

            Should.NotThrow(async () =>
            {
                Result<CallbackMessage> callback = await handler.Handle(query, CancellationToken.None);
                callback.IsSuccess.ShouldBeTrue();
                callback.Data.Reference.ShouldBe(TestData.RecordedCallbackMessageAggregate().Reference);
            });
        }
    }
}
