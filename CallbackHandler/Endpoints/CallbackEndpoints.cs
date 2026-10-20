using Asp.Versioning;
using Asp.Versioning.Builder;
using CallbackHandler.DataTransferObjects;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Middleware;

namespace CallbackHandler.Endpoints
{
    public static class CallbackEndpoints
    {
        public static IEndpointRouteBuilder MapCallbackEndpoints(this IEndpointRouteBuilder endpoints)
        {
            IVersionedEndpointRouteBuilder api = endpoints.NewVersionedApi("Callbacks");
            RouteGroupBuilder group = api
                                      .MapGroup("/api/v{version:apiVersion}/callbacks")
                                      .WithTags("Callbacks")
                                      .HasApiVersion(new ApiVersion(1.0));

            group.MapPost("/", Handlers.CallbackHandlers.RecordCallback)
                .WithName("RecordCallback")
                .WithSummary("Receive a deposit callback")
                .WithDescription("Accepts a deposit notification from the transaction processor, records it, and returns the identifier assigned to the callback. Use that identifier with GetCallback to retrieve the recorded message.")
                .Accepts<Deposit>("application/json")
                .Produces<CallbackResponse>(StatusCodes.Status200OK)
                .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
                .Produces<ErrorResponse>(StatusCodes.Status409Conflict)
                .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError)
                .Produces(StatusCodes.Status501NotImplemented);

            group.MapGet("/{callbackId:guid}", Handlers.CallbackHandlers.GetCallback)
                .WithName("GetCallback")
                .WithSummary("Get a callback by ID")
                .WithDescription("Returns the stored callback message, its original message type, and its reference for the specified callback identifier.")
                .Produces<DataTransferObjects.CallbackMessage>(StatusCodes.Status200OK)
                .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
                .Produces<ErrorResponse>(StatusCodes.Status409Conflict)
                .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError)
                .Produces(StatusCodes.Status501NotImplemented);

            return endpoints;
        }
    }
}
