using System;
using Xunit;

namespace CallbackHandler.Tests
{
    using Lamar;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.VisualStudio.TestPlatform.TestHost;
    using Moq;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net;
    using System.Threading.Tasks;

    [Collection("TestCollection")]
    public class BootstrapperTests
    {
        [Fact]
        public async Task Application_Boots_And_All_Middleware_DI_Are_Valid()
        {
            // Arrange
            var factory = new WebApplicationFactory<CallbackHandler.Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.UseEnvironment("Development");
                    builder.ConfigureAppConfiguration((context, config) =>
                    {
                        config.Sources.Clear();

                        var dict = new Dictionary<string, string>
                        {
                            // Add necessary configuration settings here
                            { "EventStoreSettings:ConnectionString", "esdb://admin:changeit@127.0.0.1:2113?tls=true&tlsVerifyCert=false" }
                        };
                        config.AddInMemoryCollection(dict);
                    });
                });
            
            var client = factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/diagnostics/verify");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("OK", content);
        }
    }
}
