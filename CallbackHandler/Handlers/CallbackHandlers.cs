using System;
using System.Threading;
using System.Threading.Tasks;
using CallbackHandler.BusinessLogic.Requests;
using CallbackHandler.DataTransferObjects;
using CallbackHandlers.Models;
using MediatR;
using Newtonsoft.Json;
using Shared.Results;
using SimpleResults;

namespace CallbackHandler.Handlers;

public static class CallbackHandlers
{
    public static async Task<Result<Guid>> RecordCallback(Deposit depositCallback,
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

        if (result.IsFailed) {
            return ResultHelpers.CreateFailure(result);
        }

        return Result.Success(callbackId);
    }

    public static async Task<Result<DataTransferObjects.CallbackMessage>> GetCallback(Guid callbackId,
                                                                                           IMediator mediator,
                                                                                           CancellationToken cancellationToken)
    {
        CallbackQueries.GetCallbackQuery query = new(callbackId);
        Result<global::CallbackHandlers.Models.CallbackMessage> getResult = await mediator.Send(query, cancellationToken);

        if (getResult.IsFailed) {
            return ResultHelpers.CreateFailure(getResult);
        }

        Result<DataTransferObjects.CallbackMessage> result = Result.Success(new DataTransferObjects.CallbackMessage
        {
            Reference = getResult.Data.Reference,
            TypeString = getResult.Data.TypeString,
            Message = getResult.Data.Message
        });

        return result;
    }
}