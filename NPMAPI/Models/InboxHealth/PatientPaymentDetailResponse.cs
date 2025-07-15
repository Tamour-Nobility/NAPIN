using System.Collections.Generic;

namespace NPMAPI.Models.InboxHealth
{
    public class PatientPaymentDetailResponse : BaseResponse
    {
        public Meta meta { get; set; }
        public List<InvoicePayment> invoice_payments { get; set; }
    }

    public class InvoicePayment
    {
        public string payment_id { get; set; }
        public string invoice_id { get; set; }
        public string id { get; set; }
        public string paid_amount_cents { get; set; }
        public string reversal { get; set; }
        public string reversal_date { get; set; }
        public PaymentReasonsInvoicePayments payment_reasons_invoice_payments { get; set; }
    }

    public class Meta
    {
        public int total_pages { get; set; }
        public int current_page { get; set; }
        public int per_page { get; set; }
        public bool has_more { get; set; }
    }

    public class PaymentReason
    {
        public int id { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
        public string code { get; set; }
        public string description { get; set; }
        public int enterprise_id { get; set; }
    }

    public class PaymentReasonsInvoicePayments
    {
        public string id { get; set; }
        public PaymentReason payment_reason { get; set; }
        public string invoice_payment_id { get; set; }
        public string amount_cents { get; set; }
    }
}