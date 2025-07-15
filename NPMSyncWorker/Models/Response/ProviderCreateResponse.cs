using System.Collections.Generic;

namespace NPMSyncWorker.Models.Response
{
    internal class ProviderCreateResponse : BaseResponse
    {
        public int id { get; set; }
        public int enterprise_id { get; set; }
        public string first_name { get; set; }
        public string middle_name { get; set; }
        public string last_name { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public string specialty { get; set; }
        public string photo_file_name { get; set; }
        public string photo_content_type { get; set; }
        public string photo_file_size { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
        public string suffix { get; set; }
        public string certification { get; set; }
        public string message { get; set; }
        public string npi { get; set; }
        public string status { get; set; }
        public string provider_code { get; set; }
        public int profile_id { get; set; }
        public int deposit_account_id { get; set; }
        public List<ExternalId> external_ids { get; set; }
        public string links { get; set; }
    }
}
