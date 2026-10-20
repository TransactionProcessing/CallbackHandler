namespace CallbackHandler.DataTransferObjects
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    
    [ExcludeFromCodeCoverage]
    public class Deposit
    {
        [Description("Account number associated with the deposit.")]
        public String AccountNumber { get; set; }

        [Description("Amount deposited.")]
        public Decimal Amount { get; set; }

        [Description("Date and time when the deposit occurred.")]
        public DateTime DateTime { get; set; }

        [Description("Identifier of the deposit event.")]
        public Guid DepositId { get; set; }

        [Description("Identifier of the host that originated the deposit.")]
        public Guid HostIdentifier { get; set; }

        [Description("Reference used to identify or correlate the deposit.")]
        public String Reference { get; set; }

        [Description("Sort code associated with the account.")]
        public String SortCode { get; set; }
    }
}
