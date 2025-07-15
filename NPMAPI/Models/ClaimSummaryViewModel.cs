using NPMAPI.Models.ViewModels;
using System;
using System.Collections.Generic;

namespace NPMAPI.Models
{
    public class ClaimSummaryViewModel
    {

        public string TOTAL_CHARGES { get; set; }
        public string TOTAL_PAYMENT { get; set; }
        public string InboxPaymnet { get; set; }
        public string INSURANCE_DUE { get; set; }
        public string PAT_DUE { get; set; }
        public string INS_TOTAL_PAYMENT { get; set; }
        public string PATIENT_PAYMENTS { get; set; }
        public string COLLECTION_DUE { get; set; }
        public Nullable<decimal> Insurance_over_paid { get; set; }
        public Nullable<decimal> Patient_credit_balance { get; set; }
        public List<Claim> claimList { get; set; }
        public List<ClaimsDetail> claimlistdetails { get; set; }

    }
}