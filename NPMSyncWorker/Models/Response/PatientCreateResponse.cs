using System.Collections.Generic;

namespace NPMSyncWorker.Models.Response
{

    internal class PatientCreateResponse : BaseResponse
    {
        public int id { get; set; }
        public int user_id { get; set; }
        public int enterprise_id { get; set; }
        public string first_name { get; set; }
        public string middle_name { get; set; }
        public string last_name { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
        public string address_line_1 { get; set; }
        public string address_line_2 { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string cell_phone { get; set; }
        public string secondary_phone { get; set; }
        public string date_of_birth { get; set; }
        public bool primary { get; set; }
        public string ssn { get; set; }
        public string sex { get; set; }
        public bool removed { get; set; }
        public string email { get; set; }
        public string zip { get; set; }
        public string email_two { get; set; }
        public int health_record_id { get; set; }
        public string notes { get; set; }
        public int balance_cents { get; set; }
        public string soft_delete { get; set; }
        public List<PatientPlan> patient_plans { get; set; }
        public string last_notified_at { get; set; }
        public string race { get; set; }
        public string language { get; set; }
        public string ethnicity { get; set; }
        public string marital_status { get; set; }
        public string employment_status { get; set; }
        public string employer_name { get; set; }
        public string contact_preference { get; set; }
        public string occupation { get; set; }
        public string pharmacy_name { get; set; }
        public string pharmacy_address_line_1 { get; set; }
        public string pharmacy_address_line_2 { get; set; }
        public string pharmacy_city { get; set; }
        public string pharmacy_state { get; set; }
        public string pharmacy_phone_number { get; set; }
        public string maiden_name { get; set; }
        public string home_phone { get; set; }
        public string work_phone { get; set; }
        public string practice_id { get; set; }
        public bool sms_opt_out { get; set; }
        public bool phone_opt_out { get; set; }
        public List<EmergencyContact> emergency_contacts { get; set; }
        public List<ExternalId> external_ids { get; set; }
        public string statement_cycle_started_at { get; set; }
        public string collection_warning_letter_date { get; set; }
        public bool needs_card_update { get; set; }
        public int cached_balance_cents { get; set; }
        public string balance_error_message { get; set; }
        public string allow_self_directed_payment_plan { get; set; }
        public string force_bad_address_mailing { get; set; }
        public string last_collection_report_sent_at { get; set; }
        public string guarantor_id { get; set; }
        public string merged_patient_id { get; set; }
        public string corrected_address_line_1 { get; set; }
        public string corrected_address_line_2 { get; set; }
        public string corrected_city { get; set; }
        public string corrected_state { get; set; }
        public string corrected_zip { get; set; }
        public string billing_status { get; set; }
        public string billing_error_message { get; set; }
        public string auto_charge { get; set; }
        public string auto_charge_max_cents { get; set; }
        public string current_billing_cycle_paper_bills_count { get; set; }
        public string hold_until { get; set; }
        public string is_auto_applying_payments { get; set; }
        public string links { get; set; }
    }
}
