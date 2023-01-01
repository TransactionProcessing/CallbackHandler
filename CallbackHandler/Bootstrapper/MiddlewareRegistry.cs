﻿namespace CallbackHandler.Bootstrapper
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Reflection;
    using Common;
    using Lamar;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Microsoft.OpenApi.Models;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using Shared.EventStore.Extensions;
    using Swashbuckle.AspNetCore.Filters;

    [ExcludeFromCodeCoverage]
    public class MiddlewareRegistry :ServiceRegistry
    {
        public MiddlewareRegistry()
        {
            this.AddHealthChecks().AddEventStore(Startup.EventStoreClientSettings,
                                                     userCredentials: Startup.EventStoreClientSettings.DefaultCredentials,
                                                     name: "Eventstore",
                                                     failureStatus: HealthStatus.Unhealthy,
                                                     tags: new string[] { "db", "eventstore" });

            this.AddSwaggerGen(c =>
                               {
                                   c.SwaggerDoc("v1", new OpenApiInfo
                                                      {
                                                          Title = "Callback Handler API",
                                                          Version = "1.0",
                                                          Description = "A REST Api to handle callback requests from external parties API's.",
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
            this.AddSwaggerExamples();

            this.AddControllers().AddNewtonsoftJson(options =>
                                                    {
                                                        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                                                        options.SerializerSettings.TypeNameHandling = TypeNameHandling.None;
                                                        options.SerializerSettings.Formatting = Formatting.Indented;
                                                        options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                                                        options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                                                    });

            Assembly assembly = this.GetType().GetTypeInfo().Assembly;
            this.AddMvcCore().AddApplicationPart(assembly).AddControllersAsServices();
        }
    }
}
