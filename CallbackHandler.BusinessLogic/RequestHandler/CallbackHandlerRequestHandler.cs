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

    public class CallbackHandlerRequestHandler : IRequestHandler<RecordCallbackRequest>
    {
        private readonly ICallbackDomainService CallbackDomainService;
        
        public CallbackHandlerRequestHandler(ICallbackDomainService callbackDomainService) {
            this.CallbackDomainService = callbackDomainService;
        }

        public async Task<Unit> Handle(RecordCallbackRequest request,
                                       CancellationToken cancellationToken) {

            await this.CallbackDomainService.RecordCallback(request.CallbackId,
                                                            request.TypeString,
                                                            request.MessageFormat,
                                                            request.CallbackMessage,
                                                            request.Reference,
                                                            request.Destinations,
                                                            cancellationToken);
            
            return new Unit();
        }
    }
}