namespace NPMAPI.Models.InboxHealth
{
    public class ProviderUpdateRequest
    {
        public long id { get; set; }
        public Doctor doctor { get; set; }
    }
    public class Doctor
    {
        public string first_name { get; set; }
        public string middle_name { get; set; }
        public string last_name { get; set; }
        public string phone { get; set; }
        public string specialty { get; set; }
        public string email { get; set; }
        public string message { get; set; }
        public string npi { get; set; }
        public string status { get; set; }
        public string provider_code { get; set; }
    }
    public class ProviderUpdateResponse : BaseResponse
    {
        public Doctor doctor { get; set; }
    }
}