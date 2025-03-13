using CallbackHandlers.Models;
using Shared.EventStore.Aggregate;
using Shared.Results;
using SimpleResults;

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
        public async Task<ActionResult<Result<Guid>>> RecordCallback(Deposit depositCallback,
                                                        CancellationToken cancellationToken)
        {
            Guid callbackId = Guid.NewGuid();

            CallbackCommands.RecordCallbackRequest request = new CallbackCommands.RecordCallbackRequest(callbackId,
                JsonConvert.SerializeObject(depositCallback),
                new[] { "TransactionProcessor" },
                MessageFormat.JSON,
                depositCallback.GetType().ToString(),
                depositCallback.Reference);

            Result result = await this.Mediator.Send(request, cancellationToken);

            if (result.IsFailed)
                ResultHelpers.CreateFailure(result);
            return Result.Success(callbackId);
        }

        [HttpGet]
        [Route("{callbackId}")]
        [SwaggerResponse(200, "OK")]
        public async Task<ActionResult<Result<CallbackMessage>>> GetCallback([FromRoute ]Guid callbackId, CancellationToken cancellationToken)
        {
            CallbackQueries.GetCallbackQuery query = new CallbackQueries.GetCallbackQuery(callbackId);

            Result<CallbackHandlers.Models.CallbackMessage> getResult = await this.Mediator.Send(query, cancellationToken);

            if (getResult.IsFailed)
                ResultHelpers.CreateFailure(getResult).ToActionResult();

            
            Result<CallbackMessage> result = Result.Success(
            new CallbackMessage
            {
                Reference = getResult.Data.Reference,
                TypeString = getResult.Data.TypeString,
                Message = getResult.Data.Message
            });

            return result.ToActionResult();
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