//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace NPMAPI.Models
{
    using System;
    
    public partial class USP_PATIENT_AGING_REPORT_NPM_Result
    {
        public long PRACTICE_CODE { get; set; }
        public string PRAC_NAME { get; set; }
        public long PATIENT_ACCOUNT { get; set; }
        public string PATIENT_NAME { get; set; }
        public long CLAIM_NO { get; set; }
        public string DOS { get; set; }
        public Nullable<int> MONTH { get; set; }
        public Nullable<int> YEAR { get; set; }
        public string BILL_DATE { get; set; }
        public string CLAIM_ENTRY_DATE { get; set; }
        public string ATTENDING_PHYSICIAN { get; set; }
        public string BILLING_PHYSICIAN { get; set; }
        public string RESOURCE_PHYSICIAN { get; set; }
        public string LOCATION_NAME { get; set; }
        public string FACILITY_NAME { get; set; }
        public string FACILITY_TYPE { get; set; }
        public Nullable<decimal> CLAIM_TOTAL { get; set; }
        public Nullable<decimal> AMT_PAID { get; set; }
        public Nullable<decimal> ADJUSTMENT { get; set; }
        public Nullable<decimal> AMT_DUE { get; set; }
        public Nullable<decimal> PRI_INS_PAYMENT { get; set; }
        public Nullable<decimal> SEC_INS_PAYMENT { get; set; }
        public Nullable<decimal> OTH_INS_PAYMENT { get; set; }
        public Nullable<decimal> PATIENT_PAYMENT { get; set; }
        public string PRI_STATUS { get; set; }
        public string PRI_PAYER { get; set; }
        public string PRI_POLICY_NUMBER { get; set; }
        public string SEC_STATUS { get; set; }
        public string SEC_PAYER { get; set; }
        public string SEC_POLICY_NUMBER { get; set; }
        public string OTH_STATUS { get; set; }
        public string OTH_PAYER { get; set; }
        public string OTH_POLICY_NUMBER { get; set; }
        public string PAT_STATUS { get; set; }
        public string SLOT { get; set; }
        public Nullable<int> Aging { get; set; }
    }
}
