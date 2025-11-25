using CallbackHandler.DataTransferObjects;
using Shared.Logger;

namespace CallbackHandler.IntegrationTests.Common;

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