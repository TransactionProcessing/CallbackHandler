namespace CallbackHandler.BusinessLogic.Tests.Requests
{
    using BusinessLogic.Requests;
    using CallbackHander.Testing;
    using CallbackHandlers.Models;
    using Shouldly;
    using Xunit;

    public class RequestTests
    {
        [Fact]
        public void RecordCallbackRequest_CanBeCreated_IsCreated()
        {
            RecordCallbackRequest recordCallbackRequest = RecordCallbackRequest.Create(TestData.CallbackId, TestData.MessageFormat, TestData.TypeString,
                                                                                       TestData.CallbackMessage, TestData.Reference,
                                                                                       TestData.Destinations);

            recordCallbackRequest.ShouldNotBeNull();
            recordCallbackRequest.ShouldSatisfyAllConditions(
                                                             () => recordCallbackRequest.CallbackId.ShouldBe(TestData.CallbackId),
                                                             () => recordCallbackRequest.MessageFormat.ShouldBe((MessageFormat)TestData.MessageFormat),
                                                             () => recordCallbackRequest.TypeString.ShouldBe(TestData.TypeString),
                                                             () => recordCallbackRequest.CallbackMessage.ShouldBe(TestData.CallbackMessage),
                                                             () => recordCallbackRequest.Reference.ShouldBe(TestData.Reference),
                                                             () => recordCallbackRequest.Destinations.ShouldBe(TestData.Destinations)
                                                            );
        }
    }
}