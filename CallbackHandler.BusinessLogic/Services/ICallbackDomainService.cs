using Azure.Core;
using CallbackHandlers.Models;
using MediatR;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CallbackHandler.BusinessLogic.Services
{
    using CallbackMessageAggregate;

    public interface ICallbackDomainService
    {
        Task RecordCallback(Guid callbackId,
                            String typeString,
                            MessageFormat messageFormat,
                            String callbackMessage,
                            String reference,
                            String[] destinations,
                            CancellationToken cancellationToken);
    }

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
}
