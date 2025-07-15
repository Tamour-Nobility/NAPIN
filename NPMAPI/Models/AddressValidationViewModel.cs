using System;
using System.ComponentModel.DataAnnotations;

namespace NPMAPI.Models
{
    public class Components
    {

        public string primary_number { get; set; }
        public string street_predirection { get; set; }
        public string street_name { get; set; }
        public string street_suffix { get; set; }
        public string city_name { get; set; }
        public string default_city_name { get; set; }
        public string state_abbreviation { get; set; }
        public string zipcode { get; set; }
        public string plus4_code { get; set; }
        public string delivery_point { get; set; }
        public string delivery_point_check_digit { get; set; }
    }
    public class Metadata
    {
        public string record_type { get; set; }
        public string zip_type { get; set; }
        public string county_fips { get; set; }
        public string county_name { get; set; }
        public string carrier_route { get; set; }
        public string congressional_district { get; set; }
        public string rdi { get; set; }
        public string elot_sequence { get; set; }
        public string elot_sort { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public string precision { get; set; }
        public string time_zone { get; set; }
        public int utc_offset { get; set; }
        public bool dst { get; set; }
    }
    public class Analysis
    {
        public string dpv_match_code { get; set; }
        public string dpv_footnotes { get; set; }
        public string dpv_cmra { get; set; }
        public string dpv_vacant { get; set; }
        public string active { get; set; }
    }
    public class ValidateAddressResponseViewModel
    {
        public string input_id { get; set; }
        public int input_index { get; set; }
        public int candidate_index { get; set; }
        public string delivery_line_1 { get; set; }
        public string last_line { get; set; }
        public string delivery_point_barcode { get; set; }
        public Components components { get; set; }
        public Metadata metadata { get; set; }
        public Analysis analysis { get; set; }
    }
    public class ValidateAddreesRequestViewModel
    {
        public string auth_id { get; set; }
        public string auth_token { get; set; }
        public string candidates { get; set; }
        [Required]
        public string street { get; set; }
        [Required]
        public string city { get; set; }
        [Required]
        public string state { get; set; }
        [Required]
        public string zipcode { get; set; }
    }
    public class PatientSummaryVM
    {
        public long PatientAccount { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string MI { get; set; }
        public DateTime? DOB { get; set; }
        public int? Gender { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string ZIP { get; set; }
        public string State { get; set; }
    }
    public class ClaimSummaryVM
    {
        public long Claim_No { get; set; }
        public long? Patient_Account { get; set; }
        public DateTime? DOS { get; set; }
        public decimal? Amt_Due { get; set; }
        public decimal? Amt_Paid { get; set; }
        public decimal? Claim_Total { get; set; }
    }
}