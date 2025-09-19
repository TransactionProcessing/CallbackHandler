using System.Runtime.CompilerServices;
using SimpleResults;

namespace CallbackHandler.CallbackMessageAggregate;

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

public static class CallbackMessageAggregateExtensions {
    public static void PlayEvent(this CallbackMessageAggregate aggregate,
                                 CallbackReceivedEvent domainEvent) {
        // Mutate the state here based on the event
        aggregate.CallbackMessage = domainEvent.CallbackMessage;
        aggregate.MessageFormat = (MessageFormat)domainEvent.MessageFormat;
        aggregate.TypeString = domainEvent.TypeString;
        aggregate.Reference = domainEvent.Reference;
        aggregate.Destinations.Add(domainEvent.Destination);
    }

    public static Result RecordCallback(this CallbackMessageAggregate aggregate,
                                        Guid aggregateId,
                                        String typeString,
                                        MessageFormat messageFormat,
                                        String callbackMessage,
                                        String reference,
                                        String[] destinations,
                                        Guid estateId,
                                        Guid merchantId) {
        foreach (String destination in destinations) {
            DomainEvent callbackReceivedEvent = CreateCallbackReceivedEvent(aggregate, aggregateId, typeString, messageFormat, callbackMessage, reference, destination, estateId, merchantId);

            aggregate.ApplyAndAppend(callbackReceivedEvent);
        }

        return Result.Success();
    }

    internal static DomainEvent CreateCallbackReceivedEvent(this CallbackMessageAggregate aggregate,
                                                            Guid aggregateId,
                                                            String typeString,
                                                            MessageFormat messageFormat,
                                                            String callbackMessage,
                                                            String reference,
                                                            String destination,
                                                            Guid estateId,
                                                            Guid merchantId) {
        return new CallbackReceivedEvent(aggregateId, typeString, (Int32)messageFormat, callbackMessage, reference, destination, estateId, merchantId);
    }

    public static String[] GetDestinations(this CallbackMessageAggregate aggregate) {
        return aggregate.Destinations.ToArray();
    }

    public static CallbackMessage GetCallbackMessage(this CallbackMessageAggregate aggregate) {
        return new CallbackMessage {
            Reference = aggregate.Reference,
            Destinations = aggregate.Destinations,
            Message = aggregate.CallbackMessage,
            MessageFormat = aggregate.MessageFormat,
            TypeString = aggregate.TypeString
        };
    }
}