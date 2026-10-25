using CallbackHander.Testing;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Imposter.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CallbackHandler.BusinessLogic.Tests.Mediator;

using CallbackHandler.Bootstrapper;
using BusinessLogic.Services;
using Lamar;
using Microsoft.Extensions.DependencyInjection;
using Shared.General;
using Shared.Serialisation;

public class MediatorTests
{
    private readonly List<IBaseRequest> Requests = new List<IBaseRequest>();

    public MediatorTests()
    {
        this.Requests.Add(TestData.RecordCallbackCommand);
        this.Requests.Add(TestData.GetCallbackQuery);
    }

    [Fact]
    public async Task Mediator_Send_RequestHandled()
    {
        IWebHostEnvironmentImposter hostingEnvironment = new();
        hostingEnvironment.EnvironmentName.Getter().Returns("Development");
        hostingEnvironment.ContentRootPath.Getter().Returns("/home");
        hostingEnvironment.ApplicationName.Getter().Returns("Test Application");

        ServiceRegistry services = new();
        IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
        
        this.AddTestRegistrations(services, hostingEnvironment.Instance(), configurationRoot);
        services.AddCallbackHandlerRegistries(configurationRoot);
        Container container = new(services);
        container.AssertConfigurationIsValid(AssertMode.Full);
        StringSerialiser.Initialise(container.GetRequiredService<IStringSerialiser>());

        List<String> errors = new();
        IMediator mediator = container.GetService<IMediator>();
        foreach (IBaseRequest baseRequest in this.Requests)
        {
            try
            {
                await mediator.Send(baseRequest, TestContext.Current.CancellationToken);
            }
            catch (Exception ex)
            {
                errors.Add(ex.Message);
            }
        }

        if (errors.Any())
        {
            String errorMessage = String.Join(Environment.NewLine, errors);
            throw new Exception(errorMessage);
        }
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

            services.OverrideServices(s => { s.AddSingleton<ICallbackDomainService, DummyCallbackDomainService>(); });
        }
    }
