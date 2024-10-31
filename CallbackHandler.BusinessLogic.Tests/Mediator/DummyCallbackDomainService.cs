using SimpleResults;

namespace CallbackHandler.BusinessLogic.Tests.Mediator;

using System;
using System.Threading;
using System.Threading.Tasks;
using BusinessLogic.Services;
using CallbackHandlers.Models;

public class DummyCallbackDomainService : ICallbackDomainService
{
    public async Task<Result> RecordCallback(Guid callbackId,
                                             String typeString,
                                             MessageFormat messageFormat,
                                             String callbackMessage,
                                             String reference,
                                             String[] destinations,
                                             CancellationToken cancellationToken) => Result.Success();
}