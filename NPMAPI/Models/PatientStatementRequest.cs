using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NPMAPI.Models
{
    public class PatientStatementRequest
    {
        [Display(Name = "Practice Code")]
        [Required]
        public long PracticeCode { get; set; }
        public string Format { get; set; }
        public List<StatementRequest> statementRequest { get; set; }
        public bool Confirmation { get; set; }
    }

    public class StatementRequest
    {
        public long PatientAccount { get; set; }
        public List<long> ExcludedClaimsIds { get; set; }
    }

    public class PatientStatementViewModel
    {
        public long ClaimNo { get; set; }
        public DateTime? DOS { get; set; }
        public string ProcedureCode { get; set; }
        public string ProcedureDescription { get; set; }
        public decimal? Amount { get; set; }
        public long? PatientAccount { get; set; }
        public DateTimeOffset? BillDate { get; set; }
        public decimal DurPerClaim { get; set; }
    }
    public class rollingReportforSP
    {
        public long practice_code { get; set; }
        public string prac_name { get; set; }
        public string DataType { get; set; }
        public decimal Amount { get; set; }
        public int Month { get; set; }
        public string Year { get; set; }
        public string Month_Year { get; set; }
    }
    public class PatientStatementViewModelFromSpXML
    {
        [Display(Name = "Claim#")]
        public long claim_no { get; set; }
        [Display(Name = "Date")]
        public DateTime? date1 { get; set; } // DOS
        [Display(Name = "Code")]
        public string proc_code { get; set; }
        [Display(Name = "Description")]
        public string description { get; set; }
        [Display(Name = "Amount")]
        public decimal? amount { get; set; }
        [Display(Name = "Rejection Amount")]
        public decimal? reject_amt { get; set; }
        [Display(Name = "Practice Code")]
        public long PRACTICE_CODE { get; set; }
        [Display(Name = "Practice Name")]
        public string PRAC_NAME { get; set; }
        [Display(Name = "Practice Address")]
        public string PAT_BILLING_ADDRESS { get; set; }
        public string PAT_BILLING_CITY_STATE_ZIP { get; set; }
        public string BILLINGQUESTIONPHONE { get; set; }
        public string patient_account { get; set; }
        [Display(Name = "Patient Name")]
        public string NAME { get; set; }
        [Display(Name = "Patient Address")]
        public string PAT_ADDRESS { get; set; }
        public string CITY { get; set; }
        public string STATE { get; set; }
        public string ZIP { get; set; }
        [Display(Name = "Amount Due")]
        public decimal? amtDue { get; set; }

        public decimal patAmtDue { get; set; }
    }

    public class PatientStatementViewModelFromSp
    {
        [Display(Name = "Claim#")]
        public long claim_no { get; set; }
        [Display(Name = "Date")]
        public DateTime? date1 { get; set; } // DOS
        [Display(Name = "Code")]
        public string proc_code { get; set; }
        public string paid_proc_code { get; set; }
        [Display(Name = "Description")]
        public string description { get; set; }
        [Display(Name = "Amount")]
        public decimal? amount { get; set; }
        [Display(Name = "Rejection Amount")]
        public decimal? reject_amt { get; set; }
        [Display(Name = "Practice Code")]
        public long PRACTICE_CODE { get; set; }
        [Display(Name = "Practice Name")]
        public string PRAC_NAME { get; set; }
        public string PRAC_ADDRESS { get; set; }
        public string ADDRESS { get; set; }
        public string PRAC_PHONE { get; set; }
        [Display(Name = "Practice Address")]
        public string PAT_BILLING_ADDRESS { get; set; }
        public string PAT_BILLING_CITY_STATE_ZIP { get; set; }
        public string BILLING_QUESTION_PHONE { get; set; }
        public string patient_account { get; set; }
        [Display(Name = "Patient Name")]
        public string NAME { get; set; }
        [Display(Name = "Patient Address")]
        public string PAT_ADDRESS { get; set; }
        public string CITY { get; set; }
        public string STATE { get; set; }
        public string ZIP { get; set; }
        [Display(Name = "Amount Due")]
        public decimal? amtDue { get; set; }
        public long seq_no { get; set; }
        public decimal patAmtDue { get; set; }

        public decimal? Pat_Amount { get; set; }
        public decimal? Ins_Amount { get; set; }
        public decimal? Pat_Amount_Row_Due { get; set; }
    }

    public class PatientStatementViewModelFromSpforprint
    {
        [Display(Name = "Claim#")]
        public long claim_no { get; set; }
        [Display(Name = "Date")]
        public DateTime? date1 { get; set; } // DOS
        [Display(Name = "Code")]
        public string proc_code { get; set; }
        [Display(Name = "Description")]
        public string paid_proc_code { get; set; }
        public string description { get; set; }
        [Display(Name = "Amount")]
        public decimal? amount { get; set; }
        [Display(Name = "Rejection Amount")]
        public decimal? reject_amt { get; set; }
        [Display(Name = "Practice Code")]
        public long PRACTICE_CODE { get; set; }
        [Display(Name = "Practice Name")]
        public string PRAC_NAME { get; set; }
        public string PRAC_ADDRESS { get; set; }
        public string ADDRESS { get; set; }
        public string PRAC_PHONE { get; set; }
        [Display(Name = "Practice Address")]
        public string PAT_BILLING_ADDRESS { get; set; }
        public string PAT_BILLING_CITY_STATE_ZIP { get; set; }
        public string BILLING_QUESTION_PHONE { get; set; }
        public string patient_account { get; set; }
        [Display(Name = "Patient Name")]
        public string NAME { get; set; }
        [Display(Name = "Patient Address")]
        public string PAT_ADDRESS { get; set; }
        public string CITY { get; set; }
        public string STATE { get; set; }
        public string ZIP { get; set; }
        [Display(Name = "Amount Due")]
        public decimal? amtDue { get; set; }
        public long seq_no { get; set; }
        public decimal patAmtDue { get; set; }
        public decimal? Pat_Amount { get; set; }
        public decimal? Ins_Amount { get; set; }
        public decimal? Pat_Amount_Row_Due { get; set; }
    }

    public class PatientStatementResponseClaimInfo
    {
        public long ClaimNo { get; set; }
        public decimal AmountDue { get; set; }
    }

    public class PatientStatementResponse
    {
        public List<PatientStatementResponseClaimInfo> ClaimsInfo { get; set; }
        public string Path { get; set; }
        public PatientStatementResponse()
        {
            ClaimsInfo = new List<PatientStatementResponseClaimInfo>();
        }
    }
}