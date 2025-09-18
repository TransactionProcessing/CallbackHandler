using System;
using CallbackHandlers.Models;
using Xunit;

namespace CallbackHander.Testing
{
    using CallbackHandler.BusinessLogic.Requests;
    using CallbackHandler.CallbackMessageAggregate;

    public class TestData
    {
        public static Guid CallbackId = Guid.Parse("E82DF060-5717-4F79-B99B-E14C702C8F0E");

        public static Int32 MessageFormat = 1;

        public static String TypeString = "TestTypeString";

        public static String CallbackMessage = "Callback message";

        public static String[] Destinations = new[] {"A", "B"};

        public static String Reference = "640E863C23E244BDB9717C92733FFD4C-9D20A3961CF645EDAA7BDD436318BA29";
        public static Guid EstateReference = Guid.Parse("640E863C-23E2-44BD-B971-7C92733FFD4C");
        public static Guid MerchantReference = Guid.Parse("9D20A396-1CF6-45ED-AA7B-DD436318BA29");

        public static CallbackCommands.RecordCallbackRequest RecordCallbackRequest =>
            new CallbackCommands.RecordCallbackRequest(TestData.CallbackId,
                TestData.CallbackMessage,
                TestData.Destinations,
                (MessageFormat)TestData.MessageFormat,
                TestData.TypeString,
                TestData.Reference);

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
                TestData.CallbackMessage, TestData.Reference, TestData.Destinations, EstateReference, MerchantReference);

            return aggregate;
        }
    }
}
