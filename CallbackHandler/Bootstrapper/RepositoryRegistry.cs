namespace CallbackHandler.Bootstrapper;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Net.Security;
using CallbackMessageAggregate;
using Common;
using Lamar;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using Shared.EventStore.EventStore;
using Shared.EventStore.Extensions;

[ExcludeFromCodeCoverage]
public class RepositoryRegistry : ServiceRegistry
{
    public RepositoryRegistry()
    {
        this.AddTransient<IEventStoreContext, EventStoreContext>();
        this.AddSingleton<IAggregateRepository<CallbackMessageAggregate, DomainEvent>, AggregateRepository<CallbackMessageAggregate, DomainEvent>>();
            
        Boolean insecureES = Startup.Configuration.GetValue<Boolean>("EventStoreSettings:Insecure");

        Func<SocketsHttpHandler> CreateHttpMessageHandler = () => new SocketsHttpHandler
                                                                  {
                                                                      SslOptions = new SslClientAuthenticationOptions
                                                                                   {
                                                                                       RemoteCertificateValidationCallback = (sender,
                                                                                           certificate,
                                                                                           chain,
                                                                                           errors) => {
                                                                                           return true;
                                                                                       }
                                                                                   }
                                                                  };

        this.AddEventStoreProjectionManagementClient(Startup.ConfigureEventStoreSettings);

        if (insecureES)
        {
            this.AddInSecureEventStoreClient(Startup.EventStoreClientSettings.ConnectivitySettings.Address, CreateHttpMessageHandler);
        }
        else
        {
            this.AddEventStoreClient(Startup.EventStoreClientSettings.ConnectivitySettings.Address, CreateHttpMessageHandler);
        }
    }
}