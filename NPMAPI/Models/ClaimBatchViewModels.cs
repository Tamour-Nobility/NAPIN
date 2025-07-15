using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NPMAPI.Models
{
    public class BatchCreateViewModel
    {
        public long BatchId { get; set; }
        public string BatchName { get; set; }
        public long? ProviderCode { get; set; }
        [Required]
        public DateTime Date { get; set; }
        public string DateStr { get; set; }
        public string BatchType { get; set; }
        public string batch_claim_type { get; set; }
        public string submission_type { get; set; }
        [Required]
        public long PracticeCode { get; set; }
    }
    public class BatchListViewModel
    {
        public long BatchId { get; set; }
        public string BatchName { get; set; }
        public long? ProviderCode { get; set; }
        public DateTime Date { get; set; }
        public string BatchType { get; set; }
        public long PracticeCode { get; set; }
        public bool? BatchLock { get; set; }
        public int TotalClaims { get; set; }
    }
    public class BatchListRequestViewModel
    {
        [Required]
        public long PracticeCode { get; set; }
        public long? ProviderCode { get; set; }
        public string prac_type { get; set; }
    }
    public class BatchListResponseViewModel
    {
        public int TotalBatch { get; set; }
        public List<SP_GetBatchDetail_Result> Batches { get; set; }
    }

    public class AddInBatchRequestViewModel
    {
        [MinLength(1)]
        public long[] ClaimIds { get; set; }
        [Required]
        [MinLength(1)]
        public long[] ClaimInsuranceIds { get; set; }
        [Required]
        public int BatchId { get; set; }
        public long PracticeCode { get; set; }
        public string SystemIP { get; set; }
        public string Type { get; set; }
    }
    public class BatchUploadViewModelBatchCSI
    {
        public long PatientAccount { get; set; }
        public long ClaimId { get; set; }
        public long BatchId { get; set; }
        public long? PracticeCode { get; set; }
        public DateTime? DOS { get; internal set; }
        public string PatientName { get; internal set; }
        public string Claim_type { get; set; }
        public string Submission_Type { get; set; }
        public string batch_status { get; set; }
        public string batch_name { get; set; }
        public string File_Path { get; set; }
        public string batch_type { get; set; }
    }
    public class LockBatchRequestViewModel
    {
        [Required]
        public long BatchId { get; set; }
        public long? UserId { get; set; }
    }

    public class HoldBatchRequestViewModel
    {
        [Required]
        public long BatchId { get; set; }
        public long? UserId { get; set; }
        public bool? holdStatus { get; set; }
    }
    public class DateRangeViewModel
    {
        [Required]
        public DateTime BeginDate { get; set; }
        [Required]
        public string BeginDateStr { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        [Required]
        public string EndDateStr { get; set; }
    }
    public class BatchUploadViewModel
    {
        public long PatientAccount { get; set; }
        public long ClaimId { get; set; }
        public long BatchId { get; set; }
        public long? PracticeCode { get; set; }
        public DateTime? DOS { get; internal set; }
        public string PatientName { get; internal set; }
        public string Claim_type { get; set; }
        public string Submission_Type { get; set; }
        public string Batch_claim_Type { get; set; }
        public string Pri_Status { get; set; }
        public string Sec_Status { get; set; }
    }

    public class BatchUploadRequest
    {
        public long[] BatcheIds { get; set; }
    }
    public class CSIClaimBatchUploadRequest
    {
        public long?[] BatcheIds { get; set; }
        public long ClaimNo { get; set; }
        public long InsuranceId { get; set; }
    }
    public class ViewBatchRequest
    {
        public long claim_no { get; set; }
        public string claim_type { get; set; }
        public string batch_claim_type { get; set; }
        public string Pri_Status { get; set; }
        public string Sec_Status { get; set; }

    }

    public class BatchClaimSubmissionResponse
    {
        public dynamic response { get; set; }
        public long ClaimId { get; set; }
        public long? PracticeCode { get; set; }
        public long BatchId { get; set; }
        public long insurance_id { get; set; }
    }

    public class ProcessedBatchResponse
    {
        public long BatchId { get; set; }
        public string BatchName { get; set; }
        public long? ProviderId { get; set; }
        public string ProviderName { get; set; }
        public long? UploadedBy { get; set; }
        public string UploadedByName { get; set; }
        public DateTime? UplodedOn { get; set; }
        public DateTime? Date { get; internal set; }
        public string BatchStatus { get; internal set; }
        public string BatchStatusDetail { get; internal set; }
        public DateTime? DateProcessed { get; internal set; }
    }
}