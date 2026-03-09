using SimpleResults;

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

        this.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CallbackHandlerRequestHandler).Assembly));
        this.AddSingleton<ICallbackDomainService, CallbackDomainService>();
    }
}