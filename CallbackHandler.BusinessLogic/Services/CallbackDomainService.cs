using Shared.EventStore.Helpers;
using SimpleResults;

namespace CallbackHandler.BusinessLogic.Services;

using CallbackHandler.BusinessLogic.Requests;
using CallbackMessageAggregate;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using System;
using System.Threading;
using System.Threading.Tasks;

public class CallbackDomainService : ICallbackDomainService
{
    private readonly IAggregateRepository<CallbackMessageAggregate, DomainEvent> AggregateRepository;

    public CallbackDomainService(IAggregateRepository<CallbackMessageAggregate, DomainEvent> aggregateRepository) {
        this.AggregateRepository = aggregateRepository;
    }

    public async Task<Result> RecordCallback(CallbackCommands.RecordCallbackCommand command,
                                             CancellationToken cancellationToken) {

        // split the reference string into an array of strings
        String[] referenceData = command.Reference?.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
        // TODO: Validate the reference data has the correct number of elements
        if (referenceData.Length == 0) {
            return Result.Invalid("Reference cannot be empty.");
        }

        // Element 0 is estate reference, Element 1 is merchant reference
        String estateReference = referenceData[0];
        String merchantReference = referenceData[1];

        // TODO: Validate the reference data
        
        Result<CallbackMessageAggregate> getResult = await this.AggregateRepository.GetLatestVersion(command.CallbackId, cancellationToken);
        Result<CallbackMessageAggregate> callbackMessageAggregateResult =
            DomainServiceHelper.HandleGetAggregateResult(getResult, command.CallbackId, false);

        CallbackMessageAggregate aggregate = callbackMessageAggregateResult.Data;
        Result stateResult = aggregate.RecordCallback(command.CallbackId, command.TypeString, command.MessageFormat, command.CallbackMessage, command.Destinations,
            (command.Reference, Guid.Parse(estateReference), Guid.Parse(merchantReference)));
        if (stateResult.IsFailed)
            return stateResult;
        return await this.AggregateRepository.SaveChanges(aggregate, cancellationToken);
    }
}