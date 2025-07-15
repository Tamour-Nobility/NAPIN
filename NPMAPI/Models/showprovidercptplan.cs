using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NPMAPI.Models
{
    public class showprovidercptplan
    {
        public string Provider_Cpt_Plan_Id { get; set; }
        public Nullable<long> Practice_Code { get; set; }
        public Nullable<long> Provider_Code { get; set; }
        public Nullable<long> InsPayer_Id { get; set; }
        public string Insurance_State { get; set; }
        public Nullable<long> Location_Code { get; set; }
        public Nullable<long> Facility_Code { get; set; }
        public string Cpt_Plan { get; set; }
        public Nullable<decimal> Percentage_Higher { get; set; }
        public Nullable<bool> self_pay { get; set; }
        public string Provid_FName { get; set; }
        public string Provid_LName { get; set; }



    }
}