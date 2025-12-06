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

        String connectionString = Startup.Configuration.GetValue<String>("EventStoreSettings:ConnectionString");

        this.AddKurrentDBProjectionManagementClient(connectionString);
        this.AddKurrentDBPersistentSubscriptionsClient(connectionString);

        this.AddKurrentDBClient(connectionString);

        this.AddSingleton<IDomainEventFactory<IDomainEvent>, DomainEventFactory>();
    }
}