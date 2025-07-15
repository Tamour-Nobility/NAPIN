using System.Collections.Generic;

namespace NPMSyncWorker.Models
{
    internal class Doctor
    {
        public string first_name { get; set; }
        public string middle_name { get; set; }
        public string last_name { get; set; }
        public string phone { get; set; }
        public string specialty { get; set; }
        public string email { get; set; }
        public string certification { get; set; }
        public string suffix { get; set; }
        public string message { get; set; }
        public string npi { get; set; }
        public int enterprise_id { get; set; }
        public string status { get; set; }
        public string provider_code { get; set; }
        public object deposit_account_id { get; set; }
        public List<ShiftsAttribute> shifts_attributes { get; set; }
    }
}
