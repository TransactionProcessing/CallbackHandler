using CallbackHandler.BusinessLogic.Requests;
using SimpleResults;

namespace CallbackHandler.BusinessLogic.Tests.Mediator;

using System;
using System.Threading;
using System.Threading.Tasks;
using BusinessLogic.Services;
using CallbackHandlers.Models;

public class DummyCallbackDomainService : ICallbackDomainService
{
    public async Task<Result> RecordCallback(CallbackCommands.RecordCallbackCommand command,
                                             CancellationToken cancellationToken) => Result.Success();
}