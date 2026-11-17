using CallbackHandler.Models;
using System;
using System.Diagnostics.CodeAnalysis;
using MediatR;
using TransactionProcessing.UnionResults;

namespace CallbackHandler.BusinessLogic.Requests;

[ExcludeFromCodeCoverage]
public class CallbackCommands
{
    public record RecordCallbackCommand(
        String CallbackMessage,
        String[] Destinations,
        MessageFormat MessageFormat,
        String TypeString,
        String Reference) : IRequest<Result<Guid>>;
}