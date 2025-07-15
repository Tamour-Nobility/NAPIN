using System;
using Newtonsoft.Json;

namespace NPMAPI.Models.InboxHealth
{
    public class InboxHealthEvent : InboxHealthEventBaseEntity
    {
        [JsonProperty("event_data")]
        public EventData EventData { get; set; }
    }

    public class EventData
    {
        public Object @object { get; set; }
    }

    public class Object
    {
        public object id { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public string date_scheduled { get; set; }
        public DateTime reversal_date { get; set; }
        public string patient_payment_info_id { get; set; }
        public object payment_plan_length { get; set; }
        public string payment_plan_id { get; set; }
        public long payment_id { get; set; }
        public string invoice_id { get; set; }
        public string status { get; set; }
        public bool successful { get; set; }
        public DateTime? submitted_at { get; set; }
        public string transfer_id { get; set; }
        public string date_cancelled { get; set; }
        public string card_brand { get; set; }
        public string bank_number { get; set; }
        public string payment_method_type { get; set; }
        public string description { get; set; }
        public string batch_number { get; set; }
        public string secondary_patient_payment_info_id { get; set; }
        public int? created_by_user_id { get; set; }
        public int? total_fee_cents { get; set; }
        public int? expected_amount_cents { get; set; }
        public int? refunded_amount_cents { get; set; }
        public int? paid_amount_cents { get; set; }
        public int? adjusted_fee_cents { get; set; }
        public int? enterprise_id { get; set; }
        public long? patient_id { get; set; }
        public string transferred_at { get; set; }
        //public object gift_card_number { get; set; }
        public int? applied_amount_cents { get; set; }
        public long? practice_id { get; set; }
        public string doctor_id { get; set; }
        public bool voided { get; set; }
        public bool signed { get; set; }
        public object dispute_status { get; set; }
        public int? dispute_id { get; set; }
        public int? reversal_amount_cents { get; set; }
        public string reversal { get; set; }
        //public object tag { get; set; }
        public string payer_description { get; set; }
        public string merged_payment_id { get; set; }
        public string deposit_account_id { get; set; }
        public string sub_type { get; set; }
        public bool is_posting { get; set; }
        public string last_4_digits { get; set; }
        public bool has_attachments { get; set; }
        public bool can_change_submitted_at_date { get; set; }
        public Links links { get; set; }
        public bool is_archive_pending { get; set; }
        public Data data { get; set; }
    }

    public class Links
    {
        public object payment_adjustments { get; set; }
        public object invoices { get; set; }
        public object invoice_payments { get; set; }
        public object attachments { get; set; }
    }

    public class Data
    {
        public Attributes attributes { get; set; }
    }

    public class Attributes
    {
        public int patient_id { get; set; }
        public string @object { get; set; }
    }
}