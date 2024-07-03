using System;
using System.Diagnostics.CodeAnalysis;
using MediatR;

namespace CallbackHandler.BusinessLogic.Requests;

[ExcludeFromCodeCoverage]
public class CallbackQueries
{
    public record GetCallbackQuery(Guid CallbackId) : IRequest<CallbackHandlers.Models.CallbackMessage>;
}