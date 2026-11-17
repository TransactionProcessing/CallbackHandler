using CallbackHandler.Testing;
using TransactionProcessing.Shared.DomainDrivenDesign.EventSourcing;
using TransactionProcessing.Shared.EventStore.Aggregate;
using TransactionProcessing.UnionResults;

namespace CallbackHandler.BusinessLogic.Tests.RequestHandler;

using System;
using System.Threading;
using BusinessLogic.RequestHandler;
using BusinessLogic.Requests;
using BusinessLogic.Services;
using CallbackHandler.Models;
using CallbackMessageAggregate;
using Imposter.Abstractions;
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
                .ReturnsAsync(Result.Success(TestData.RecordedCallbackMessageAggregate()));

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
