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
    
    public partial class Claims_Condition_Code
    {
        public long Claims_Condition_Code_Id { get; set; }
        public Nullable<long> Practice_Code { get; set; }
        public Nullable<long> Claim_No { get; set; }
        public string ConditionCode { get; set; }
        public string Descriptions { get; set; }
        public Nullable<System.DateTime> Date { get; set; }
        public Nullable<bool> Isdeleted { get; set; }
        public Nullable<long> Created_By { get; set; }
        public Nullable<System.DateTime> Created_Date { get; set; }
        public Nullable<long> Modified_By { get; set; }
        public Nullable<System.DateTime> Modified_Date { get; set; }
    }
}
