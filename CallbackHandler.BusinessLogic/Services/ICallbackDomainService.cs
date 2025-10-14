using Azure.Core;
using CallbackHandlers.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CallbackHandler.BusinessLogic.Requests;
using SimpleResults;

namespace CallbackHandler.BusinessLogic.Services;

public interface ICallbackDomainService
{
    Task<Result> RecordCallback(CallbackCommands.RecordCallbackCommand command,
                                CancellationToken cancellationToken);
}
