namespace NPMAPI.Models.InboxHealth
{
    public class ClaimUpdateRequest
    {
        public long id { get; set; }
        public Invoice invoice { get; set; }
    }

    public class Invoice
    {
        public long patient_id { get; set; }
        public long practice_id { get; set; }
        public string date_of_service { get; set; }
    }

    public class ClaimUpdateResponse : BaseResponse
    {
        public Invoice invoice { get; set; }
    }
}