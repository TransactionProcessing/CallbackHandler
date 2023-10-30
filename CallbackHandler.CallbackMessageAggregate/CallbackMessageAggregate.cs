using System.Runtime.CompilerServices;

namespace CallbackHandler.CallbackMessageAggregate
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using CallbackHandlers.Models;
    using CallbackMessage.DomainEvents;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.Aggregate;
    using Shared.General;
    
    public record CallbackMessageAggregate : Aggregate
    {
        public CallbackMessageAggregate()
        {
            this.Destinations = new List<String>();
        }

        public String CallbackMessage { get; internal set; }

        public MessageFormat MessageFormat { get; internal set; }

        public String TypeString { get; internal set; }

        public String Reference { get; internal set; }

        internal List<String> Destinations;

        [ExcludeFromCodeCoverage]
        protected override Object GetMetadata()
        {
            return null;
        }

        public override void PlayEvent(IDomainEvent domainEvent) => CallbackMessageAggregateExtensions.PlayEvent(this, (dynamic)domainEvent);
    }

    public static class CallbackMessageAggregateExtensions
    {
        public static void PlayEvent(this CallbackMessageAggregate aggregate, CallbackReceivedEvent domainEvent)
        {
            // Mutate the state here based on the event
            aggregate.CallbackMessage = domainEvent.CallbackMessage;
            aggregate.MessageFormat = (MessageFormat) domainEvent.MessageFormat;
            aggregate.TypeString = domainEvent.TypeString;
            aggregate.Reference = domainEvent.Reference;
            aggregate.Destinations.Add(domainEvent.Destination);
        }

        public static void RecordCallback(this CallbackMessageAggregate aggregate,
            Guid aggregateId,
            String typeString,
            MessageFormat messageFormat,
            String callbackMessage,
            String reference,
            String[] destinations)
        {
            foreach (String destination in destinations)
            {
                DomainEvent callbackReceivedEvent = CreateCallbackReceivedEvent(aggregate,aggregateId, typeString,
                    messageFormat, callbackMessage, reference, destination);

                aggregate.ApplyAndAppend(callbackReceivedEvent);
            }
        }

        internal static DomainEvent CreateCallbackReceivedEvent(this CallbackMessageAggregate aggregate,
            Guid aggregateId,
            String typeString,
            MessageFormat messageFormat,
            String callbackMessage,
            String reference,
            String destination)
        {
            return new CallbackReceivedEvent(aggregateId, typeString, (Int32) messageFormat,
                callbackMessage,
                reference,
                destination);
        }

        public static String[] GetDestinations(this CallbackMessageAggregate aggregate)
        {
            return aggregate.Destinations.ToArray();
        }
    }
}