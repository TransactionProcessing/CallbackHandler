using CallbackHandler.BusinessLogic.Requests;
using TransactionProcessing.UnionResults;

namespace CallbackHandler.BusinessLogic.Tests.Mediator;

using System;
using System.Threading;
using System.Threading.Tasks;
using BusinessLogic.Services;

public class DummyCallbackDomainService : ICallbackDomainService
{
    public async Task<Result<Guid>> RecordCallback(CallbackCommands.RecordCallbackCommand command,
                                             CancellationToken cancellationToken) => Result.Success(Guid.NewGuid());
}