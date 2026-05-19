using System;

namespace SynchPayIntegrationDemo.Net45.Models
{
    public sealed class PaymentFormModel
    {
        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public string CompanyId { get; set; }

        public string ContactNumber { get; set; }

        public decimal Amount { get; set; }

        public int AmountInCents
        {
            get
            {
                return (int)Math.Round(Amount * 100m, MidpointRounding.AwayFromZero);
            }
        }
    }
}
