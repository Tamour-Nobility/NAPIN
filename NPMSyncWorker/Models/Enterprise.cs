using System.Collections.Generic;

namespace NPMSyncWorker.Models
{
    internal class Enterprise
    {
        public string name { get; set; }
        public string address_line_1 { get; set; }
        public string address_line_2 { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string zip { get; set; }
        public int sales_tax { get; set; }
        public string time_zone { get; set; }
        public List<PracticesAttribute> practices_attributes { get; set; }
        public int minimum_bill_amount_cents { get; set; }
        public string ein { get; set; }
        public string support_phone_number { get; set; }
        public bool use_support_phone_number { get; set; }
        public string default_service_code { get; set; }
        public string default_statement_memo { get; set; }
        public int days_before_patient_balance_reminder { get; set; }
        public int days_before_patient_balance_reminder_with_paper_statements { get; set; }
        public int estimate_down_payment_rate { get; set; }
        public string default_quick_pay_description { get; set; }
        public bool enable_scheduled_eligible_requests { get; set; }
        public string logo_background_color { get; set; }
        public string statement_descriptor { get; set; }
        public int cancellation_fee_window { get; set; }
        public int cancellation_fee_cents { get; set; }
        public int reschedule_window { get; set; }
        public int soonest_bookable_time_window { get; set; }
        public bool apply_payment { get; set; }
        public bool checkin_card_on_file_required { get; set; }
        public bool patient_billing_active { get; set; }
        public bool auto_charge { get; set; }
        public int auto_charge_max_cents { get; set; }
        public int past_due_balance_days { get; set; }
        public int collection_warning_balance_days { get; set; }
        public bool enable_scheduled_appointment_reminders { get; set; }
        public int max_paper_bills_per_billing_cycle_count { get; set; }
        public string billing_contact_email { get; set; }
        public string billing_name { get; set; }
        public string billing_business_name { get; set; }
        public string billing_address_line_1 { get; set; }
        public string billing_address_line_2 { get; set; }
        public string billing_city { get; set; }
        public string billing_state { get; set; }
        public string billing_zip { get; set; }
        public bool send_initial_statement_sms { get; set; }
        public int default_billing_cycle_template_id { get; set; }
        public bool twenty_four_hours_appt_reminders { get; set; }
        public bool fourty_eight_hours_appt_reminders { get; set; }
        public bool one_week_appt_reminders { get; set; }
        public int self_pay_imported_payer_id { get; set; }
        public string make_checks_payable_to { get; set; }
        public string custom_collection_copy { get; set; }
        public bool enable_advanced_payment_routing { get; set; }
        public bool enable_unapplied_payment_only { get; set; }
        public bool enable_simplified_account_view { get; set; }
        public string post_checkin_message { get; set; }
        public bool accepts_credit_card { get; set; }
        public bool color_statements { get; set; }
        public bool first_class { get; set; }
        public bool return_envelope { get; set; }
        public bool perforation { get; set; }
        public string patient_support_email { get; set; }
    }
}
