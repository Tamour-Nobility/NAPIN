using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NPMAPI.Models
{
    public class ClaimSubmission
    {
        public long Practice_Code { get; set; }
        public string PracticeName { get; set; }
        public string PatientName { get; set; }
        public long AccountNo { get; set; }
        public long ClaimNo { get; set; }
        public string Status { get; set; }
        public string File277CA { get; set; }
        public string message { get; set; }
        public string statusLevel { get; set; }
        public string FileEntryDate { get; set; }
        public string StatusDate { get; set; }
        public DateTime SubmitDate { get; set; }
        public decimal Chargeamount { get; set; }
        public string InsuranceName { get; set; }
        public DateTime DOS { get; set; }
        public string batchName { get; set; }
        public string batchStatus { get; set; }
        public string RejectionCode { get; set; }

    }

    public class EdiHistoryModel
    {
        public long ClaimNo { get; set; }
        public string File277CA { get; set; }
        public string message { get; set; }
        public string Status { get; set; }
        public string statusLevel { get; set; }
        public string StatusDate { get; set; }
        public DateTime SubmitDate { get; set; }
        public string InsuranceName { get; set; }
        public DateTime DOS { get; set; }
        public string RejectionCode { get; set; }
    }
    public class ClaimRejectionModel
    {
        public long AccountNumber { get; set; }
        public long ClaimNo { get; set; }
        public string PatientName { get; set; }
        public string PatientFirstName { get; set; }
        public string PatientLastName { get; set; }
        public string ProviderName { get; set; }
        public DateTime DOS { get; set; }
        public DateTime SubmissionDate { get; set; }
        public decimal Chargeamount { get; set; }
        public string InsuranceName { get; set; }
        public string Status { get; set; }
        public string RejectionCode { get; set; }
        public string ResponseLevel { get; set; }
        public string RejectionReason { get; set; }
        public DateTime date_created { get; set; }

        public DateTime StatusDate { get; set; }
    }
}