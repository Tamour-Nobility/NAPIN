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
    
    public partial class SP_PRACTICEPROVIDERPAYERSEARCH_Result
    {
        public long INSURANCE_PAYER_ID { get; set; }
        public string INSURANCE_PAYER_DESCRIPTION { get; set; }
        public string SUBMISSION_SETUP_DESCRIPTION { get; set; }
        public string INSURANCE_PAYER_STATE { get; set; }
        public string PROVIDER_PAYER_ID { get; set; }
        public string PROVIDER_PAYER_GROUP { get; set; }
        public string INDIVIDUAL_NPI { get; set; }
        public string GROUP_NPI { get; set; }
        public bool EDI_SETUP_PENDING { get; set; }
        public bool TRIAL_SUBMISSION { get; set; }
        public bool IS_PARTICIPATING { get; set; }
        public bool STOP_PATIENT_BILLING { get; set; }
        public bool CREDENTIALING { get; set; }
        public string CREDENTIALING_DATE { get; set; }
        public string CREDENTIALING_EXPIRY_DATE { get; set; }
        public string INSURANCE_837_ID { get; set; }
    }
}
