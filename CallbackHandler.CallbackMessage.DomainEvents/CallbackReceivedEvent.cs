using TransactionProcessing.Shared.DomainDrivenDesign.EventSourcing;

namespace CallbackHandler.CallbackMessage.DomainEvents
{
    using System;

    public record CallbackReceivedEvent(Guid AggregateId,
                                        String TypeString,
                                        Int32 MessageFormat,
                                        String CallbackMessage,
                                        String Reference,
                                        String Destination,
                                        Guid EstateId,
                                        Guid MerchantId) : DomainEvent(AggregateId, Guid.NewGuid());
}