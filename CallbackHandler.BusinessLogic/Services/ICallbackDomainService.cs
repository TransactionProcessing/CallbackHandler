using Azure.Core;
using CallbackHandlers.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SimpleResults;

namespace CallbackHandler.BusinessLogic.Services
{
    public interface ICallbackDomainService
    {
        Task<Result> RecordCallback(Guid callbackId,
                            String typeString,
                            MessageFormat messageFormat,
                            String callbackMessage,
                            String reference,
                            String[] destinations,
                            CancellationToken cancellationToken);
    }
}
