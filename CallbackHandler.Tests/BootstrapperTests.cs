using System;
using Xunit;

namespace CallbackHandler.Tests
{
    using CallbackHandler.Bootstrapper;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Lamar;
    using Imposter.Abstractions;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Shared.Serialisation;

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
            IConfigurationRoot configuration = this.SetupMemoryConfiguration();

            this.AddTestRegistrations(services, hostingEnvironment.Instance(), configuration);
            services.AddCallbackHandlerRegistries(configuration);
            Container container = new(services);
            container.AssertConfigurationIsValid(AssertMode.Full);
            StringSerialiser.Initialise(container.GetRequiredService<IStringSerialiser>());
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
                                         IWebHostEnvironment hostingEnvironment,
                                         IConfiguration configuration)
        {
            services.AddLogging();
            DiagnosticListener diagnosticSource = new(hostingEnvironment.ApplicationName);
            services.AddSingleton<DiagnosticSource>(diagnosticSource);
            services.AddSingleton<DiagnosticListener>(diagnosticSource);
            services.AddSingleton<IWebHostEnvironment>(hostingEnvironment);
            services.AddSingleton<IHostEnvironment>(hostingEnvironment);
            services.AddSingleton<IConfiguration>(configuration);
        }
    }
}
