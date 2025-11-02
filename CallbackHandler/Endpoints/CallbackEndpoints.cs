using CallbackHandler.DataTransferObjects;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SimpleResults;
using System;
using Shared.Extensions;
using Shared.Middleware;

namespace CallbackHandler.Endpoints
{
    public static class CallbackEndpoints
    {
        private const string BaseRoute = "/api/callbacks";

        public static IEndpointRouteBuilder MapCallbackEndpoints(this IEndpointRouteBuilder endpoints)
        {
            RouteGroupBuilder group = endpoints.MapGroup(BaseRoute)
                .WithTags("Callbacks");

            group.MapPost("/", Handlers.CallbackHandlers.RecordCallback)
                .WithName("RecordCallback")
                .WithSummary("Records a deposit callback")
                .WithStandardProduces<CallbackResponse, ErrorResponse>(); ;

            group.MapGet("/{callbackId:guid}", Handlers.CallbackHandlers.GetCallback)
                .WithName("GetCallback")
                .WithSummary("Gets a callback by ID")
                .WithStandardProduces<DataTransferObjects.CallbackMessage, ErrorResponse>();

            return endpoints;
        }
    }
}
