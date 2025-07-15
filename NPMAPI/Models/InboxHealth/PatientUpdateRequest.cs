namespace NPMAPI.Models.InboxHealth
{
    public class PatientUpdateRequest
    {
        public long id { get; set; }
        public Patient patient { get; set; }
    }
    public class Patient
    {
        public string first_name { get; set; }
        public string middle_name { get; set; }
        public string last_name { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public int enterprise_id { get; set; }
        public string address_line_1 { get; set; }
        public string address_line_2 { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string cell_phone { get; set; }
        public string home_phone { get; set; }
        public string secondary_phone { get; set; }
        public string date_of_birth { get; set; }
        public bool primary { get; set; }
        public string ssn { get; set; }
        public string sex { get; set; }
        public bool removed { get; set; }
        public string zip { get; set; }

        //Added below two props by Hamza Ikhlaq for  inbox statement fixation
        public string billing_status { get; set; }
        public bool precollection { get; set; }
    }
    public class PatientUpdateResponse : BaseResponse
    {
        Patient patient { get; set; }
    }
}