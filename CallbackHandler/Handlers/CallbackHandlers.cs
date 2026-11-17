using CallbackHandler.BusinessLogic.Requests;
using CallbackHandler.DataTransferObjects;
using CallbackHandler.Models;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using TransactionProcessing.Shared.Serialisation;
using TransactionProcessing.UnionResults;

namespace CallbackHandler.Handlers;

public static class CallbackHandlers
{
    public static async Task<Result<CallbackResponse>> RecordCallback(Deposit depositCallback,
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

        return result.Match(
            callbackId => Result<CallbackResponse>.Created(new CallbackResponse() { CallbackId = callbackId }),
            (callbackId, location) => Result<CallbackResponse>.Created(new CallbackResponse { CallbackId = callbackId }, location),
            Result<CallbackResponse>.NoContent,
            Result<CallbackResponse>.Fail,
            Result<CallbackResponse>.Fail);


    }

    public static async Task<Result<DataTransferObjects.CallbackMessage>> GetCallback(Guid callbackId,
                                                  IMediator mediator,
                                                  CancellationToken cancellationToken)
    {
        CallbackQueries.GetCallbackQuery query = new(callbackId);
        Result<CallbackHandler.Models.CallbackMessage> result = await mediator.Send(query, cancellationToken);

        //return ResponseFactory.FromResult(result, (r) => new DataTransferObjects.CallbackMessage
        //{
        //    Reference = r.Reference,
        //    TypeString = r.TypeString,
        //    Message = r.Message
        //});
        return result.Match(
            r => Result<DataTransferObjects.CallbackMessage>.Created(new DataTransferObjects.CallbackMessage() {
                                                                                                    Reference = r.Reference,
                                                                                                        TypeString = r.TypeString,
                                                                                                    Message = r.Message
            }),
            (r ,location)=> Result<DataTransferObjects.CallbackMessage>.Created(new DataTransferObjects.CallbackMessage() {
                                                                                                                        Reference = r.Reference,
                                                                                                                        TypeString = r.TypeString,
                                                                                                                        Message = r.Message
                                                                                                                    }),
            Result<DataTransferObjects.CallbackMessage>.NoContent,
            Result<DataTransferObjects.CallbackMessage>.Fail,
            Result<DataTransferObjects.CallbackMessage>.Fail);
    }
}