namespace CallbackHandler.BusinessLogic.Requests
{
    using System;
    using CallbackHandlers.Models;
    using MediatR;

    public class RecordCallbackRequest : IRequest
    {
        #region Constructors

        private RecordCallbackRequest(Guid callbackId,
                                      Int32 messageFormat,
                                      String typeString,
                                      String callbackMessage,
                                      String reference,
                                      String[] destinations)
        {
            this.CallbackId = callbackId;
            this.MessageFormat = (MessageFormat)messageFormat;
            this.TypeString = typeString;
            this.CallbackMessage = callbackMessage;
            this.Destinations = destinations;
            this.Reference = reference;
        }

        #endregion

        #region Properties

        public Guid CallbackId { get; }

        public String CallbackMessage { get; }

        public String[] Destinations { get; }

        public MessageFormat MessageFormat { get; }

        public String TypeString { get; }

        public String Reference { get; }

        #endregion

        #region Methods

        public static RecordCallbackRequest Create(Guid callbackId,
                                                   Int32 messageFormat,
                                                   String typeString,
                                                   String callbackMessage,
                                                   String reference,
                                                   String[] destinations)
        {
            return new RecordCallbackRequest(callbackId, messageFormat, typeString, callbackMessage, reference, destinations);
        }

        #endregion
    }
}