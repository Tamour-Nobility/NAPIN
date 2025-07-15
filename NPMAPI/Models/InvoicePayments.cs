using NPMAPI.Models.InboxHealth;
using System;
using System.Collections.Generic;

namespace NPMAPI.Models
{
    public class InvoicePayment
    {
        public long payment_id { get; set; }
        public long invoice_id { get; set; }
        public long id { get; set; }
        public long paid_amount_cents { get; set; }
        public string reversal { get; set; }
        public string reversal_date { get; set; }
        public List<object> payment_reasons_invoice_payments { get; set; }
        public long enterprise_id { get; set; }
    }

    public class Meta
    {
        public int total_pages { get; set; }
        public int current_page { get; set; }
        public int per_page { get; set; }
        public bool has_more { get; set; }
    }

    public class InvoicePaymentsResponse : BaseResponse
    {
        public Meta meta { get; set; }
        public List<InvoicePayment> invoice_payments { get; set; }
    }
}
