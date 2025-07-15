using System;

namespace NPMAPI.Models.InboxHealth
{
    public class PatientPaymentProcessedModel
    {
        public PatientPaymentProcessed @object { get; set; }
    }

    public class PaymentAttributes
    {
        public int patient_id { get; set; }
        public string @object { get; set; }
    }

    public class PaymentData
    {
        public PaymentAttributes attributes { get; set; }
    }

    public class PaymentLinks
    {
        public decimal payment_adjustments { get; set; }
        public object invoices { get; set; }
        public object invoice_payments { get; set; }
        public object attachments { get; set; }
    }

    public class PatientPaymentProcessed
    {
        public int id { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public DateTime date_scheduled { get; set; }
        public int patient_payment_info_id { get; set; }
        public object payment_plan_length { get; set; }
        public int payment_plan_id { get; set; }
        public string status { get; set; }
        public bool successful { get; set; }
        public DateTime submitted_at { get; set; }
        public int transfer_id { get; set; }
        public DateTime date_cancelled { get; set; }
        public string card_brand { get; set; }
        public long bank_number { get; set; }
        public string payment_method_type { get; set; }
        public string description { get; set; }
        public long batch_number { get; set; }
        public int secondary_patient_payment_info_id { get; set; }
        public int created_by_user_id { get; set; }
        public int total_fee_cents { get; set; }
        public int expected_amount_cents { get; set; }
        public int refunded_amount_cents { get; set; }
        public int adjusted_fee_cents { get; set; }
        public int enterprise_id { get; set; }
        public long patient_id { get; set; }
        public DateTime transferred_at { get; set; }
        public long gift_card_number { get; set; }
        public int applied_amount_cents { get; set; }
        public long practice_id { get; set; }
        public long doctor_id { get; set; }
        public bool voided { get; set; }
        public bool signed { get; set; }
        public string dispute_status { get; set; }
        public int dispute_id { get; set; }
        public int reversal_amount_cents { get; set; }
        public object tag { get; set; }
        public string payer_description { get; set; }
        public int merged_payment_id { get; set; }
        public int deposit_account_id { get; set; }
        public string sub_type { get; set; }
        public bool is_posting { get; set; }
        public int last_4_digits { get; set; }
        public bool has_attachments { get; set; }
        public bool can_change_submitted_at_date { get; set; }
        public PaymentLinks links { get; set; }
        public bool is_archive_pending { get; set; }
        public PaymentData data { get; set; }
    }
}