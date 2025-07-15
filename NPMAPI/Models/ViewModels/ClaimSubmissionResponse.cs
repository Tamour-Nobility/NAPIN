using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NPMAPI.Models.ViewModels
{
    public class ClaimSubmissionResponse
    {
        public long Practice_Code { get; set; }
        public string Practice_Name { get; set; }
        public long Claim_No { get; set; }
        public long Account_No { get; set; }
        public string Patient_Name { get; set; }
        public DateTime DOS { get; set; }

        public string Status_Date { get; set; }
        public DateTime Submission_Date { get; set; }
        public decimal Charge_amount { get; set; }

        public string Insurance_Name { get; set; }
        public string response_Level { get; set; }
        public string Status { get; set; }
        public string Code { get; set; }
        public string Rejection_reason { get; set; }
        public string File_name { get; set; }
     
   

   
    }
}