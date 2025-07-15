namespace NPMAPI.Models
{
    public class StatmentDownloadRequestModel
    {
        public long PracticeCode { get; set; }
        public long PatientAccount { get; set; }
        public string Message { get; set; }
        public long[] ExcludedClaimsIds { get; set; }
    }
}