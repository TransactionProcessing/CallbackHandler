namespace CallbackHandler.Controllers
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessLogic.Requests;
    using DataTransferObjects;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using Swashbuckle.AspNetCore.Annotations;

    [ExcludeFromCodeCoverage]
    [Route(CallbackController.ControllerRoute)]
    [ApiController]
    public class CallbackController : ControllerBase
    {
        #region Fields

        private readonly IMediator Mediator;

        #endregion

        #region Constructors

        public CallbackController(IMediator mediator)
        {
            this.Mediator = mediator;
        }

        #endregion

        #region Methods

        [HttpPost]
        [SwaggerResponse(200, "OK")]
        public async Task<IActionResult> RecordCallback(Deposit depositCallback,
                                                        CancellationToken cancellationToken)
        {
            Guid callbackId = Guid.NewGuid();

            RecordCallbackRequest request = RecordCallbackRequest.Create(callbackId,
                                                                         1, // JSON
                                                                         depositCallback.GetType().ToString(),
                                                                         JsonConvert.SerializeObject(depositCallback),
                                                                         depositCallback.Reference,
                                                                         new[] {"EstateManagement"});

            await this.Mediator.Send(request, cancellationToken);

            return this.Ok();
        }

        #endregion

        #region Others

        /// <summary>
        /// The controller name
        /// </summary>
        public const String ControllerName = "callbacks";

        /// <summary>
        /// The controller route
        /// </summary>
        private const String ControllerRoute = "api/" + CallbackController.ControllerName;

        #endregion
    }
}