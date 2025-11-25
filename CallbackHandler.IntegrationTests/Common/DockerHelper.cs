using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Bson;

namespace CallbackHandler.IntegrationTests.Common;

public class DockerHelper : global::Shared.IntegrationTesting.TestContainers.DockerHelper
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