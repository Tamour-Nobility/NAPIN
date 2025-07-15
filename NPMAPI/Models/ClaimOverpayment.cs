using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NPMAPI.Models
{
    public class ClaimOverpayment
    {
        public Nullable<long> Claim_No { get; set; }
        public Nullable<decimal> Insurance_over_paid { get; set; } = 0;
        public Nullable<decimal> Patient_credit_balance { get; set; } = 0;
        public Nullable<decimal> Total_Responsibility { get; set; }
        public Nullable<long> Created_By { get; set; }
        public Nullable<System.DateTimeOffset> Created_Date { get; set; }
        public Nullable<long> Modified_By { get; set; }
        public Nullable<System.DateTimeOffset> Modified_date { get; set; }
        public Nullable<bool> Deleted { get; set; }
    }
}