using EventStore.Client;
using SecurityService.Client;
using Shared.IntegrationTesting;
using Shared.Serialisation;
using TransactionProcessor.Client;

namespace CallbackHandler.IntegrationTests.Common;

public class DockerHelper : global::Shared.IntegrationTesting.TestContainers.DockerHelper
{
    public DockerHelper() {
        StringSerialiser.Initialise(new SystemTextJsonSerializer(SystemTextJsonSerializer.GetDefaultJsonSerializerOptions()));
    }

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
        this.SecurityServiceClient = new SecurityServiceClient(SecurityServiceBaseAddressResolver, httpClient, Serialise, Deserialise);
        this.TransactionProcessorClient = new TransactionProcessorClient(TransactionProcessorBaseAddressResolver, httpClient, Serialise, Deserialise);
        this.ProjectionManagementClient = new EventStoreProjectionManagementClient(ConfigureEventStoreSettings());
        this.TestHostHttpClient = new HttpClient(clientHandler);
        this.TestHostHttpClient.BaseAddress = new Uri($"http://127.0.0.1:{this.TestHostServicePort}");
    }

    String Serialise(Object arg)
    {
        return StringSerialiser.Serialise<Object>(arg, new SerialiserOptions(SerialiserPropertyFormat.SnakeCase));
    }

    Object Deserialise(String arg, Type type)
    {
        return StringSerialiser.DeserializeObject<Object>(arg, type, new SerialiserOptions(SerialiserPropertyFormat.SnakeCase));
    }
}