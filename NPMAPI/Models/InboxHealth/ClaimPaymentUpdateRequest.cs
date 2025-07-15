namespace NPMAPI.Models.InboxHealth
{
    public class ClaimPaymentUpdateRequest
    {
        public long id { get; set; }
        public Payment payment { get; set; }
    }
    public class Payment
    {
        public decimal expected_amount_cents { get; set; }
        public string payment_method_type { get; set; }
    }
    public class ClaimPaymentUpdateResponse : BaseResponse
    {
        public Payment payment { get; set; }
    }
}