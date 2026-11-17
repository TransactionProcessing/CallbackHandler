using System;
using System.Diagnostics.CodeAnalysis;
using MediatR;
using TransactionProcessing.UnionResults;

namespace CallbackHandler.BusinessLogic.Requests;

[ExcludeFromCodeCoverage]
public class CallbackQueries
{
    public record GetCallbackQuery(Guid CallbackId) : IRequest<Result<CallbackHandler.Models.CallbackMessage>>;
}