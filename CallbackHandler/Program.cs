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

                                                     IConfigurationSection isSentryConfigured = config.GetSection("SentryConfiguration");
                                                     if (isSentryConfigured.Exists()) {
                                                         webBuilder.Configure((context,
                                                                               app) => {
                                                             if (context.HostingEnvironment.IsDevelopment() == false) {
                                                                 Version version = Assembly.GetExecutingAssembly().GetName().Version;

                                                                 webBuilder.UseSentry(o => {

                                                                     o.Dsn = ConfigurationReader.GetValueFromSection<String>("SentryConfiguration", "Dsn");
                                                                     o.SendDefaultPii = true; // required for body + user data
                                                                     o.MaxRequestBodySize = RequestSize.Always;
                                                                     o.CaptureBlockingCalls = true;
                                                                     //o.CaptureFailedRequests = true;
                                                                     o.Release = version != null ? version.ToString() : "unknown";

                                                                 });
                                                             }
                                                         });

                                                     }
                                                 });

            return hostBuilder;
        }

    }
}
