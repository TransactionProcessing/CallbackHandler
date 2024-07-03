using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace CallbackHandlers.Models;

[ExcludeFromCodeCoverage]
public class CallbackMessage
{
    public String Message { get; set; }

    public MessageFormat MessageFormat { get; set; }

    public String TypeString { get; set; }

    public String Reference { get; set; }

    public List<String> Destinations { get; set; }

}