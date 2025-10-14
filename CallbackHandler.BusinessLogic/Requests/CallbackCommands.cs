using System;
using System.Diagnostics.CodeAnalysis;
using CallbackHandlers.Models;
using MediatR;
using SimpleResults;

namespace CallbackHandler.BusinessLogic.Requests;

[ExcludeFromCodeCoverage]
public class CallbackCommands
{
    public record RecordCallbackCommand(
        Guid CallbackId,
        String CallbackMessage,
        String[] Destinations,
        MessageFormat MessageFormat,
        String TypeString,
        String Reference) : IRequest<Result>;
}