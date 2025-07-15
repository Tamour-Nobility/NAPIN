using System;
using System.Collections.Generic;

namespace NPMAPI.Models
{
    public class ProcedureViewModel
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
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
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
        public string oldProcCode { get; set; }
        public string oldAlternateCode { get; set; }


    }
    public class ProceduresSearchViewModel
    {
        public string ProcedureCode { get; set; }
        public string ProcedureDescription { get; set; }
    }
    public class ProcedureDropdownListViewModel
    {
        public List<SelectListViewModel> Modifiers { get; set; }
        public List<SelectListViewModel> POS { get; set; }
        public List<SelectListViewModel> Category { get; set; }

    }
}