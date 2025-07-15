using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;



namespace NPMAPI.Models
{
    public class providercptplanModel
    {
        public string Provider_Cpt_Plan_Id { get; set; }
        public long Practice_Code { get; set; }
        public long Provider_Code { get; set; }
        public Nullable<long> InsPayer_Id { get; set; }
        public string Insurance_State { get; set; }
        public Nullable<long> Location_Code { get; set; }
        public Nullable<long> Facility_Code { get; set; }
        public string Cpt_Plan { get; set; }
        public decimal Percentage_Higher { get; set; }
        public bool self_pay { get; set; }
    }
}