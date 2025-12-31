using EventStore.Client;
using Newtonsoft.Json.Bson;
using SecurityService.Client;
using Shared.IntegrationTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TransactionProcessor.Client;

namespace CallbackHandler.IntegrationTests.Common;

public class DockerHelper : global::Shared.IntegrationTesting.TestContainers.DockerHelper
{
    public ISecurityServiceClient SecurityServiceClient;
    public ITransactionProcessorClient TransactionProcessorClient;
    public EventStoreProjectionManagementClient ProjectionManagementClient;
    public HttpClient TestHostHttpClient;
    public override async Task CreateSubscriptions()
    {
        List<(String streamName, String groupName, Int32 maxRetries)> subscriptions = new();
        subscriptions.AddRange(TransactionProcessor.IntegrationTesting.Helpers.SubscriptionsHelper.GetSubscriptions());
        foreach ((String streamName, String groupName, Int32 maxRetries) subscription in subscriptions)
        {
            var x = subscription;
            x.maxRetries = 2;
            await this.CreatePersistentSubscription(x);
        }
    }

    protected override List<String> GetRequiredProjections()
    {
        List<String> requiredProjections = new List<String>();

        requiredProjections.Add("CallbackHandlerEnricher.js");
        requiredProjections.Add("EstateAggregator.js");
        requiredProjections.Add("MerchantAggregator.js");
        requiredProjections.Add("MerchantBalanceCalculator.js");
        requiredProjections.Add("MerchantBalanceProjection.js");

        return requiredProjections;
    }

    public Int32 GetCallbackHandlerPort()
    {
        return this.CallbackHandlerPort;
    }

    public override async Task StartContainersForScenarioRun(String scenarioName,
                                                       DockerServices dockerServices) {
        await base.StartContainersForScenarioRun(scenarioName, dockerServices);

        // Setup the base address resolvers
        String SecurityServiceBaseAddressResolver(String api) => $"https://127.0.0.1:{this.SecurityServicePort}";
        String TransactionProcessorBaseAddressResolver(String api) => $"http://127.0.0.1:{this.TransactionProcessorPort}";

        HttpClientHandler clientHandler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message,
                                                         certificate2,
                                                         arg3,
                                                         arg4) =>
            {
                return true;
            }
        };
        HttpClient httpClient = new HttpClient(clientHandler);
        this.SecurityServiceClient = new SecurityServiceClient(SecurityServiceBaseAddressResolver, httpClient);
        this.TransactionProcessorClient = new TransactionProcessorClient(TransactionProcessorBaseAddressResolver, httpClient);
        this.ProjectionManagementClient = new EventStoreProjectionManagementClient(ConfigureEventStoreSettings());
        this.TestHostHttpClient = new HttpClient(clientHandler);
        this.TestHostHttpClient.BaseAddress = new Uri($"http://127.0.0.1:{this.TestHostServicePort}");
    }
}