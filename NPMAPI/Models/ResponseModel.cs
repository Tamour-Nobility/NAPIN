using System;
using System.Collections.Generic;

namespace NPMAPI.Models
{
    public class ResponseModel
    {
        public string Status { get; set; }
        public dynamic Response { get; set; }
        public dynamic Response1 { get; set; }
        public string AdditionalInfo { get; set; }  
        public string STCStatus { get; set; }
        public string STCDescription { get; set; }
        public string stcStatusCategoryDescription { get; set; }  
        public string StcStatusDescription { get; set; }
        public string StcCategoryDescription { get; set; }       
        public string DTPDates { get; set; }
    }

    public class ResponseDataModel
    {
        public ClaimsAndERAs claimsAndERAs { get; set; }
    }

    public class ClaimsAndERAs
    {
        public int claims_submitted { get; set; }
        public int pending_claims { get; set; }
        public int total_posted_eras { get; set; }
        public int total_unposted_eras { get; set; }
        public int total_patient_accounts { get; set; }
        public int total_claims { get; set; }
        public int total_statements_sent { get; set; }
    }


    public class ResponseModelForE
    {
        public string Status { get; set; }

        public dynamic Response { get; set; }
        public dynamic Data { get; set; }

        public dynamic SuccessCode { get; set; }

        public string[] SuccessCodeText { get; set; }

    }
    public class ResponseModelForClaimSubmission
    {
        public string Status { get; set; }
        public dynamic Response { get; set; }
        public List<ClaimSubmission> obj { get; set; }
    }
    public class ResponseModelForEdiHistory
    {
        public string Status { get; set; }
        public dynamic Response { get; set; }
        public List<EdiHistoryModel> obj { get; set; }
    }
    public class ResponseModelforClaimRejection
    {
        public string Status { get; set; }
        public dynamic Response { get; set; }
        public string Message { get; set; }
        public int TotalCount { get; set; }
        public List<ClaimRejectionModel> obj { get; set; }
    }

  
}