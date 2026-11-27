using CallbackHandler.Endpoints;
using CallbackHandler.Bootstrapper;
using HealthChecks.UI.Client;
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
using NLog.Extensions.Logging;
using Sentry.Extensibility;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TransactionProcessing.ApiDefaults;
using TransactionProcessing.CertificateDefaults;
using TransactionProcessing.Shared.Extensions;
using TransactionProcessing.Shared.General;
using TransactionProcessing.Shared.Logger;
using TransactionProcessing.Shared.Middleware;
using TransactionProcessing.Shared.Serialisation;
using Logger = TransactionProcessing.Shared.Logger.Logger;

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
        bool isIntegrationTest = builder.Environment.IsEnvironment("IntegrationTest");
        builder.WebHost.UseKestrel();
        if (!isIntegrationTest)
        {
            builder.WebHost.ConfigureTransactionProcessingHttps();
        }

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

        if (isIntegrationTest)
        {
            builder.WebHost.UseUrls($"{System.Uri.UriSchemeHttp}://*:5010");
        }

        ConfigurationReader.Initialise(builder.Configuration);
        ConfigureSentry(builder);

        builder.Host.ConfigureContainer<ServiceRegistry>((context, services) =>
        {
            services.AddCallbackHandlerRegistries((IConfigurationRoot)context.Configuration);
        });

        WebApplication app = builder.Build();

        LogManager.Setup().LoadConfigurationFromFile("NLog.config");
        NLog.Logger logger = LogManager.GetLogger("CallbackHandler");
        Logger.Initialise(logger, "callbackhandler");
        IConfigurationRoot appConfiguration = (IConfigurationRoot)app.Configuration;
        appConfiguration.LogConfiguration(Logger.LogWarning);
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
            ResponseWriter = TransactionProcessing.Shared.HealthChecks.HealthCheckMiddleware.WriteResponse
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
                setup.AddCallSiteHiddenAssembly(typeof(TransactionProcessing.Shared.Logger.Logger).Assembly);
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

