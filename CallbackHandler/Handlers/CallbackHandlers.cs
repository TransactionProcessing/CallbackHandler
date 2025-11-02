using CallbackHandler.BusinessLogic.Requests;
using CallbackHandler.DataTransferObjects;
using CallbackHandlers.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Shared.Results;
using SimpleResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using Shared.Results.Web;

namespace CallbackHandler.Handlers;

public static class CallbackHandlers
{
    public static async Task<IResult> RecordCallback(Deposit depositCallback,
                                                     IMediator mediator,
                                                     CancellationToken cancellationToken)
    {
        Guid callbackId = Guid.NewGuid();

        CallbackCommands.RecordCallbackCommand request = new(
            callbackId,
            JsonConvert.SerializeObject(depositCallback),
            new[] { "TransactionProcessor" },
            MessageFormat.JSON,
            depositCallback.GetType().ToString(),
            depositCallback.Reference);

        Result result = await mediator.Send(request, cancellationToken);

        return  ResponseFactory.FromResult<CallbackResponse>(result, (r) => new CallbackResponse {
            CallbackId = callbackId
        });
    }

    public static async Task<IResult> GetCallback(Guid callbackId,
                                                  IMediator mediator,
                                                  CancellationToken cancellationToken)
    {
        CallbackQueries.GetCallbackQuery query = new(callbackId);
        Result<global::CallbackHandlers.Models.CallbackMessage> result = await mediator.Send(query, cancellationToken);

        return ResponseFactory.FromResult(result, (r) => new DataTransferObjects.CallbackMessage
        {
            Reference = r.Reference,
            TypeString = r.TypeString,
            Message = r.Message
        });
    }
}