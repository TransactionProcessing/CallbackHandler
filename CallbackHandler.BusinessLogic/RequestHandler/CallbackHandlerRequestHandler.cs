namespace CallbackHandler.BusinessLogic.RequestHandler
{
    using System.Threading;
    using System.Threading.Tasks;
    using CallbackMessageAggregate;
    using MediatR;
    using Requests;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.Aggregate;

    public class CallbackHandlerRequestHandler : IRequestHandler<RecordCallbackRequest>
    {
        private readonly IAggregateRepository<CallbackMessageAggregate, DomainEventRecord.DomainEvent> AggregateRepository;

        public CallbackHandlerRequestHandler(IAggregateRepository<CallbackMessageAggregate,DomainEventRecord.DomainEvent> aggregateRepository)
        {
            this.AggregateRepository = aggregateRepository;
        }

        public async Task<Unit> Handle(RecordCallbackRequest request,
                                       CancellationToken cancellationToken)
        {

            CallbackMessageAggregate aggregate = await this.AggregateRepository.GetLatestVersion(request.CallbackId, cancellationToken);

            aggregate.RecordCallback(request.TypeString, request.MessageFormat, request.CallbackMessage, request.Reference, request.Destinations);

            await this.AggregateRepository.SaveChanges(aggregate,cancellationToken);
            
            return new Unit();
        }
    }
}