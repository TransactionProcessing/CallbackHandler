/*using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog.Extensions.Logging;
using Shared.Logger;
using Shared.Middleware;

namespace CallbackHandler
{
    using Lamar.Microsoft.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection;
    using NLog;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.Aggregate;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;

    [ExcludeFromCodeCoverage]
    public class Program
    {
        public static void Main(string[] args)
        {
            Program.CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            //At this stage, we only need our hosting file for ip and ports

            FileInfo fi = new(typeof(Program).Assembly.Location);

            IConfigurationRoot config = new ConfigurationBuilder().SetBasePath(fi.Directory.FullName)
                                                                  .AddJsonFile("hosting.json", optional: true)
                                                                  .AddJsonFile("hosting.development.json", optional: true)
                                                                  .AddEnvironmentVariables().Build();

            String contentRoot = Directory.GetCurrentDirectory();
            String nlogConfigPath = Path.Combine(contentRoot, "nlog.config");

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


            IHostBuilder hostBuilder = Host.CreateDefaultBuilder(args);
            hostBuilder.UseWindowsService();
            hostBuilder.UseLamar();
            hostBuilder.ConfigureLogging(logging =>
                                         {
                                             logging.AddConsole();
                                             logging.AddNLog();

                                         });
            hostBuilder.ConfigureWebHostDefaults(webBuilder =>
                                                 {
                                                     webBuilder.UseStartup<Startup>();
                                                     webBuilder.UseConfiguration(config);
                                                     webBuilder.UseKestrel();
                                                 });
            return hostBuilder;
        }

    }
}*/

using CallbackHandler.Bootstrapper;
using HealthChecks.UI.Client;
using Lamar.Microsoft.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using Shared.EventStore.Aggregate;
using Shared.General;
using Shared.HealthChecks;
using Shared.Logger;
using Shared.Middleware;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shared.Extensions;
using HealthCheckMiddleware = Shared.HealthChecks.HealthCheckMiddleware;
using ILogger = Microsoft.Extensions.Logging.ILogger;

var builder = WebApplication.CreateBuilder(args);

// --------------------------------------
// Load hosting configuration
// --------------------------------------
var assemblyLocation = typeof(Program).Assembly.Location;
var fi = new FileInfo(assemblyLocation);

var hostingConfig = new ConfigurationBuilder()
    .SetBasePath(fi.Directory!.FullName)
    .AddJsonFile("hosting.json", optional: true)
    .AddJsonFile("hosting.development.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

builder.Configuration.AddConfiguration(hostingConfig);

// --------------------------------------
// Setup NLog
// --------------------------------------
string contentRoot = Directory.GetCurrentDirectory();
string nlogConfigPath = Path.Combine(contentRoot, "nlog.config");

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

// --------------------------------------
// Host configuration
// --------------------------------------
builder.Host
    .UseWindowsService()
    .UseLamar()
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
        logging.AddNLog();
    })
    .UseConsoleLifetime();

// --------------------------------------
// Load application configuration
// --------------------------------------
var env = builder.Environment;

builder.Configuration
    .AddJsonFile("/home/txnproc/config/appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"/home/txnproc/config/appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);

// --------------------------------------
// Lamar service configuration
// --------------------------------------
builder.Host.UseLamar((context, services) =>
{
    ConfigurationReader.Initialise(builder.Configuration);

    services.IncludeRegistry<MediatorRegistry>();
    services.IncludeRegistry<RepositoryRegistry>();
    services.IncludeRegistry<MiddlewareRegistry>();

    TypeProvider.LoadDomainEventsTypeDynamically();
});

// --------------------------------------
// Add framework services
// --------------------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

// --------------------------------------
// Build the app
// --------------------------------------
var app = builder.Build();

// --------------------------------------
// Configure middleware pipeline
// --------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

ILogger logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("CallbackHandler");
Shared.Logger.Logger.Initialise(logger);
builder.Configuration.LogConfiguration(Shared.Logger.Logger.LogWarning);
ConfigurationReader.Initialise(builder.Configuration);

app.UseMiddleware<TenantMiddleware>();
app.AddRequestLogging();
app.AddResponseLogging();
app.AddExceptionHandler();

app.UseRouting();

app.MapControllers();

app.MapHealthChecks("health", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = HealthCheckMiddleware.WriteResponse
});

app.MapHealthChecks("healthui", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.UseSwagger();
app.UseSwaggerUI();

app.Run();

namespace CallbackHandler {
    public partial class Program {
    }
}