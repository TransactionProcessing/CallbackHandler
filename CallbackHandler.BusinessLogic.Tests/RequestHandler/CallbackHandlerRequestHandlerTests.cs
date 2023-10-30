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
            CallbackHandlerRequestHandler handler = new CallbackHandlerRequestHandler(domainService.Object);
            
            RecordCallbackRequest request = TestData.RecordCallbackRequest;

            Should.NotThrow(async () =>
                            {
                                await handler.Handle(request, CancellationToken.None);
                            });
        }
    }
}
