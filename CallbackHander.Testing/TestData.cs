using System;
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

        public static String Reference = "TestRef";

        public static RecordCallbackRequest RecordCallbackRequest =>
            RecordCallbackRequest.Create(TestData.CallbackId, TestData.MessageFormat, TestData.TypeString,
                                         TestData.CallbackMessage, TestData.Reference, TestData.Destinations);

        public static CallbackMessageAggregate EmptyCallbackMessageAggregate()
        {
            return new CallbackMessageAggregate();
        }
    }
}
