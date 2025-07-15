using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NPMAPI.Models.ViewModels
{
    public class ScrubberReportResponse
    {
        public long Practice_Code { get; set; }
        public string Practice_Name { get; set; }
        public Nullable<System.DateTime> Rejection_Date { get; set; }
        public long Account_Number { get; set; }
        public string Patient_Name { get; set; }
        public long Claim_Number { get; set; }
        public Nullable<System.DateTime> Date_Of_Service { get; set; }
        public Nullable<decimal> Charge_amount { get; set; }
        public string Rejection_Reason { get; set; }
        public string Insurance_Name { get; set; }
   
        public string Action_Taken { get; set; }
 
    }
}