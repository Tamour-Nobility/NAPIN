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
    
    public partial class SP_GetClaimById_Result
    {
        public long Claim_No { get; set; }
        public Nullable<long> Patient_Account { get; set; }
        public string First_Name { get; set; }
        public string Last_Name { get; set; }
        public Nullable<decimal> CO_Payment { get; set; }
        public Nullable<decimal> Deductions { get; set; }
        public string Pri_Sec_Oth_Type { get; set; }
        public Nullable<System.DateTime> DOS { get; set; }
        public Nullable<decimal> amt_paid { get; set; }
        public Nullable<decimal> claim_total { get; set; }
        public Nullable<decimal> amt_due { get; set; }
        public Nullable<decimal> Adjustment { get; set; }
        public Nullable<bool> isPosted { get; set; }
    }
}
