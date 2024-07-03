using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallbackHandler.DataTransferObjects
{
    [ExcludeFromCodeCoverage]
    public class CallbackMessage
    {
        public String Message { get; set; }

        public String TypeString { get; set; }

        public String Reference { get; set; }
    }
}
