using System;
using System.Collections.Generic;

namespace NPMSyncWorker.Models.Response
{
    internal class EnterpriseCreateResponse : BaseResponse
    {
        public int id { get; set; }
        public string name { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
        public string address_line_1 { get; set; }
        public string address_line_2 { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string ein { get; set; }
        public string enterprise_payment_date_scheduled { get; set; }
        public int minimum_bill_amount_cents { get; set; }
        public string support_phone_number { get; set; }
        public string use_support_phone_number { get; set; }
        public string payment_phone_number { get; set; }
        public string default_service_code { get; set; }
        public int sales_tax { get; set; }
        public string time_zone { get; set; }
        public string default_service_location { get; set; }
        public string zip { get; set; }
        public string default_statement_memo { get; set; }
        public int days_before_patient_balance_reminder { get; set; }
        public int days_before_patient_balance_reminder_with_paper_statements { get; set; }
        public List<Practice> practices { get; set; }
        public EnterprisePlan enterprise_plan { get; set; }
        public int estimate_down_payment_rate { get; set; }
        public string bcbs_payer_id { get; set; }
        public string default_quick_pay_description { get; set; }
        public bool enable_scheduled_eligible_requests { get; set; }
        public string logo_background_color { get; set; }
        public string default_checkin_routes { get; set; }
        public string enable_giftcards { get; set; }
        public string statement_descriptor { get; set; }
        public string has_logo_base64 { get; set; }
        public int cancellation_fee_window { get; set; }
        public int cancellation_fee_cents { get; set; }
        public int reschedule_window { get; set; }
        public int soonest_bookable_time_window { get; set; }
        public bool patient_billing_active { get; set; }
        public bool apply_payment { get; set; }
        public string default_eligible_npi { get; set; }
        public bool checkin_card_on_file_required { get; set; }
        public int past_due_balance_days { get; set; }
        public int collection_warning_balance_days { get; set; }
        public int max_paper_bills_per_billing_cycle_count { get; set; }
        public bool twenty_four_hours_appt_reminders { get; set; }
        public bool one_week_appt_reminders { get; set; }
        public bool accepts_credit_card { get; set; }
        public bool color_statements { get; set; }
        public bool first_class { get; set; }
        public bool return_envelope { get; set; }
        public bool perforation { get; set; }
        public List<ExternalId> external_ids { get; set; }
        public int account_manager_id { get; set; }
        public int admin_organization_id { get; set; }
        public string billing_business_name { get; set; }
        public string billing_address_line_1 { get; set; }
        public string billing_address_line_2 { get; set; }
        public string billing_city { get; set; }
        public string billing_state { get; set; }
        public string billing_zip { get; set; }
        public string patient_support_email { get; set; }
        public string payment_plan_minimums { get; set; }
        public string paper_bill_includes_payment_reasons { get; set; }
        public string enable_live_chat { get; set; }
        public string churned { get; set; }
        public string enable_unposted_payment_tab { get; set; }
        public string connect_onboarding_uuid { get; set; }
        public string links { get; set; }
    }
}
