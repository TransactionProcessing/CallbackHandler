using HealthMonitoring.Client;
using KurrentDB.Client;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using TransactionProcessing.ApiDefaults;

namespace CallbackHandler.Bootstrapper;

using Common;
using Lamar;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Shared.EventStore.Extensions;
using Shared.General;
using Shared.Middleware;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;

[ExcludeFromCodeCoverage]
public class MiddlewareRegistry :ServiceRegistry
{
    public MiddlewareRegistry(IConfiguration configuration)
    {
        String connectionString = configuration.GetValue<String>("EventStoreSettings:ConnectionString");
        KurrentDBClientSettings eventStoreSettings = KurrentDBClientSettings.Create(connectionString);

        this.AddHealthChecks().AddEventStore(eventStoreSettings, userCredentials: eventStoreSettings.DefaultCredentials,
                                             name: "Eventstore", failureStatus: HealthStatus.Unhealthy, tags: new[] { "db", "eventstore" });

        this.AddControllers();

        Assembly assembly = this.GetType().GetTypeInfo().Assembly;
        this.AddMvcCore().AddApplicationPart(assembly).AddControllersAsServices();

        bool logRequests = ConfigurationReader.GetValueOrDefault<Boolean>("MiddlewareLogging", "LogRequests", true);
        bool logResponses = ConfigurationReader.GetValueOrDefault<Boolean>("MiddlewareLogging", "LogResponses", true);
        LogLevel middlewareLogLevel = ConfigurationReader.GetValueOrDefault<LogLevel>("MiddlewareLogging", "MiddlewareLogLevel", LogLevel.Warning);
        
        this.AddSingleton(new RequestResponseMiddlewareLoggingConfig(middlewareLogLevel, logRequests, logResponses));

        this.ConfigureHttpJsonOptions(jsonOptions => JsonSerializerConfiguration.ConfigureMinimalApi(jsonOptions.SerializerOptions));

        this.AddHealthMonitoringRegistration(configuration);

        this.AddApiDefaults(options =>
        {
            options.Title = "Callback Handler";
            options.Description = "API to accept callbacks from external systems";
        });
    }
}
