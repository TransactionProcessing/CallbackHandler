using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CallbackHandler
{
    using System.IO;
    using System.Net.Http;
    using System.Reflection;
    using BusinessLogic.Common;
    using BusinessLogic.RequestHandler;
    using BusinessLogic.Requests;
    using Common;
    using EventStore.Client;
    using HealthChecks.UI.Client;
    using MediatR;
    using Microsoft.AspNetCore.Diagnostics.HealthChecks;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Microsoft.Extensions.Logging;
    using Microsoft.OpenApi.Models;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using NLog.Extensions.Logging;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.Aggregate;
    using Shared.EventStore.EventStore;
    using Shared.EventStore.Extensions;
    using Shared.Extensions;
    using Shared.General;
    using Shared.Logger;
    using Swashbuckle.AspNetCore.Filters;
    using ILogger = Microsoft.Extensions.Logging.ILogger;

    public class Startup
    {
        public Startup(IWebHostEnvironment webHostEnvironment)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder().SetBasePath(webHostEnvironment.ContentRootPath)
                                                                      .AddJsonFile("/home/txnproc/config/appsettings.json", true, true)
                                                                      .AddJsonFile($"/home/txnproc/config/appsettings.{webHostEnvironment.EnvironmentName}.json", optional: true)
                                                                      .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                                                                      .AddJsonFile($"appsettings.{webHostEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                                                                      .AddEnvironmentVariables();

            Startup.Configuration = builder.Build();
            Startup.WebHostEnvironment = webHostEnvironment;
        }

        /// <summary>
        /// Gets or sets the configuration.
        /// </summary>
        /// <value>
        /// The configuration.
        /// </value>
        public static IConfigurationRoot Configuration { get; set; }

        /// <summary>
        /// Gets or sets the web host environment.
        /// </summary>
        /// <value>
        /// The web host environment.
        /// </value>
        public static IWebHostEnvironment WebHostEnvironment { get; set; }

        private static EventStoreClientSettings EventStoreClientSettings;

        private static void ConfigureEventStoreSettings(EventStoreClientSettings settings = null)
        {
            if (settings == null)
            {
                settings = new EventStoreClientSettings();
            }

            settings.CreateHttpMessageHandler = () => new SocketsHttpHandler
                                                      {
                                                          SslOptions =
                                                          {
                                                              RemoteCertificateValidationCallback = (sender,
                                                                                                     certificate,
                                                                                                     chain,
                                                                                                     errors) => true,
                                                          }
                                                      };
            settings.ConnectionName = Startup.Configuration.GetValue<String>("EventStoreSettings:ConnectionName");
            settings.ConnectivitySettings = new EventStoreClientConnectivitySettings
                                            {
                                                Address = new Uri(Startup.Configuration.GetValue<String>("EventStoreSettings:ConnectionString")),
                                            };

            settings.DefaultCredentials = new UserCredentials(Startup.Configuration.GetValue<String>("EventStoreSettings:UserName"),
                                                              Startup.Configuration.GetValue<String>("EventStoreSettings:Password"));
            Startup.EventStoreClientSettings = settings;
        }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            ConfigurationReader.Initialise(Startup.Configuration);

            Startup.ConfigureEventStoreSettings();

            this.ConfigureMiddlewareServices(services);

            services.AddTransient<IMediator, Mediator>();
            
            services.AddTransient<IEventStoreContext, EventStoreContext>();

            Dictionary<String, String[]> handlerEventTypesToSilentlyHandle = new Dictionary<String, String[]>();

            if (Startup.Configuration != null)
            {
                IConfigurationSection section = Startup.Configuration.GetSection("AppSettings:HandlerEventTypesToSilentlyHandle");

                if (section != null)
                {
                    Startup.Configuration.GetSection("AppSettings:HandlerEventTypesToSilentlyHandle").Bind(handlerEventTypesToSilentlyHandle);
                }
            }

            DomainEventTypesToSilentlyHandle eventTypesToSilentlyHandle = new DomainEventTypesToSilentlyHandle(handlerEventTypesToSilentlyHandle);
            services.AddTransient<IEventStoreContext, EventStoreContext>();
            services.AddSingleton<IAggregateRepository<CallbackMessageAggregate.CallbackMessageAggregate, DomainEventRecord.DomainEvent>, AggregateRepository<CallbackMessageAggregate.CallbackMessageAggregate, DomainEventRecord.DomainEvent>>();
            services.AddEventStoreClient(Startup.ConfigureEventStoreSettings);
            services.AddEventStoreProjectionManagerClient(Startup.ConfigureEventStoreSettings);

            // TODO: Create one domain event from each assembly here
            TypeProvider.LoadDomainEventsTypeDynamically();

            // request & notification handlers
            services.AddTransient<ServiceFactory>(context =>
                                                  {
                                                      return t => context.GetService(t);
                                                  });

            services.AddSingleton<IRequestHandler<RecordCallbackRequest, Unit>, CallbackHandlerRequestHandler>();

        }

        /// <summary>
        /// Configures the middleware services.
        /// </summary>
        /// <param name="services">The services.</param>
        private void ConfigureMiddlewareServices(IServiceCollection services)
        {
            services.AddHealthChecks().AddEventStore(Startup.EventStoreClientSettings,
                                                     userCredentials:Startup.EventStoreClientSettings.DefaultCredentials,
                                                     name:"Eventstore",
                                                     failureStatus:HealthStatus.Unhealthy,
                                                     tags:new string[] {"db", "eventstore"});
                    
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Estate Management API",
                    Version = "1.0",
                    Description = "A REST Api to manage all aspects of an estate (merchants, operators and contracts).",
                    Contact = new OpenApiContact
                    {
                        Name = "Stuart Ferguson",
                        Email = "golfhandicapping@btinternet.com"
                    }
                });
                // add a custom operation filter which sets default values
                c.OperationFilter<SwaggerDefaultValues>();
                c.ExampleFilters();

                //Locate the XML files being generated by ASP.NET...
                var directory = new DirectoryInfo(AppContext.BaseDirectory);
                var xmlFiles = directory.GetFiles("*.xml");

                //... and tell Swagger to use those XML comments.
                foreach (FileInfo fileInfo in xmlFiles)
                {
                    c.IncludeXmlComments(fileInfo.FullName);
                }

            });
            
            services.AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                options.SerializerSettings.TypeNameHandling = TypeNameHandling.None;
                options.SerializerSettings.Formatting = Formatting.Indented;
                options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            });

            Assembly assembly = this.GetType().GetTypeInfo().Assembly;
            services.AddMvcCore().AddApplicationPart(assembly).AddControllersAsServices();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            String nlogConfigFilename = "nlog.config";

            if (env.IsDevelopment())
            {
                nlogConfigFilename = $"nlog.{env.EnvironmentName}.config";
                app.UseDeveloperExceptionPage();
            }

            loggerFactory.ConfigureNLog(Path.Combine(env.ContentRootPath, nlogConfigFilename));
            loggerFactory.AddNLog();

            ILogger logger = loggerFactory.CreateLogger("CallbackHandler");

            Logger.Initialise(logger);

            Action<String> loggerAction = message =>
            {
                Logger.LogInformation(message);
            };
            Startup.Configuration.LogConfiguration(loggerAction);

            ConfigurationReader.Initialise(Startup.Configuration);

            app.AddRequestLogging();
            app.AddResponseLogging();
            app.AddExceptionHandler();

            app.UseRouting();
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("health", new HealthCheckOptions()
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
            });
            app.UseSwagger();

            app.UseSwaggerUI();
            
        }
    }
}
