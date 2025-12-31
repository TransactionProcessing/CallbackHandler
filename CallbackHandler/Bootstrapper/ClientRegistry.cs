using ClientProxyBase;
using Lamar;
using Microsoft.Extensions.DependencyInjection;
using SecurityService.Client;
using System;
using System.Diagnostics.CodeAnalysis;
using Shared.General;
using TransactionProcessor.Client;

namespace CallbackHandler.Bootstrapper;

[ExcludeFromCodeCoverage]
public class ClientRegistry : ServiceRegistry
{
    public ClientRegistry() {
        this.AddHttpContextAccessor();
        this.RegisterHttpClient<ISecurityServiceClient, SecurityServiceClient>();
        this.RegisterHttpClient<ITransactionProcessorClient, TransactionProcessorClient>();

        Func<String, String> resolver(IServiceProvider container) => serviceName => ConfigurationReader.GetBaseServerUri(serviceName).OriginalString;
        this.AddSingleton<Func<String, String>>(resolver);
    }
}