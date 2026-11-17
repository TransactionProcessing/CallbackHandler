using TransactionProcessing.Shared.DomainDrivenDesign.EventSourcing;
using TransactionProcessing.Shared.EventStore.Aggregate;
using TransactionProcessing.Shared.EventStore.EventStore;

namespace CallbackHandler.Bootstrapper;

using System;
using System.Diagnostics.CodeAnalysis;
using CallbackMessageAggregate;
using Lamar;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[ExcludeFromCodeCoverage]
public class RepositoryRegistry : ServiceRegistry
{
    public RepositoryRegistry(IConfiguration configuration)
    {
        this.AddTransient<IEventStoreContext, EventStoreContext>();
        this.AddSingleton<IAggregateRepository<CallbackMessageAggregate, DomainEvent>, AggregateRepository<CallbackMessageAggregate, DomainEvent>>();

        String connectionString = configuration.GetValue<String>("EventStoreSettings:ConnectionString");

        this.AddKurrentDBProjectionManagementClient(connectionString);
        this.AddKurrentDBPersistentSubscriptionsClient(connectionString);

        this.AddKurrentDBClient(connectionString);

        this.AddSingleton<IDomainEventFactory<IDomainEvent>, DomainEventFactory>();
    }
}
