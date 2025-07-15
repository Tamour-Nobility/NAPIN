using System;
using System.Collections.Generic;

namespace NPMSyncWorker.Models
{
    internal class Patient
    {
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string date_of_birth { get; set; }
        public int enterprise_id { get; set; }
        public string middle_name { get; set; }
        public string address_line_1 { get; set; }
        public string address_line_2 { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string zip { get; set; }
        public string sex { get; set; }
        public string ssn { get; set; }
        public bool primary { get; set; }
        public string cell_phone { get; set; }
        public string secondary_phone { get; set; }
        public string email { get; set; }
        public string health_record_id { get; set; }
        public string notes { get; set; }
        public string race { get; set; }
        public string marital_status { get; set; }
        public string language { get; set; }
        public string ethnicity { get; set; }
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
        public List<EmergencyContactsAttribute> emergency_contacts_attributes { get; set; }
        public string billing_status { get; set; }
        public bool auto_charge { get; set; }
        public int auto_charge_max_cents { get; set; }
        public string primary_care_provider_name { get; set; }
        public string primary_care_provider_city { get; set; }
        public string primary_care_provider_state { get; set; }
        public DateTime hold_until { get; set; }
        public bool force_bad_address_mailing { get; set; }
    }
}
