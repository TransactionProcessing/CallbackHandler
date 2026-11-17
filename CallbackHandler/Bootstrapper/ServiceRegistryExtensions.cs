using Lamar;
using Microsoft.Extensions.Configuration;
using TransactionProcessing.Shared.EventStore.Aggregate;
using TransactionProcessing.Shared.General;

namespace CallbackHandler.Bootstrapper;

public static class ServiceRegistryExtensions
{
    public static ServiceRegistry AddCallbackHandlerRegistries(this ServiceRegistry services, IConfigurationRoot configuration)
    {
        ConfigurationReader.Initialise(configuration);

        services.IncludeRegistry<MediatorRegistry>();
        services.IncludeRegistry(new RepositoryRegistry(configuration));
        services.IncludeRegistry(new MiddlewareRegistry(configuration));
        services.IncludeRegistry<ClientRegistry>();
        services.IncludeRegistry<SerialiserRegistry>();

        TypeProvider.LoadDomainEventsTypeDynamically();

        return services;
    }
}
