namespace CallbackHandler.BusinessLogic.Common;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Shared.DomainDrivenDesign.EventSourcing;

[ExcludeFromCodeCoverage]
public class DomainEventTypesToSilentlyHandle : IDomainEventTypesToSilentlyHandle
{
    #region Fields

    /// <summary>
    /// The handler event types to silently handle
    /// </summary>
    private readonly Dictionary<String, String[]> HandlerEventTypesToSilentlyHandle;

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainEventTypesToSilentlyHandle" /> class.
    /// </summary>
    /// <param name="handlerEventTypesToSilentlyHandle">The handler event types to silently handle.</param>
    public DomainEventTypesToSilentlyHandle(Dictionary<String, String[]> handlerEventTypesToSilentlyHandle)
    {
        this.HandlerEventTypesToSilentlyHandle = handlerEventTypesToSilentlyHandle;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Handles the silently.
    /// </summary>
    /// <param name="handlerName">Name of the handler.</param>
    /// <param name="domainEvent">The domain event.</param>
    /// <returns></returns>
    public Boolean HandleSilently(String handlerName,
                                  DomainEvent domainEvent)
    {
        if (this.HandlerEventTypesToSilentlyHandle.TryGetValue(handlerName, out var eventTypes)
            && eventTypes.Contains(domainEvent.GetType().FullName))
        {
            return true;
        }

        return false;
        }

        #endregion
}