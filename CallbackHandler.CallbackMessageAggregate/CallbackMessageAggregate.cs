namespace CallbackHandler.CallbackMessageAggregate
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using CallbackHandlers.Models;
    using CallbackMessage.DomainEvents;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.Aggregate;
    using Shared.General;

    public class CallbackMessageAggregate : Aggregate
    {
        #region Fields

        private readonly List<String> Destinations;

        #endregion

        #region Constructors

        [ExcludeFromCodeCoverage]
        public CallbackMessageAggregate()
        {
            this.Destinations = new List<String>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractAggregate" /> class.
        /// </summary>
        /// <param name="aggregateId">The aggregate identifier.</param>
        private CallbackMessageAggregate(Guid aggregateId)
        {
            Guard.ThrowIfInvalidGuid(aggregateId, "Aggregate Id cannot be an Empty Guid");

            this.AggregateId = aggregateId;
            this.Destinations = new List<String>();
        }

        #endregion

        #region Properties

        public String CallbackMessage { get; private set; }

        public MessageFormat MessageFormat { get; private set; }

        public String TypeString { get; private set; }

        public String Reference { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Creates the specified aggregate identifier.
        /// </summary>
        /// <param name="aggregateId">The aggregate identifier.</param>
        /// <returns></returns>
        public static CallbackMessageAggregate Create(Guid aggregateId)
        {
            return new CallbackMessageAggregate(aggregateId);
        }

        public String[] GetDestinations()
        {
            return this.Destinations.ToArray();
        }

        public override void PlayEvent(IDomainEvent domainEvent)
        {
            this.PlayEvent((dynamic)domainEvent);
        }

        public void RecordCallback(String typeString,
                                   MessageFormat messageFormat,
                                   String callbackMessage,
                                   String reference,
                                   String[] destinations)
        {
            foreach (String destination in destinations)
            {
                CallbackReceivedEvent callbackReceivedEvent = new CallbackReceivedEvent(this.AggregateId, typeString, (Int32)messageFormat, callbackMessage, reference, destination);
                this.ApplyAndAppend(callbackReceivedEvent);
            }
        }

        [ExcludeFromCodeCoverage]
        protected override Object GetMetadata()
        {
            return null;
        }

        private void PlayEvent(CallbackReceivedEvent domainEvent)
        {
            this.CallbackMessage = domainEvent.CallbackMessage;
            this.MessageFormat = (MessageFormat)domainEvent.MessageFormat;
            this.TypeString = domainEvent.TypeString;
            this.Destinations.Add(domainEvent.Destination);
            this.Reference = domainEvent.Reference;
        }

        #endregion
    }
}