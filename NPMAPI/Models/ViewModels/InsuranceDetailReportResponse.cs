using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NPMAPI.Models.ViewModels
{
    public class InsuranceDetailReportResponse
    {
        public Nullable<long> PRACTICE_CODE { get; set; }
        public string Practice_name { get; set; }
        public string PATIENT_NAME { get; set; }
        public long PATIENT_ACCOUNT { get; set; }
        public Nullable<System.DateTime> DATE_OF_BIRTH { get; set; }
        public Nullable<System.DateTimeOffset> CLAIM_ENTRY_DATE { get; set; }
        public long CLAIM_NO { get; set; }
        public Nullable<System.DateTime> DOS { get; set; }
        public Nullable<decimal> CLAIM_TOTAL { get; set; }
        public Nullable<decimal> Amount_Paid { get; set; }
        public Nullable<decimal> Amount_Adjusted { get; set; }
        public Nullable<decimal> Amount_Due { get; set; }
        public string Primary_Status { get; set; }
        public string PRIMARY_PAYER { get; set; }
        public string PRIMARY_POLICY_NUMBER { get; set; }
        public string Secondary_Status { get; set; }
        public string SECONDARY_PAYER { get; set; }
        public string SECONDARY_POLICY_NUMBER { get; set; }
        public string Other_Status { get; set; }
        public Nullable<int> AGING_DAYS { get; set; }
        public string Patient_Status { get; set; }
    }
}