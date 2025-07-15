namespace NPMAPI.Models
{
    public class DeleteInvoicepaymentsRequest
    {
        public DeleteInoicePayment payment { get; set; }
        public long id { get; set; }

        public DeleteInvoicepaymentsRequest()
        {
            payment = new DeleteInoicePayment();
        }
    }

    public class DeleteInoicePayment
    {
        public string voided { get; set; } = "true"; // Initialize the default value directly
    }
}
