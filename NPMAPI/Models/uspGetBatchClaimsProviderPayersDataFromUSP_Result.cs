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
    
    public partial class uspGetBatchClaimsProviderPayersDataFromUSP_Result
    {
        public string Provider_Payer_Id { get; set; }
        public long Payer_Id { get; set; }
        public long Billing_Provider_Id { get; set; }
        public string Provider_Identification_Number_Type { get; set; }
        public string Provider_Identification_Number { get; set; }
        public string Box_33_Type { get; set; }
        public System.DateTime Validation_Expiry_Date { get; set; }
    }
}
