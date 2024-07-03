using CallbackHandlers.Models;

namespace CallbackHandler.Controllers
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using Azure.Core;
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

            CallbackCommands.RecordCallbackRequest request = new CallbackCommands.RecordCallbackRequest(callbackId,
                JsonConvert.SerializeObject(depositCallback),
                new[] { "EstateManagement" },
                MessageFormat.JSON,
                depositCallback.GetType().ToString(),
                depositCallback.Reference);

            await this.Mediator.Send(request, cancellationToken);

            return this.Ok(callbackId);
        }

        [HttpGet]
        [Route("{callbackId}")]
        [SwaggerResponse(200, "OK")]
        public async Task<IActionResult> GetCallback([FromRoute ]Guid callbackId, CancellationToken cancellationToken)
        {
            CallbackQueries.GetCallbackQuery query = new CallbackQueries.GetCallbackQuery(callbackId);

            var message = await this.Mediator.Send(query, cancellationToken);

            var response = new CallbackMessage
            {
                Reference = message.Reference,
                TypeString = message.TypeString,
                Message = message.Message
            };

            return this.Ok(response);
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