namespace NPMSyncWorker.Models
{
    internal class PatientPlan
    {
        public int id { get; set; }
        public string member_id { get; set; }
        public string first_name { get; set; }
        public string middle_name { get; set; }
        public string last_name { get; set; }
        public string date_of_birth { get; set; }
        public int payer_id { get; set; }
        public int imported_payer_id { get; set; }
        public string group_id { get; set; }
        public bool primary { get; set; }
        public int patient_id { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
        public bool secondary { get; set; }
        public bool dependent { get; set; }
        public bool soft_delete { get; set; }
        public EligibilityResponses eligibility_responses { get; set; }
    }
}
