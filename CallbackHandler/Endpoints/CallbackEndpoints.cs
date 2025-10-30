using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SimpleResults;
using System;

namespace CallbackHandler.Endpoints
{
    public static class CallbackEndpoints
    {
        private const string BaseRoute = "/api/callbacks";

        public static IEndpointRouteBuilder MapCallbackEndpoints(this IEndpointRouteBuilder endpoints)
        {
            RouteGroupBuilder group = endpoints.MapGroup(BaseRoute)
                .WithTags("Callbacks");

            group.MapPost("/", Handlers.CallbackHandlers.RecordCallbackAsync)
                .WithName("RecordCallback")
                .WithSummary("Records a deposit callback")
                .Produces<Result<Guid>>(StatusCodes.Status200OK);

            group.MapGet("/{callbackId:guid}", Handlers.CallbackHandlers.GetCallbackAsync)
                .WithName("GetCallback")
                .WithSummary("Gets a callback by ID")
                .Produces<Result<DataTransferObjects.CallbackMessage>>(StatusCodes.Status200OK);

            return endpoints;
        }
    }
}
