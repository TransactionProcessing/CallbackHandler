namespace CallbackHandler.DataTransferObjects
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    
    [ExcludeFromCodeCoverage]
    public class Deposit
    {
        public String AccountNumber { get; set; }

        public Decimal Amount { get; set; }

        public DateTime DateTime { get; set; }

        public Guid DepositId { get; set; }

        public Guid HostIdentifier { get; set; }

        public String Reference { get; set; }

        public String SortCode { get; set; }
    }
}