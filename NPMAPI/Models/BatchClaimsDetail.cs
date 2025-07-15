using System;

namespace NPMAPI.Models
{
    public class BatchClaimsDetail
    {
        public long Claim_No { get; set; }
        public DateTime DOS { get; set; }
        public decimal Billed_Amount { get; set; }
    }
}