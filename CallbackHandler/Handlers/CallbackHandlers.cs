using CallbackHandler.BusinessLogic.Requests;
using CallbackHandler.DataTransferObjects;
using CallbackHandler.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;
using TransactionProcessing.Shared.Serialisation;
using TransactionProcessing.UnionResults;

namespace CallbackHandler.Handlers;

public static class CallbackHandlers
{
    public static async Task<object> RecordCallback(Deposit depositCallback,
                                                    IMediator mediator,
                                                    CancellationToken cancellationToken)
    {
        CallbackCommands.RecordCallbackCommand request = new(
            StringSerialiser.Serialise(depositCallback),
            new[] { "TransactionProcessor" },
            MessageFormat.JSON,
            depositCallback.GetType().ToString(),
            depositCallback.Reference);

        Result<Guid> result = await mediator.Send(request, cancellationToken);

        if (result.Value is Success<Guid> success)
            return TypedResults.Created((String)null, new CallbackResponse { CallbackId = success.Value });

        return result;
    }

    public static async Task<object> GetCallback(Guid callbackId,
                                                 IMediator mediator,
                                                 CancellationToken cancellationToken)
    {
        CallbackQueries.GetCallbackQuery query = new(callbackId);
        Result<CallbackHandler.Models.CallbackMessage> result = await mediator.Send(query, cancellationToken);

        if (result.Value is Success<Models.CallbackMessage> success)
        {
            return TypedResults.Created((String)null, new DataTransferObjects.CallbackMessage
            {
                Reference = success.Value.Reference,
                TypeString = success.Value.TypeString,
                Message = success.Value.Message
            });
        }

        return result;
    }
}
