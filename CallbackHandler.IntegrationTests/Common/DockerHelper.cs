using Ductus.FluentDocker.Services;
using NLog;
using Reqnroll;
using Shared.IntegrationTesting;
using Shared.Logger;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CallbackHandler.DataTransferObjects;
using Ductus.FluentDocker.Services.Extensions;
using Newtonsoft.Json.Bson;

namespace CallbackHandler.IntegrationTests.Common
{
    public class DockerHelper : global::Shared.IntegrationTesting.DockerHelper
    {
        public override async Task CreateSubscriptions()
        {    
            // Nothing to do here
        }

        public Int32 GetCallbackHandlerPort()
        {
            return this.CallbackHandlerPort;
        }
    }

    [Binding]
    [Scope(Tag = "base")]
    public class GenericSteps
    {
        private readonly ScenarioContext ScenarioContext;

        private readonly TestingContext TestingContext;

        public GenericSteps(ScenarioContext scenarioContext,
            TestingContext testingContext)
        {
            this.ScenarioContext = scenarioContext;
            this.TestingContext = testingContext;
        }

        [BeforeScenario()]
        public async Task StartSystem()
        {
            // Initialise a logger
            String scenarioName = this.ScenarioContext.ScenarioInfo.Title.Replace(" ", "");
            NlogLogger logger = new();
            logger.Initialise(LogManager.GetLogger(scenarioName), scenarioName);
            LogManager.AddHiddenAssembly(typeof(NlogLogger).Assembly);

            DockerServices dockerServices = DockerServices.CallbackHandler | DockerServices.EventStore | DockerServices.SqlServer;

            this.TestingContext.DockerHelper = new DockerHelper();
            this.TestingContext.DockerHelper.Logger = logger;
            this.TestingContext.Logger = logger;
            this.TestingContext.DockerHelper.RequiredDockerServices = dockerServices;
            this.TestingContext.Logger.LogInformation("About to Start Global Setup");

            await Setup.GlobalSetup(this.TestingContext.DockerHelper);

            this.TestingContext.DockerHelper.SqlServerContainer = Setup.DatabaseServerContainer;
            this.TestingContext.DockerHelper.SqlServerNetwork = Setup.DatabaseServerNetwork;
            this.TestingContext.DockerHelper.DockerCredentials = Setup.DockerCredentials;
            this.TestingContext.DockerHelper.SqlCredentials = Setup.SqlCredentials;
            this.TestingContext.DockerHelper.SqlServerContainerName = "sharedsqlserver";

            this.TestingContext.DockerHelper.SetImageDetails(ContainerType.CallbackHandler, ("callbackhandler", false));

            this.TestingContext.Logger = logger;
            this.TestingContext.Logger.LogInformation($"About to Start Containers for Scenario Run [{this.ScenarioContext.ScenarioInfo.Title}]");
            await this.TestingContext.DockerHelper.StartContainersForScenarioRun(scenarioName, dockerServices).ConfigureAwait(false);
            this.TestingContext.Logger.LogInformation("Containers for Scenario Run Started");
        }

        [AfterScenario()]
        public async Task StopSystem()
        {
            DockerServices dockerSharedServices = DockerServices.SqlServer;

            this.TestingContext.Logger.LogInformation("About to Stop Containers for Scenario Run");
            await this.TestingContext.DockerHelper.StopContainersForScenarioRun(dockerSharedServices).ConfigureAwait(false);
            this.TestingContext.Logger.LogInformation($"Containers for Scenario Run [{this.ScenarioContext.ScenarioInfo.Title}] Stopped Status is [{this.ScenarioContext.ScenarioExecutionStatus}]");

        }

    }

    public class TestingContext
    {
        public DockerHelper DockerHelper { get; set; }
        public NlogLogger Logger { get; set; }

        public List<Deposit> Deposits { get; set; }

        public Dictionary<Guid, String> SentCallbacks { get; set; }

        public TestingContext()
        {
            this.SentCallbacks = new Dictionary<Guid, string>();
        }
    }

    [Binding]
    public class Setup
    {
        public static IContainerService DatabaseServerContainer;
        public static INetworkService DatabaseServerNetwork;
        public static (String usename, String password) SqlCredentials = ("sa", "thisisalongpassword123!");
        public static (String url, String username, String password) DockerCredentials = ("https://www.docker.com", "stuartferguson", "Sc0tland");

        static object padLock = new object(); // Object to lock on

        public static async Task GlobalSetup(DockerHelper dockerHelper)
        {
            ShouldlyConfiguration.DefaultTaskTimeout = TimeSpan.FromMinutes(1);
            dockerHelper.SqlCredentials = Setup.SqlCredentials;
            dockerHelper.DockerCredentials = Setup.DockerCredentials;
            dockerHelper.SqlServerContainerName = "sharedsqlserver";

            lock (Setup.padLock)
            {
                Setup.DatabaseServerNetwork = dockerHelper.SetupTestNetwork("sharednetwork");

                dockerHelper.Logger.LogInformation("in start SetupSqlServerContainer");
                Setup.DatabaseServerContainer = dockerHelper.SetupSqlServerContainer(Setup.DatabaseServerNetwork).Result;
            }
        }

        public static String GetConnectionString(String databaseName)
        {
            return $"server={Setup.DatabaseServerContainer.Name};database={databaseName};user id={Setup.SqlCredentials.usename};password={Setup.SqlCredentials.password}&Encrypt=false";
        }

        public static String GetLocalConnectionString(String databaseName)
        {
            Int32 databaseHostPort = Setup.DatabaseServerContainer.ToHostExposedEndpoint("1433/tcp").Port;

            return $"server=localhost,{databaseHostPort};database={databaseName};user id={Setup.SqlCredentials.usename};password={Setup.SqlCredentials.password}&Encrypt=false";
        }
    }
}
