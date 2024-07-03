namespace CallbackHandler.Bootstrapper;

using BusinessLogic.RequestHandler;
using BusinessLogic.Requests;
using BusinessLogic.Services;
using Lamar;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class MediatorRegistry : ServiceRegistry
{
    public MediatorRegistry()
    {
        this.AddTransient<IMediator, Mediator>();
        
        // request & notification handlers
        this.AddSingleton<IRequestHandler<CallbackCommands.RecordCallbackRequest>, CallbackHandlerRequestHandler>();
        this.AddSingleton<IRequestHandler<CallbackQueries.GetCallbackQuery, CallbackHandlers.Models.CallbackMessage>, CallbackHandlerRequestHandler>();
        this.AddSingleton<ICallbackDomainService, CallbackDomainService>();
    }
}