using Ductus.FluentDocker.Services;
using Ductus.FluentDocker.Services.Extensions;
using Reqnroll;
using Shared.IntegrationTesting;
using Shouldly;

namespace CallbackHandler.IntegrationTests.Common;

[Binding]
public class Setup
{
    public static (String usename, String password) SqlCredentials = ("sa", "thisisalongpassword123!");
    public static (String url, String username, String password) DockerCredentials = ("https://www.docker.com", "stuartferguson", "Sc0tland");
    
    public static async Task GlobalSetup(DockerHelper dockerHelper)
    {
        ShouldlyConfiguration.DefaultTaskTimeout = TimeSpan.FromMinutes(1);
    }
}