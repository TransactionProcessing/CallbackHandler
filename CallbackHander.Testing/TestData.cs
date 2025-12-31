using CallbackHandlers.Models;
using System;
using System.Collections.Generic;
using Xunit;

namespace CallbackHander.Testing;

using CallbackHandler.BusinessLogic.Requests;
using CallbackHandler.CallbackMessageAggregate;
using SecurityService.DataTransferObjects.Responses;

public class TestData
{
    public static Guid CallbackId = Guid.Parse("E82DF060-5717-4F79-B99B-E14C702C8F0E");

    public static Int32 MessageFormat = 1;

    public static String TypeString = "TestTypeString";

    public static String CallbackMessage = "Callback message";

    public static String[] Destinations = {"A", "B"};

    public static String Reference = "640E863C23E244BDB9717C92733FFD4C-9D20A3961CF645EDAA7BDD436318BA29";
    public static Guid EstateReference = Guid.Parse("640E863C-23E2-44BD-B971-7C92733FFD4C");
    public static Guid MerchantReference = Guid.Parse("9D20A396-1CF6-45ED-AA7B-DD436318BA29");
    public static TokenResponse TokenResponse()
    {
        return SecurityService.DataTransferObjects.Responses.TokenResponse.Create("AccessToken", string.Empty, 100);
    }

    public static IReadOnlyDictionary<String, String> DefaultAppSettings =>
        new Dictionary<String, String>
        {
            ["AppSettings:ClientId"] = "clientId",
            ["AppSettings:ClientSecret"] = "clientSecret",
            ["AppSettings:UseConnectionStringConfig"] = "false",
            ["EventStoreSettings:ConnectionString"] = "esdb://127.0.0.1:2113",
            ["SecurityConfiguration:Authority"] = "https://127.0.0.1",
            ["AppSettings:EstateManagementApi"] = "http://127.0.0.1",
            ["AppSettings:SecurityService"] = "http://127.0.0.1",
            ["AppSettings:TransactionProcessorApi"] = "http://127.0.0.1",
            ["AppSettings:ContractProductFeeCacheExpiryInHours"] = "",
            ["AppSettings:ContractProductFeeCacheEnabled"] = "",
            ["ConnectionStrings:HealthCheck"] = "HealthCheck",
            ["ConnectionStrings:EstateReportingReadModel"] = "",
            ["ConnectionStrings:TransactionProcessorReadModel"] = ""
        };
    public static CallbackCommands.RecordCallbackCommand RecordCallbackCommand =>
        new (TestData.CallbackId,
            TestData.CallbackMessage,
            TestData.Destinations,
            (MessageFormat)TestData.MessageFormat,
            TestData.TypeString,
            TestData.Reference);

    public static CallbackCommands.RecordCallbackCommand RecordCallbackCommandEmptyReference =>
        new(TestData.CallbackId,
            TestData.CallbackMessage,
            TestData.Destinations,
            (MessageFormat)TestData.MessageFormat,
            TestData.TypeString,
            String.Empty);

    public static CallbackCommands.RecordCallbackCommand RecordCallbackCommandInvalidReference =>
        new(TestData.CallbackId,
            TestData.CallbackMessage,
            TestData.Destinations,
            (MessageFormat)TestData.MessageFormat,
            TestData.TypeString,
            "reference");

    public static CallbackCommands.RecordCallbackCommand RecordCallbackCommandInvalidEstateIdInReference =>
        new(TestData.CallbackId,
            TestData.CallbackMessage,
            TestData.Destinations,
            (MessageFormat)TestData.MessageFormat,
            TestData.TypeString,
            "reference-71AA10137C9341C793EDDDE89C549455");

    public static CallbackCommands.RecordCallbackCommand RecordCallbackCommandInvalidMerchantIdInReference =>
        new(TestData.CallbackId,
            TestData.CallbackMessage,
            TestData.Destinations,
            (MessageFormat)TestData.MessageFormat,
            TestData.TypeString,
            "71AA10137C9341C793EDDDE89C549455-reference");

    public static CallbackQueries.GetCallbackQuery GetCallbackQuery =>
        new CallbackQueries.GetCallbackQuery(TestData.CallbackId);

    public static CallbackMessageAggregate EmptyCallbackMessageAggregate()
    {
        return new CallbackMessageAggregate();
    }

    public static CallbackMessageAggregate RecordedCallbackMessageAggregate()
    {
        CallbackMessageAggregate aggregate = new();
        aggregate.RecordCallback(TestData.CallbackId, TestData.TypeString, (MessageFormat)TestData.MessageFormat,
            TestData.CallbackMessage, TestData.Destinations,(TestData.Reference, EstateReference, MerchantReference));

        return aggregate;
    }
}
