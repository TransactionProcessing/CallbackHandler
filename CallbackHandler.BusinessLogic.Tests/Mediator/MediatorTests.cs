using CallbackHander.Testing;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CallbackHandler.BusinessLogic.Tests.Mediator;

using BusinessLogic.Services;
using Lamar;
using Microsoft.Extensions.DependencyInjection;
using Shared.General;

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
        Mock<IWebHostEnvironment> hostingEnvironment = new();
        hostingEnvironment.Setup(he => he.EnvironmentName).Returns("Development");
        hostingEnvironment.Setup(he => he.ContentRootPath).Returns("/home");
        hostingEnvironment.Setup(he => he.ApplicationName).Returns("Test Application");

        ServiceRegistry services = new();
        Startup s = new(hostingEnvironment.Object);
        IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
        //ConfigurationReader.Initialise(configurationRoot);

        Startup.Configuration = configurationRoot;

        this.AddTestRegistrations(services, hostingEnvironment.Object);
        s.ConfigureContainer(services);
        Startup.Container.AssertConfigurationIsValid(AssertMode.Full);

        List<String> errors = new();
        IMediator mediator = Startup.Container.GetService<IMediator>();
        foreach (IBaseRequest baseRequest in this.Requests)
        {
            try
            {
                await mediator.Send(baseRequest);
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
                                          IWebHostEnvironment hostingEnvironment)
        {
            services.AddLogging();
            DiagnosticListener diagnosticSource = new(hostingEnvironment.ApplicationName);
            services.AddSingleton<DiagnosticSource>(diagnosticSource);
            services.AddSingleton<DiagnosticListener>(diagnosticSource);
            services.AddSingleton<IWebHostEnvironment>(hostingEnvironment);
            services.AddSingleton<IHostEnvironment>(hostingEnvironment);
            services.AddSingleton<IConfiguration>(Startup.Configuration);

            services.OverrideServices(s => { s.AddSingleton<ICallbackDomainService, DummyCallbackDomainService>(); });
        }
    }
