namespace NPMSyncWorker.Models.Request
{
    internal class PatientCreateRequest
    {
        public bool force { get; set; }
        public Patient patient { get; set; }
    }
}
