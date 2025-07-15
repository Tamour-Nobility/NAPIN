using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NPMAPI.Models.ViewModels
{
    public class Payment_detail_Proc_Response
    {
        public long PRACTICE_CODE { get; set; }
        public string PRACTICE_NAME { get; set; }
        public long CLAIM_NO { get; set; }

        public string LOCATION_NAME { get; set; }
        public string FACILITY { get; set; }
        public Nullable<System.DateTime> DOS { get; set; }
        public string PATIENT_NAME { get; set; }
        public Nullable<long> PATIENT_ACCOUNT { get; set; }
        public Nullable<long> ATTENDING_PHYSICIAN { get; set; }
        public string BILLING_PROVIDER { get; set; }
        public string DATE_ENTRY { get; set; }
        public Nullable<decimal> AMOUNT_PAID { get; set; }
        public Nullable<decimal> AMOUNT_ADJUSTED { get; set; }
        public Nullable<decimal> AMOUNT_REJECTED { get; set; }
        public Nullable<System.DateTime> DOS_FROM { get; set; }
        public Nullable<System.DateTime> DOS_TO { get; set; }
        public string PAYMENT_TYPE { get; set; }
        public string PAYMENT_SOURCE { get; set; }
        public string INSURANCE_NAME { get; set; }
        public Nullable<System.DateTime> CHEQUE_DATE { get; set; }
        public string CHEQUE_NO { get; set; }
        public string CPT { get; set; }
    }
}