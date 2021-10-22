namespace CallbackHandler.CallbackMessage.DomainEvents
{
    using System;
    using Shared.DomainDrivenDesign.EventSourcing;

    public record CallbackReceivedEvent : DomainEventRecord.DomainEvent
    {
        public CallbackReceivedEvent(Guid aggregateId, String typeString,
                                     Int32 messageFormat,
                                     String callbackMessage,
                                     String reference,
                                     String destination) : base(aggregateId, Guid.NewGuid())
        {
            this.TypeString = typeString;
            this.MessageFormat = messageFormat;
            this.CallbackMessage = callbackMessage;
            this.Destination = destination;
            this.Reference = reference;
        }

        public String TypeString { get; init; }
        public Int32 MessageFormat { get; init; }
        public String CallbackMessage { get; init; }

        public String Reference { get; init; }

        public String Destination { get; init; }
    }
}