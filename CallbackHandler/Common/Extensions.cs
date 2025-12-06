using KurrentDB.Client;

namespace CallbackHandler.Common;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using EventStore.Client;
using Microsoft.Extensions.DependencyInjection;

[ExcludeFromCodeCoverage]
public static class Extensions
{
    public static IServiceCollection AddInSecureEventStoreClient(this IServiceCollection services,
                                                                 Uri address,
                                                                 Func<HttpMessageHandler>? createHttpMessageHandler = null) {
        return services.AddKurrentDBClient((Action<KurrentDBClientSettings>)(options => {
                                                                                   options.ConnectivitySettings.Address = address;
                                                                                   options.ConnectivitySettings.Insecure = true;
                                                                                   options.CreateHttpMessageHandler = createHttpMessageHandler;
                                                                               }));
    }
}