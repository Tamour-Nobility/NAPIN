using System.Collections.Generic;

namespace NPMSyncWorker.Models
{
    internal class Practice
    {
        public int id { get; set; }
        public string name { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
        public int enterprise_id { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string logo_file_name { get; set; }
        public string logo_content_type { get; set; }
        public int logo_file_size { get; set; }
        public string logo_updated_at { get; set; }
        public string address_line_1 { get; set; }
        public string address_line_2 { get; set; }
        public string practice_summary { get; set; }
        public string monday_open { get; set; }
        public string monday_close { get; set; }
        public string tuesday_open { get; set; }
        public string tuesday_close { get; set; }
        public string wednesday_open { get; set; }
        public string wednesday_close { get; set; }
        public string thursday_open { get; set; }
        public string thursday_close { get; set; }
        public string friday_open { get; set; }
        public string friday_close { get; set; }
        public string saturday_open { get; set; }
        public string saturday_close { get; set; }
        public string sunday_open { get; set; }
        public string sunday_close { get; set; }
        public string email { get; set; }
        public string website { get; set; }
        public string phone { get; set; }
        public string zip { get; set; }
        public string npi { get; set; }
        public string time_zone { get; set; }
        public int deposit_account_id { get; set; }
        public List<ExternalId> external_ids { get; set; }
        public string links { get; set; }
    }



}
