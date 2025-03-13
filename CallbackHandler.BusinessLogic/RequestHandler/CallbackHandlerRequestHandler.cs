using Shared.Results;
using SimpleResults;

namespace CallbackHandler.BusinessLogic.RequestHandler
{
    using System.Threading;
    using System.Threading.Tasks;
    using CallbackHandler.BusinessLogic.Services;
    using CallbackMessageAggregate;
    using MediatR;
    using Requests;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.Aggregate;

    public class CallbackHandlerRequestHandler : IRequestHandler<CallbackCommands.RecordCallbackRequest, Result>,
                                                 IRequestHandler<CallbackQueries.GetCallbackQuery, Result<CallbackHandlers.Models.CallbackMessage>>
    {
        private readonly ICallbackDomainService CallbackDomainService;
        private readonly IAggregateRepository<CallbackMessageAggregate, DomainEvent> CallbackAggregateRepository;

        public CallbackHandlerRequestHandler(ICallbackDomainService callbackDomainService,
            IAggregateRepository<CallbackMessageAggregate, DomainEvent> callbackAggregateRepository)
        {
            this.CallbackDomainService = callbackDomainService;
            CallbackAggregateRepository = callbackAggregateRepository;
        }

        public async Task<Result> Handle(CallbackCommands.RecordCallbackRequest request,
                                         CancellationToken cancellationToken) {

            return await this.CallbackDomainService.RecordCallback(request.CallbackId,
                                                            request.TypeString,
                                                            request.MessageFormat,
                                                            request.CallbackMessage,
                                                            request.Reference,
                                                            request.Destinations,
                                                            cancellationToken);
        }

        public async Task<Result<CallbackHandlers.Models.CallbackMessage>> Handle(CallbackQueries.GetCallbackQuery request, CancellationToken cancellationToken)
        {
            Result<CallbackMessageAggregate> callbackAggregate =
                await this.CallbackAggregateRepository.GetLatestVersion(request.CallbackId, cancellationToken);
            if (callbackAggregate.IsFailed)
                return ResultHelpers.CreateFailure(callbackAggregate);

            return Result.Success(callbackAggregate.Data.GetCallbackMessage());
        }
    }
}