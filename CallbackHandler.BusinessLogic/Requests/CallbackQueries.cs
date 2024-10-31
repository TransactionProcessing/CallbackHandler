using System;
using System.Diagnostics.CodeAnalysis;
using MediatR;
using SimpleResults;

namespace CallbackHandler.BusinessLogic.Requests;

[ExcludeFromCodeCoverage]
public class CallbackQueries
{
    public record GetCallbackQuery(Guid CallbackId) : IRequest<Result<CallbackHandlers.Models.CallbackMessage>>;
}