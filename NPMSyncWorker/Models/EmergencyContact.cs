namespace NPMSyncWorker.Models
{
    internal class EmergencyContact
    {
        public int id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string cell_phone { get; set; }
        public string secondary_phone { get; set; }
        public string relationship { get; set; }
        public int patient_id { get; set; }
    }
}
