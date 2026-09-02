using SimpleResults;

namespace CallbackHandler.BusinessLogic.Tests.RequestHandler;

using System;
using System.Threading;
using BusinessLogic.RequestHandler;
using BusinessLogic.Requests;
using BusinessLogic.Services;
using CallbackHander.Testing;
using CallbackHandlers.Models;
using CallbackMessageAggregate;
using MediatR;
using Imposter.Abstractions;
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
        ICallbackDomainServiceImposter domainService = new();
        IAggregateRepositoryImposter<CallbackMessageAggregate, DomainEvent> aggregateRepository = new();

        CallbackHandlerRequestHandler handler = new(domainService.Instance(), aggregateRepository.Instance());
        
        CallbackCommands.RecordCallbackCommand request = TestData.RecordCallbackCommand;

        Should.NotThrow(async () => await handler.Handle(request, CancellationToken.None));
    }

    [Fact]
    public void CallbackHandlerRequestHandlerTests_GetCallbackQuery_IsHandled()
    {
        ICallbackDomainServiceImposter domainService =
            new();
            IAggregateRepositoryImposter<CallbackMessageAggregate, DomainEvent> aggregateRepository =
                new();

            aggregateRepository.GetLatestVersion(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(TestData.RecordedCallbackMessageAggregate());

            CallbackHandlerRequestHandler handler = new(domainService.Instance(), aggregateRepository.Instance());

            CallbackQueries.GetCallbackQuery query = TestData.GetCallbackQuery;

            Should.NotThrow(async () =>
            {
                Result<CallbackMessage> callback = await handler.Handle(query, CancellationToken.None);
                callback.IsSuccess.ShouldBeTrue();
                callback.Data.Reference.ShouldBe(TestData.RecordedCallbackMessageAggregate().Reference);
            });
        }
    }
