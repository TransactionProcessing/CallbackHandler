using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using Shared.General;
using Shared.Logger;
using Shared.Middleware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JasperFx.CommandLine.Descriptions;

namespace CallbackHandler
{
    using Lamar.Microsoft.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using NLog;
    using Sentry.Extensibility;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.Aggregate;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Reflection;

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

            ConfigureLogging();


            IHostBuilder hostBuilder = Host.CreateDefaultBuilder(args);
            hostBuilder.UseWindowsService();
            hostBuilder.UseLamar();
            hostBuilder.ConfigureLogging(logging =>
                                         {
                                             logging.AddConsole();
                                             logging.AddNLog();

                                         });
            ConfigureWebHost(hostBuilder, fi.Directory.FullName, config);

            return hostBuilder;
        }

        private static void ConfigureWebHost(IHostBuilder hostBuilder,
                                             String basePath,
                                             IConfigurationRoot config) {
            hostBuilder.ConfigureWebHostDefaults(webBuilder =>
            {
                // Configure per-environment configuration and build a snapshot so we can read settings here.
                webBuilder.ConfigureAppConfiguration((context, configBuilder) =>
                {
                    IWebHostEnvironment env = context.HostingEnvironment;

                    configBuilder.SetBasePath(basePath)
                        .AddJsonFile("hosting.json", optional: true)
                        .AddJsonFile($"hosting.{env.EnvironmentName}.json", optional: true)
                        .AddJsonFile("/home/txnproc/config/appsettings.json", optional: true, reloadOnChange: true)
                        .AddJsonFile($"/home/txnproc/config/appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                        .AddEnvironmentVariables();
                                                         
                    Startup.Configuration = configBuilder.Build();
                    ConfigurationReader.Initialise(Startup.Configuration);

                    // Configure Sentry on the webBuilder using the config snapshot.
                    ConfigureSentry(env, webBuilder);
                });

                webBuilder.UseStartup<Startup>();
                webBuilder.UseConfiguration(config);
                webBuilder.UseKestrel();
            });
        }

        private static void ConfigureLogging() {
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
        }

        private static void ConfigureSentry(IWebHostEnvironment env,
                                            IWebHostBuilder webBuilder) {
            var sentrySection = Startup.Configuration.GetSection("SentryConfiguration");
            if (sentrySection.Exists())
            {
                // Replace the condition below if you intended to only enable Sentry in certain environments.
                if (env.IsDevelopment() == false)
                {
                    webBuilder.UseSentry(o =>
                    {
                        o.Dsn = Startup.Configuration["SentryConfiguration:Dsn"];
                        o.SendDefaultPii = true;
                        o.MaxRequestBodySize = RequestSize.Always;
                        o.CaptureBlockingCalls = ConfigurationReader.GetValueOrDefault("SentryConfiguration", "CaptureBlockingCalls", false);
                        o.IncludeActivityData = ConfigurationReader.GetValueOrDefault("SentryConfiguration", "IncludeActivityData", false);
                        o.Release = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";
                    });
                }
            }
        }
    }
}
