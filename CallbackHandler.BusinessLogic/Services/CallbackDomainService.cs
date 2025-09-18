using Shared.Results;
using SimpleResults;

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

    public async Task<Result> RecordCallback(Guid callbackId,
                                             String typeString,
                                             MessageFormat messageFormat,
                                             String callbackMessage,
                                             String reference,
                                             String[] destinations,
                                             CancellationToken cancellationToken) {

        // split the reference string into an array of strings
        String[] referenceData = reference?.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
        if (referenceData.Length == 0)
        {
            return Result.Failure("Reference cannot be empty.");
        }
        // Element 0 is estate reference, Element 1 is merchant reference
        String estateReference = referenceData[0];
        String merchantReference = referenceData[1];

        // TODO: Validate the reference data
        
        Result<CallbackMessageAggregate> getResult = await this.AggregateRepository.GetLatestVersion(callbackId, cancellationToken);
        Result<CallbackMessageAggregate> callbackMessageAggregateResult =
            DomainServiceHelper.HandleGetAggregateResult(getResult, callbackId, false);

        CallbackMessageAggregate aggregate = callbackMessageAggregateResult.Data;
        Result stateResult = aggregate.RecordCallback(callbackId, typeString, messageFormat, callbackMessage, reference, destinations,
            Guid.Parse(estateReference), Guid.Parse(merchantReference));
        if (stateResult.IsFailed)
            return stateResult;
        return await this.AggregateRepository.SaveChanges(aggregate, cancellationToken);
    }
}

public static class DomainServiceHelper
{
    public static Result<T> HandleGetAggregateResult<T>(Result<T> result, Guid aggregateId, bool isNotFoundError = true)
        where T : Aggregate, new()  // Constraint: T is a subclass of Aggregate and has a parameterless constructor
    {
        if (result.IsFailed && result.Status != ResultStatus.NotFound)
        {
            return ResultHelpers.CreateFailure(result);
        }

        if (result.Status == ResultStatus.NotFound && isNotFoundError)
        {
            return ResultHelpers.CreateFailure(result);
        }

        T aggregate = result.Status switch
        {
            ResultStatus.NotFound => new T { AggregateId = aggregateId },  // Set AggregateId when creating a new instance
            _ => result.Data
        };

        return Result.Success(aggregate);
    }
}