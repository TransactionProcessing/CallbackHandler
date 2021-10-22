namespace CallbackHandler.CallbackMessageAggregate.Tests
{
    using CallbackHander.Testing;
    using CallbackMessage.DomainEvents;
    using Shouldly;
    using Xunit;

    public class DomainEventTests
    {
        [Fact]
        public void CallbackReceivedEvent_CanbeCreated_IsCreated()
        {
            CallbackReceivedEvent callbackReceivedEvent =
                new CallbackReceivedEvent(TestData.CallbackId, TestData.TypeString, TestData.MessageFormat, 
                                          TestData.CallbackMessage, TestData.Reference, TestData.Destinations[0]);

            callbackReceivedEvent.ShouldNotBeNull();
            callbackReceivedEvent.ShouldSatisfyAllConditions(() => callbackReceivedEvent.CallbackMessage.ShouldBe(TestData.CallbackMessage),
                                                             () => callbackReceivedEvent.TypeString.ShouldBe(TestData.TypeString),
                                                             () => callbackReceivedEvent.MessageFormat.ShouldBe(TestData.MessageFormat),
                                                             () => callbackReceivedEvent.Reference.ShouldBe(TestData.Reference),
                                                             () => callbackReceivedEvent.Destination.ShouldBe(TestData.Destinations[0]));
        }
    }
}