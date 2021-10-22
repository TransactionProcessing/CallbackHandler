namespace CallbackHandler.CallbackMessageAggregate.Tests
{
    using CallbackHander.Testing;
    using CallbackHandlers.Models;
    using Shouldly;
    using Xunit;

    public class CallbackMessageAggregateTests
    {
        [Fact]
        public void CallbackMessageAggregate_CanBeCreated_IsCreated()
        {
            CallbackMessageAggregate aggregate = CallbackMessageAggregate.Create(TestData.CallbackId);

            aggregate.AggregateId.ShouldBe(TestData.CallbackId);
        }

        [Fact]
        public void CallbackMessageAggregate_RecordCallback_CallbackIsRecorded()
        {
            CallbackMessageAggregate aggregate = CallbackMessageAggregate.Create(TestData.CallbackId);

            aggregate.RecordCallback(TestData.TypeString, MessageFormat.JSON, TestData.CallbackMessage, TestData.Reference, TestData.Destinations);

            aggregate.ShouldSatisfyAllConditions(() => aggregate.CallbackMessage.ShouldBe(TestData.CallbackMessage),
                                                 () => aggregate.TypeString.ShouldBe(TestData.TypeString),
                                                 () => aggregate.MessageFormat.ShouldBe(MessageFormat.JSON),
                                                 () => aggregate.Reference.ShouldBe(TestData.Reference),
                                                 () => aggregate.GetDestinations().ShouldNotBeEmpty(),
                                                 () => aggregate.GetDestinations().Length.ShouldBe(TestData.Destinations.Length));
        }
    }
}