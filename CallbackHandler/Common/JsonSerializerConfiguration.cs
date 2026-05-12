using System;
using System.Text.Json;
using Shared.Serialisation;

namespace CallbackHandler.Common;

public static class JsonSerializerConfiguration
{
    public static void ConfigureMinimalApi(JsonSerializerOptions serializerOptions)
    {
        var defaultOptions = SystemTextJsonSerializer.GetDefaultJsonSerializerOptions();
        serializerOptions.PropertyNamingPolicy = defaultOptions.PropertyNamingPolicy;
        serializerOptions.DictionaryKeyPolicy = defaultOptions.DictionaryKeyPolicy;
        serializerOptions.ReferenceHandler = defaultOptions.ReferenceHandler;
        serializerOptions.WriteIndented = defaultOptions.WriteIndented;
        serializerOptions.Converters.Add(new DateTimeSpaceConverter());
    }
}