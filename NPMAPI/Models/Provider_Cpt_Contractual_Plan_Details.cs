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
    
    public partial class Provider_Cpt_Contractual_Plan_Details
    {
        public long Provider_Cpt_Plan_Detail_Id { get; set; }
        public string Provider_Cpt_Plan_Id { get; set; }
        public Nullable<long> Practice_Code { get; set; }
        public Nullable<long> Provider_Code { get; set; }
        public Nullable<long> Facility_Code { get; set; }
        public Nullable<long> Location_Code { get; set; }
        public string Insurance_State { get; set; }
        public string Cpt_Code { get; set; }
        public string Cpt_Description { get; set; }
        public string Cpt_Modifier { get; set; }
        public string POS { get; set; }
        public Nullable<long> InsPayer_Id { get; set; }
        public Nullable<System.DateTime> Start_Date { get; set; }
        public Nullable<System.DateTime> End_Date { get; set; }
        public Nullable<decimal> Non_Facility_Participating_Fee_ctrl_Fee { get; set; }
        public Nullable<decimal> Non_Facility_Non_Participating_Fee_ctrl_Fee { get; set; }
        public Nullable<decimal> Facility_Participating_Fee_ctrl_Fee { get; set; }
        public Nullable<decimal> Facility_Non_Participating_Fee_ctrl_Fee { get; set; }
        public Nullable<bool> Deleted { get; set; }
        public string Created_By { get; set; }
        public Nullable<System.DateTime> Created_Date { get; set; }
        public string Modified_By { get; set; }
        public Nullable<System.DateTime> Modified_Date { get; set; }
        public Nullable<bool> ISMTBC_Defined { get; set; }
    }
}
