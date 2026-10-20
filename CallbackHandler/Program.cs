using CallbackHandler.Endpoints;
using CallbackHandler.Bootstrapper;
using HealthChecks.UI.Client;
using JasperFx.CommandLine.Descriptions;
using Lamar;
using Lamar.Microsoft.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using Sentry;
using NLog.Extensions.Logging;
using Sentry.Extensibility;
using Sentry.Protocol;
using Shared.Extensions;
using Shared.General;
using Shared.Logger;
using Shared.Middleware;
using Shared.Serialisation;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TransactionProcessing.ApiDefaults;

namespace CallbackHandler;

[ExcludeFromCodeCoverage]
public static class Program
{
    public static void Main(string[] args)
    {
        FileInfo assemblyFile = new(typeof(Program).Assembly.Location);
        string basePath = assemblyFile.Directory!.FullName;

        ConfigureLogging();

        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        builder.Host.UseWindowsService();
        builder.Host.UseLamar();

        builder.Logging.AddConsole();
        builder.Logging.AddNLog();

        IConfigurationRoot hostConfiguration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("hosting.json", optional: true)
            .AddJsonFile("hosting.development.json", optional: true)
            .AddEnvironmentVariables()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["HealthMonitoring:Service:Version"] = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "0.0.0.0"
            })
            .Build();

        builder.Configuration.AddConfiguration(hostConfiguration);
        builder.Configuration.SetBasePath(basePath)
            .AddJsonFile("hosting.json", optional: true)
            .AddJsonFile($"hosting.{builder.Environment.EnvironmentName}.json", optional: true)
            .AddJsonFile("/home/txnproc/config/appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"/home/txnproc/config/appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .AddJsonFile("/home/txnproc/config/appsettings.local.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();

        ConfigurationReader.Initialise(builder.Configuration);
        ConfigureSentry(builder);

        builder.Host.ConfigureContainer<ServiceRegistry>((context, services) =>
        {
            services.AddCallbackHandlerRegistries((IConfigurationRoot)context.Configuration);
        });

        WebApplication app = builder.Build();

        Microsoft.Extensions.Logging.ILogger logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("CallbackHandler");
        Shared.Logger.Logger.Initialise(logger);
        IConfigurationRoot appConfiguration = (IConfigurationRoot)app.Configuration;
        appConfiguration.LogConfiguration(Shared.Logger.Logger.LogWarning);
        ConfigurationReader.Initialise(appConfiguration);

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseMiddleware<TenantMiddleware>();
        app.AddRequestResponseLogging();
        app.AddExceptionHandler();
        app.UseRouting();

        app.MapCallbackEndpoints();
        app.MapHealthChecks("health", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = Shared.HealthChecks.HealthCheckMiddleware.WriteResponse
        });
        app.MapHealthChecks("healthui", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });
        app.UseApiDefaults();

        StringSerialiser.Initialise(app.Services.GetRequiredService<IStringSerialiser>());

        app.Run();
    }

    private static void ConfigureLogging()
    {
        string nlogConfigPath = Path.Combine(Directory.GetCurrentDirectory(), "nlog.config");

        LogManager.Setup(b =>
        {
            b.SetupLogFactory(setup =>
            {
                setup.AddCallSiteHiddenAssembly(typeof(NlogLogger).Assembly);
                setup.AddCallSiteHiddenAssembly(typeof(Shared.Logger.Logger).Assembly);
                setup.AddCallSiteHiddenAssembly(typeof(TenantMiddleware).Assembly);
            });
            b.LoadConfigurationFromFile(nlogConfigPath);
        });
    }

    private static void ConfigureSentry(WebApplicationBuilder builder)
    {
        IConfiguration configuration = builder.Configuration;
        IConfigurationSection sentrySection = configuration.GetSection("SentryConfiguration");
        if (sentrySection.Exists() && !builder.Environment.IsDevelopment())
        {
            builder.WebHost.UseSentry(options =>
            {
                options.Dsn = configuration["SentryConfiguration:Dsn"];
                options.SendDefaultPii = true;
                options.MaxRequestBodySize = RequestSize.Always;
                options.CaptureBlockingCalls = ConfigurationReader.GetValueOrDefault("SentryConfiguration", "CaptureBlockingCalls", false);
                options.IncludeActivityData = ConfigurationReader.GetValueOrDefault("SentryConfiguration", "IncludeActivityData", false);
                options.Release = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";
            });
        }
    }
}
