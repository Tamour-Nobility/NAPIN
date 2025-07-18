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
    
    public partial class SP_ProcedureSearch_Result
    {
        public string ProcedureCode { get; set; }
        public string ProcedureDescription { get; set; }
        public Nullable<double> ProcedureDefaultCharge { get; set; }
        public string ProcedureDefaultModifier { get; set; }
        public string ProcedurePosCode { get; set; }
        public string ProcedureTosCode { get; set; }
        public Nullable<System.DateTime> EffectiveDate { get; set; }
        public string GenderAppliedOn { get; set; }
        public string AgeCategory { get; set; }
        public string AgeRangeCriteria { get; set; }
        public Nullable<int> AgeFrom { get; set; }
        public Nullable<int> AgeTo { get; set; }
        public Nullable<bool> Deleted { get; set; }
        public Nullable<long> CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<long> ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public Nullable<System.DateTime> ProcedureEffectiveDate { get; set; }
        public Nullable<bool> IncludeInEDI { get; set; }
        public Nullable<bool> clia_number { get; set; }
        public Nullable<long> CategoryId { get; set; }
        public Nullable<int> MxUnits { get; set; }
        public string LongDescription { get; set; }
        public string Comments { get; set; }
        public Nullable<int> TimeMin { get; set; }
        public string Qualifier { get; set; }
        public string CPTDosage { get; set; }
        public Nullable<bool> NOC { get; set; }
        public Nullable<int> ComponentCode { get; set; }
        public string Alternate_Code { get; set; }
        public string Category_Desc { get; set; }
    }
}
