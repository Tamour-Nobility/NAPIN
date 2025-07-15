namespace NPMSyncWorker.Models
{
    internal class EligibilityResponses
    {
        public string id { get; set; }
        public string type { get; set; }
        public string patient_plan_id { get; set; }
        public string eligible_request { get; set; }
        public string eligible_response { get; set; }
        public string service_code { get; set; }
        public string service_code_label { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
        public string enterprise_id { get; set; }
        public string successful { get; set; }
        public string invoice_id { get; set; }
        public string service_location { get; set; }
        public string service_location_label { get; set; }
        public string scheduled_eligibility_request_id { get; set; }
    }
}
