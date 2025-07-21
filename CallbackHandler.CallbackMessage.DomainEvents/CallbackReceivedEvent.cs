namespace CallbackHandler.CallbackMessage.DomainEvents
{
    using System;
    using Shared.DomainDrivenDesign.EventSourcing;

    public record CallbackReceivedEvent(Guid AggregateId,
                                        String TypeString,
                                        Int32 MessageFormat,
                                        String CallbackMessage,
                                        String Reference,
                                        String Destination,
                                        Guid EstateId,
                                        Guid MerchantId) : DomainEvent(AggregateId, Guid.NewGuid());
}