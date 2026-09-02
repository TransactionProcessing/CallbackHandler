using System;
using Xunit;

namespace CallbackHandler.Tests
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using Lamar;
    using Imposter.Abstractions;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    [Collection("TestCollection")]
    public class BootstrapperTests
    {
        [Fact]
        public void VerifyBootstrapperIsValid()
        {
            IWebHostEnvironmentImposter hostingEnvironment = new();
            hostingEnvironment.EnvironmentName.Getter().Returns("Development");
            hostingEnvironment.ContentRootPath.Getter().Returns("/home");
            hostingEnvironment.ApplicationName.Getter().Returns("Test Application");

            ServiceRegistry services = new();
            Startup s = new(hostingEnvironment.Instance());
            Startup.Configuration = this.SetupMemoryConfiguration();

            this.AddTestRegistrations(services, hostingEnvironment.Instance());
            s.ConfigureContainer(services);
            Startup.Container.AssertConfigurationIsValid(AssertMode.Full);
        }

        private IConfigurationRoot SetupMemoryConfiguration()
        {
            Dictionary<String, String> configuration = new Dictionary<String, String>();

            IConfigurationBuilder builder = new ConfigurationBuilder();

            configuration.Add("ConnectionStrings:HealthCheck", "HeathCheckConnString");
            configuration.Add("SecurityConfiguration:Authority", "https://127.0.0.1");
            configuration.Add("EventStoreSettings:ConnectionString", "esdb://127.0.0.1:2113");
            configuration.Add("EventStoreSettings:ConnectionName", "UnitTestConnection");
            configuration.Add("EventStoreSettings:UserName", "admin");
            configuration.Add("EventStoreSettings:Password", "changeit");
            configuration.Add("AppSettings:UseConnectionStringConfig", "false");
            configuration.Add("AppSettings:SecurityService", "http://127.0.0.1");
            configuration.Add("AppSettings:MessagingServiceApi", "http://127.0.0.1");
            configuration.Add("AppSettings:DatabaseEngine", "SqlServer");
            configuration.Add("AppSettings:TransactionProcessorApi","http://127.0.0.1");

            builder.AddInMemoryCollection(configuration);

            return builder.Build();
        }

        private void AddTestRegistrations(ServiceRegistry services,
                                                        IWebHostEnvironment hostingEnvironment)
        {
            services.AddLogging();
            DiagnosticListener diagnosticSource = new(hostingEnvironment.ApplicationName);
            services.AddSingleton<DiagnosticSource>(diagnosticSource);
            services.AddSingleton<DiagnosticListener>(diagnosticSource);
            services.AddSingleton<IWebHostEnvironment>(hostingEnvironment);
            services.AddSingleton<IHostEnvironment>(hostingEnvironment);
        }
    }
}
