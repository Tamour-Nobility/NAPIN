using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NPMAPI.Models.ViewModels
{
    public class PaymentDetailResponse
    {
        public long practice_code { get; set; }
        public string Practice_Name { get; set; }
        public long claim_no { get; set; }
        public Nullable<System.DateTime> dos { get; set; }
        public string Patient_Name { get; set; }
        public Nullable<long> patient_account { get; set; }
        public Nullable<long> attending_physician { get; set; }
        public string Billing_Provider { get; set; }
        public string date_entry { get; set; }
        public Nullable<decimal> Amount_Paid { get; set; }
        public Nullable<decimal> amount_adjusted { get; set; }
        public Nullable<decimal> Amount_rejected { get; set; }
        public string payment_type { get; set; }
        public string Payment_Source { get; set; }
        public Nullable<System.DateTime> Cheque_Date { get; set; }
        public string Cheque_No { get; set; }
   
    }
}