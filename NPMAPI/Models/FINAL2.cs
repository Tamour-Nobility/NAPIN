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
    using System.Collections.Generic;
    
    public partial class FINAL2
    {
        public long CLAIM_PAYMENTS_ID { get; set; }
        public string REFERENCE_CLAIM_NO { get; set; }
        public string CLAIM_NO { get; set; }
        public string PAYMENT_TYPE { get; set; }
        public string PAYMENT_SOURCE { get; set; }
        public System.DateTimeOffset DATE_ENTRY { get; set; }
        public System.DateTimeOffset DATE_ADJ_PAYMENT { get; set; }
        public Nullable<System.DateTime> DATE_FILING { get; set; }
        public Nullable<decimal> AMOUNT { get; set; }
        public Nullable<decimal> AMOUNT_APPROVED { get; set; }
        public Nullable<decimal> AMOUNT_PAID { get; set; }
        public Nullable<decimal> AMOUNT_ADJUSTED { get; set; }
        public string DETAILS { get; set; }
        public Nullable<decimal> REJECT_AMOUNT { get; set; }
        public string REJECT_TYPE { get; set; }
        public string PAID_PROC_CODE { get; set; }
        public string CHARGED_PROC_CODE { get; set; }
        public int UNITS { get; set; }
        public Nullable<long> INSURANCE_ID { get; set; }
        public string CHECK_NO { get; set; }
        public string MODI_CODE1 { get; set; }
        public string MODI_CODE2 { get; set; }
        public int DELETED { get; set; }
        public Nullable<long> CREATED_BY { get; set; }
        public System.DateTimeOffset CREATED_DATE { get; set; }
        public string ERA_CATEGORY_CODE { get; set; }
        public string ERA_ADJUSTMENT_CODE { get; set; }
        public string ENTERED_FROM { get; set; }
    }
}
