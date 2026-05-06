using ClientProxyBase;
using Lamar;
using Microsoft.Extensions.DependencyInjection;
using SecurityService.Client;
using Shared.General;
using Shared.Serialisation;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using NLog;
using Shared.DomainDrivenDesign.EventSourcing;
using TransactionProcessor.Client;

namespace CallbackHandler.Bootstrapper;

[ExcludeFromCodeCoverage]
public class ClientRegistry : ServiceRegistry
{
    public ClientRegistry() {
        this.AddHttpContextAccessor();
        this.RegisterHttpClient<ISecurityServiceClient, SecurityServiceClient>();
        this.RegisterHttpClient<ITransactionProcessorClient, TransactionProcessorClient>();

        Func<String, String> resolver(IServiceProvider container) => serviceName => ConfigurationReader.GetBaseServerUri(serviceName).OriginalString;
        this.AddSingleton<Func<String, String>>(resolver);
    }
}

[ExcludeFromCodeCoverage]
public class SerialiserRegistry : ServiceRegistry
{
    public SerialiserRegistry()
    {
        this.AddSingleton<IStringSerialiser, SystemTextJsonSerializer>();
        this.AddSingleton<Func<Object, String>>(_ => obj => StringSerialiser.Serialise(obj));
        this.AddSingleton<Func<String, Type, Object>>(_ => (str,type) => StringSerialiser.DeserializeObject<Object>(str, type));
        this.AddSingleton(SystemTextJsonSerializer.GetDefaultJsonSerializerOptions());
    }
}