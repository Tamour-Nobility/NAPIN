using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NPMAPI.Models.ViewModels
{
    public class NegativeBalanceReportResponse
    {
        public long Practice_Code { get; set; }
        public string Practice_Name { get; set; }
        public long Patient_Account { get; set; }
        public string Patient_Name { get; set; }
        public long Claim_No { get; set; }
        public long? Created_By { get; set; }
        public string DOS { get; set; }  // Date of Service
        public string Bill_Date { get; set; }
        public string Attending_Physician { get; set; }
        public string Billing_Physician { get; set; }
        public decimal? Claim_Total { get; set; }
        public decimal? Amount_Paid { get; set; }
        public decimal? Adjustment { get; set; }
        public decimal? Amount_Due { get; set; }  // Total Due Amount (Negative)
        public decimal? Primary_Ins_Payment { get; set; }
        public decimal? Secondary_Ins_Payment { get; set; }
        public decimal? Other_Ins_Payment { get; set; }
        public decimal? Patient_Payment { get; set; }
        public string Primary_Status { get; set; }
        public string Primary_Payer { get; set; }
        public string Secondary_Status { get; set; }
        public string Secondary_Payer { get; set; }
        public string Other_Status { get; set; }
        public string Other_Payer { get; set; }
        public string Patient_Status { get; set; }
        public string Aging_Payer_Type { get; set; }
        public string Aging_Payer { get; set; }
        public decimal? Patient_Credit_Balance { get; set; }
        public decimal? Insurance_Overpaid { get; set; }
        public DateTimeOffset? Moved_Date { get; set; }  // Negative indicator for moved date
    }
}