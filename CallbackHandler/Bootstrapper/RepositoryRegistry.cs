namespace CallbackHandler.Bootstrapper;

using CallbackMessageAggregate;
using Common;
using Lamar;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using Shared.EventStore.EventStore;
using Shared.EventStore.Extensions;
using Shared.General;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Net.Security;

[ExcludeFromCodeCoverage]
public class RepositoryRegistry : ServiceRegistry
{
    public RepositoryRegistry()
    {
        this.AddTransient<IEventStoreContext, EventStoreContext>();
        this.AddSingleton<IAggregateRepository<CallbackMessageAggregate, DomainEvent>, AggregateRepository<CallbackMessageAggregate, DomainEvent>>();

        String connectionString = ConfigurationReader.GetValue("EventStoreSettings", "ConnectionString");

        this.AddEventStoreProjectionManagementClient(connectionString);
        this.AddEventStorePersistentSubscriptionsClient(connectionString);

        this.AddEventStoreClient(connectionString);

        this.AddSingleton<IDomainEventFactory<IDomainEvent>, DomainEventFactory>();
    }
}