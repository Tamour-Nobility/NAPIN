namespace NPMSyncWorker.Models
{
    internal class EnterprisePlan
    {
        public int id { get; set; }
        public string plan_name { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
        public int per_email_rate_cents { get; set; }
        public int per_outbound_sms_rate_cents { get; set; }
        public int per_paper_rate_cents { get; set; }
        public int per_outbound_call_minute_rate_cents { get; set; }
        public int monthly_fee_cents { get; set; }
        public bool custom { get; set; }
        public int check_fee_cents { get; set; }
        public int card_transaction_rate { get; set; }
        public int amex_transaction_rate { get; set; }
        public int amex_debit_transaction_rate { get; set; }
        public int amex_prepaid_transaction_rate { get; set; }
        public int amex_credit_transaction_rate { get; set; }
        public int discover_debit_transaction_rate { get; set; }
        public int discover_prepaid_transaction_rate { get; set; }
        public int discover_credit_transaction_rate { get; set; }
        public int mastercard_debit_transaction_rate { get; set; }
        public int mastercard_prepaid_transaction_rate { get; set; }
        public int mastercard_credit_transaction_rate { get; set; }
        public int visa_debit_transaction_rate { get; set; }
        public int visa_prepaid_transaction_rate { get; set; }
        public int visa_credit_transaction_rate { get; set; }
        public int terminal_amex_debit_transaction_rate { get; set; }
        public int terminal_amex_prepaid_transaction_rate { get; set; }
        public int terminal_amex_credit_transaction_rate { get; set; }
        public int terminal_discover_debit_transaction_rate { get; set; }
        public int terminal_discover_prepaid_transaction_rate { get; set; }
        public int terminal_discover_credit_transaction_rate { get; set; }
        public int terminal_mastercard_debit_transaction_rate { get; set; }
        public int terminal_mastercard_prepaid_transaction_rate { get; set; }
        public int terminal_mastercard_credit_transaction_rate { get; set; }
        public int terminal_visa_debit_transaction_rate { get; set; }
        public int terminal_visa_prepaid_transaction_rate { get; set; }
        public int terminal_visa_credit_transaction_rate { get; set; }
        public int amex_debit_transaction_flat_fee_cents { get; set; }
        public int amex_prepaid_transaction_flat_fee_cents { get; set; }
        public int amex_credit_transaction_flat_fee_cents { get; set; }
        public int discover_debit_transaction_flat_fee_cents { get; set; }
        public int discover_prepaid_transaction_flat_fee_cents { get; set; }
        public int discover_credit_transaction_flat_fee_cents { get; set; }
        public int mastercard_debit_transaction_flat_fee_cents { get; set; }
        public int mastercard_prepaid_transaction_flat_fee_cents { get; set; }
        public int mastercard_credit_transaction_flat_fee_cents { get; set; }
        public int visa_debit_transaction_flat_fee_cents { get; set; }
        public int visa_prepaid_transaction_flat_fee_cents { get; set; }
        public int visa_credit_transaction_flat_fee_cents { get; set; }
        public int terminal_amex_debit_transaction_flat_fee_cents { get; set; }
        public int terminal_amex_prepaid_transaction_flat_fee_cents { get; set; }
        public int terminal_amex_credit_transaction_flat_fee_cents { get; set; }
        public int terminal_discover_debit_transaction_flat_fee_cents { get; set; }
        public int terminal_discover_prepaid_transaction_flat_fee_cents { get; set; }
        public int terminal_discover_credit_transaction_flat_fee_cents { get; set; }
        public int terminal_mastercard_debit_transaction_flat_fee_cents { get; set; }
        public int terminal_mastercard_prepaid_transaction_flat_fee_cents { get; set; }
        public int terminal_mastercard_credit_transaction_flat_fee_cents { get; set; }
        public int terminal_visa_debit_transaction_flat_fee_cents { get; set; }
        public int terminal_visa_prepaid_transaction_flat_fee_cents { get; set; }
        public int terminal_visa_credit_transaction_flat_fee_cents { get; set; }
        public int inbox_health_transaction_rate { get; set; }
        public int transaction_flat_fee_cents { get; set; }
        public bool enable_preauth { get; set; }
        public bool enable_estimates { get; set; }
        public bool enable_billing { get; set; }
        public bool enable_eligibility { get; set; }
        public bool enable_checkin { get; set; }
        public bool enable_full_service { get; set; }
        public string stripe_plan_id { get; set; }
        public int transfer_flat_fee_cents { get; set; }
    }



}
