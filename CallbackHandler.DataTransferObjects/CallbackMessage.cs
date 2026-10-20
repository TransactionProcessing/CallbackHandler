using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace CallbackHandler.DataTransferObjects
{
    [ExcludeFromCodeCoverage]
    public class CallbackMessage
    {
        [Description("Original callback payload serialized as a JSON string.")]
        public String Message { get; set; }

        [Description(".NET type name recorded for the original callback payload.")]
        public String TypeString { get; set; }

        [Description("Reference supplied with the original deposit callback.")]
        public String Reference { get; set; }
    }

    public class CallbackResponse
    {
        [Description("Identifier assigned to the recorded callback. Use it to retrieve the callback.")]
        public Guid CallbackId { get; set; }
    }
}
