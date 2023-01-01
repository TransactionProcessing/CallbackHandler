namespace CallbackHandler.BusinessLogic.Services;

using System;
using System.Threading;
using System.Threading.Tasks;
using CallbackHandlers.Models;
using CallbackMessageAggregate;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;

public class CallbackDomainService : ICallbackDomainService
{
    private readonly IAggregateRepository<CallbackMessageAggregate, DomainEvent> AggregateRepository;

    public CallbackDomainService(IAggregateRepository<CallbackMessageAggregate, DomainEvent> aggregateRepository) {
        this.AggregateRepository = aggregateRepository;
    }

    public async Task RecordCallback(Guid callbackId,
                                     String typeString,
                                     MessageFormat messageFormat,
                                     String callbackMessage,
                                     String reference,
                                     String[] destinations,
                                     CancellationToken cancellationToken) {
        CallbackMessageAggregate aggregate = await this.AggregateRepository.GetLatestVersion(callbackId, cancellationToken);
        aggregate.RecordCallback(callbackId, typeString, messageFormat, callbackMessage, reference, destinations);

        await this.AggregateRepository.SaveChanges(aggregate, cancellationToken);
    }
}