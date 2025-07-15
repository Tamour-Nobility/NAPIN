using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace NPMAPI.Models
{
    public class PatientDemographicViewModel
    {
        public long Patient_Account { get; set; }
        public string alternate_id { get; set; }
        public string Last_Name { get; set; }
        public string First_Name { get; set; }
        public string MI { get; set; }
        public string SSN { get; set; }
        public Nullable<System.DateTime> Date_Of_Birth { get; set; }
        public int? Gender { get; set; }
        public int? Marital_Status { get; set; }
        public int? Address_Type { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZIP { get; set; }
        public string Home_Phone { get; set; }
        public string Business_Phone { get; set; }
        public Nullable<long> Financial_Guarantor { get; set; }
        public string Financial_Guarantor_Name { get; set; }
        public int? Gurantor_Relation { get; set; }
        public string Email_Address { get; set; }
        public string Eligibility_Status { get; set; }
        public Nullable<long> Location_Code { get; set; }
        public Nullable<System.DateTime> Expiry_Date { get; set; }
        public string Chart_Id { get; set; }
        public Nullable<bool> Move_Collection { get; set; }
        public Nullable<long> Practice_Code { get; set; }
        public Nullable<bool> Patient_Payment_Plan { get; set; }
        public Nullable<bool> Patient_Statement { get; set; }
        public string Patient_Type { get; set; }
        public string Scan_No { get; set; }
        public Nullable<bool> @as { get; set; }
        public string PTL_Web_Appearnce_Days { get; set; }
        public Nullable<long> Created_By { get; set; }
        public Nullable<System.DateTimeOffset> Created_Date { get; set; }
        public Nullable<long> Modified_By { get; set; }
        public Nullable<System.DateTimeOffset> Modified_Date { get; set; }
        public Nullable<bool> Deleted { get; set; }
        public Nullable<bool> PTL_STATUS { get; set; }
        public Nullable<System.DateTime> PTL_Date { get; set; }
        public Nullable<System.DateTime> Scan_Date { get; set; }
        public Nullable<long> Provider_Code { get; set; }
        public string Notes { get; set; }
        public string Cell_Phone { get; set; }
        public string Ext { get; set; }
        public Nullable<long> Referring_Physician { get; set; }
        public Nullable<bool> Address_To_Guarantor { get; set; }
        public string Special_Billing_Note { get; set; }
        public Nullable<long> CREATED_FROM { get; set; }
        public string ALTERNATE_PHONE { get; set; }
        public Nullable<bool> Terminated { get; set; }
        public string Primary_Phone { get; set; }
        public int? Ethnicities { get; set; }
        public int? Race { get; set; }
        public int? Languages { get; set; }
        public bool? IsDeceased { get; set; }
        public DateTime? DeathDate { get; set; }
        public string DeathCause { get; set; }
        public Nullable<bool> Chk_Hospice { get; set; }
        public string Father_Cell { get; set; }
        public string Father_FName { get; set; }
        public string Father_LName { get; set; }
        public string Mother_Cell { get; set; }
        public string Mother_FName { get; set; }
        public string Mother_LName { get; set; }
        public string Spouse_Cell { get; set; }
        public string Spouse_Fname { get; set; }
        public string Spouse_Lname { get; set; }
        public Nullable<System.DateTime> Registration_Complete_Date { get; set; }
        public Nullable<bool> Prac_Address_PT_Billing { get; set; }
        public string RACE2 { get; set; }
        public string Address_Line2 { get; set; }
        public string Blood_Type { get; set; }
        public Nullable<long> Primary_Care_Physician { get; set; }
        public Nullable<bool> Is_Self_Pay { get; set; }
        public string Patient_Category { get; set; }
        public string Adjuster_Name { get; set; }
        public string Adjuster_Phone { get; set; }
        public Nullable<System.DateTime> Inactivation_Date { get; set; }
        public string Family_Id { get; set; }
        public string Fam_Relation { get; set; }
        public Nullable<bool> FamilyBit { get; set; }
        public Nullable<bool> GuarantorBit { get; set; }
        public string Family_Name { get; set; }
        public Nullable<bool> Address_To_Family { get; set; }
        public Nullable<int> ChronicCare { get; set; }
        public string Risk_Level { get; set; }
        public string Country { get; set; }
        //Added BY Hamza Akhlaq For Hold Patient Billing
        public bool Hold_Pt_Billing { get; set; }
        public DateTime? Hold_pt_until { get; set; }
        public Nullable<bool> IsDisable { get; set; }
        public List<Gender> GenderList { get; set; }
        public List<MaritalStatu> MaritalStatusList { get; set; }
        public List<ethnicity> EthiniciesList { get; set; }
        public List<Race> RaceList { get; set; }
        public List<Language> LanguageList { get; set; }
        public List<AddressType> AddressTypeList { get; set; }
        public List<Relationship> RelationshipList { get; set; }
        public List<Provider> ProviderList { get; set; }
        public List<Practice_Locations> PracticeLocationsList { get; set; }
        public List<Referral_Physicians> ReferringPhysicianList { get; set; }
        public string PrimaryInsuranceName { get; set; }
        public string SecondaryInsuranceName { get; set; }
        public string OtherInsuranceName { get; set; }
        public List<PatientInsuranceViewModel> PatientInsuranceList { get; set; }
        public List<Insurance_Payers> InsuranceList { get; set; }
        public bool emailnotonfile { get; set; }

        public string PicturePath { get; set; }
        public List<CityStateModel> ZipCodeCities { get; set; }
    }
    public class PatientCreateViewModel
    {
        //General
        public string alternate_id { get; set; }

        [DisplayName("Last Name")]
        [Required(AllowEmptyStrings = false)]
        [MaxLength(50, ErrorMessage = "The {0} is too long (Maximum required length is {1})")]
        [RegularExpression(@"^[a-zA-Z ]+$", ErrorMessage = "Please use only letters in {0}")]
        public string Last_Name { get; set; }

        [DisplayName("Middle Initial")]
        [MaxLength(1, ErrorMessage = "The {0} is too long (Maximum required length is {1})")]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Please use only letters in {0}")]
        public string MI { get; set; }

        [DisplayName("First Name")]
        [Required(AllowEmptyStrings = false)]
        [MaxLength(50, ErrorMessage = "The {0} is too long (Maximum required length is {1})")]
        [RegularExpression(@"^[a-zA-Z ]+$", ErrorMessage = "Please use only letters in {0}")]
        public string First_Name { get; set; }

        [DisplayName("Date of Birth")]
        [Required(AllowEmptyStrings = false)]
        [DataType(DataType.DateTime)]
        public DateTime? Date_Of_Birth { get; set; }

        [DisplayName("Social Security Number (SSN)")]
        [RegularExpression("[0-9]+", ErrorMessage = "Please use only numbers in {0}")]
        [MaxLength(9, ErrorMessage = "The {0} is too long (Maximum required length is {1})")]
        [MinLength(9, ErrorMessage = "The {0} is too short (Minimum required length is {1})")]
        public string SSN { get; set; }

        [DisplayName("Gender")]
        [Required(AllowEmptyStrings = false)]
        public int? Gender { get; set; }

        [DisplayName("Marital Status")]
        [Required(AllowEmptyStrings = false)]
        public int? Marital_Status { get; set; }

        [DisplayName("Race")]
        //[Required(AllowEmptyStrings = false)]
        public int? Race { get; set; }

        [DisplayName("Ethnicity")]
        //[Required(AllowEmptyStrings = false)]
        public int? Ethnicities { get; set; }

        [DisplayName("Preferred language")]
        //[Required(AllowEmptyStrings = false)]
        public int? Languages { get; set; }

        // Address
        [DisplayName("ZIP")]
        [Required(AllowEmptyStrings = false)]
        [MaxLength(9, ErrorMessage = "The {0} is too long (Maximum required length is {1})")]
        [MinLength(5, ErrorMessage = "The {0} is too short (Minimum required length is {1})")]
        [RegularExpression("[0-9]+", ErrorMessage = "Please use only numbers in {0}")]
        public string ZIP { get; set; }

        [DisplayName("City")]
        [Required(AllowEmptyStrings = false)]
        [MaxLength(50, ErrorMessage = "The {0} is too long (Maximum required length is {1})")]
        public string City { get; set; }

        [DisplayName("State")]
        [Required(AllowEmptyStrings = false)]
        [MaxLength(2, ErrorMessage = "The {0} is too long (Maximum required length is {1})")]
        [MinLength(2, ErrorMessage = "The {0} is too short (Minimum required length is {1})")]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Please use only letters in {0}")]
        public string State { get; set; }

        [DisplayName("Address")]
        [Required(AllowEmptyStrings = false)]
        [MaxLength(500, ErrorMessage = "The {0} is too long (Maximum required length is {1})")]
        public string Address { get; set; }

        [DisplayName("Address Type")]
        [Required(AllowEmptyStrings = false)]
        public int? Address_Type { get; set; }

        [DisplayName("Home Phone")]
        //[Required(AllowEmptyStrings = false)]
        [MaxLength(10, ErrorMessage = "The {0} is too long (Maximum required length is {1})")]
        [MinLength(10, ErrorMessage = "The {0} is too short (Minimum required length is {1})")]
        [RegularExpression("[0-9]+", ErrorMessage = "Please use only numbers in {0}")]
        public string Home_Phone { get; set; }

        [DisplayName("Cell Phone")]
        //[Required(AllowEmptyStrings = false)]
        [MaxLength(10, ErrorMessage = "The {0} is too long (Maximum required length is {1})")]
        [MinLength(10, ErrorMessage = "The {0} is too short (Minimum required length is {1})")]
        [RegularExpression("[0-9]+", ErrorMessage = "Please use only numbers in {0}")]
        public string Cell_Phone { get; set; }

        [DisplayName("Work Phone")]
        [MaxLength(10, ErrorMessage = "The {0} is too long (Maximum required length is {1})")]
        [MinLength(10, ErrorMessage = "The {0} is too short (Minimum required length is {1})")]
        [RegularExpression("[0-9]+", ErrorMessage = "Please use only numbers in {0}")]
        public string Business_Phone { get; set; }

        [DisplayName("Email Address")]
        [Required(AllowEmptyStrings = false)]
        [MaxLength(50, ErrorMessage = "The {0} is too long (Maximum required length is {1})")]
        [EmailAddress]
        public string Email_Address { get; set; }

        [DisplayName("Email not available")]
        [Required(AllowEmptyStrings = false)]
        public bool emailnotonfile { get; set; }

        // Financial Guarantor
        [DisplayName("Guarantor")]
        //[Required(AllowEmptyStrings = false)]
        public long? Financial_Guarantor { get; set; }

        [DisplayName("Guarantor Name")]
        //[Required(AllowEmptyStrings = false)]
        [MaxLength(100, ErrorMessage = "The {0} is too long (Maximum required length is {1})")]
        public string Financial_Guarantor_Name { get; set; }

        [DisplayName("Guarantor Relationship")]
        [Required(AllowEmptyStrings = false)]
        public int? Gurantor_Relation { get; set; }
        // Provider

        [DisplayName("Provider")]
        [Required(AllowEmptyStrings = false)]
        public long? Provider_Code { get; set; }

        [DisplayName("Location")]
        [Required(AllowEmptyStrings = false)]
        public long? Location_Code { get; set; }

        [DisplayName("Referring Physician")]
        public long? Referring_Physician { get; set; }

        // emergency
        [DisplayName("Emergency Contact Person Name")]
        [MaxLength(100, ErrorMessage = "The {0} is too long (Maximum required length is {1})")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Please use only letters in {0}")]
        public string Family_Name { get; set; }
        public string Fam_Relation { get; set; }

        [DisplayName("Emergency Person Relation")]
        public long? Family_Id { get; set; }

        [DisplayName("Emergency Person Phone")]
        [MaxLength(10, ErrorMessage = "The {0} is too long (Maximum required length is {1})")]
        [MinLength(10, ErrorMessage = "The {0} is too short (Minimum required length is {1})")]
        [RegularExpression("[0-9]+", ErrorMessage = "Please use only numbers in {0}")]
        public string Father_Cell { get; set; }
        public string PicturePath { get; set; }

        // death
        public bool? IsDeceased { get; set; }
        public DateTime? DeathDate { get; set; }
        public long Patient_Account { get; set; }
        public DateTime? Expiry_Date { get; set; }
        public long Practice_Code { get; set; }
        public bool PTL_STATUS { get; set; }
        //Added BY Hamza Akhlaq For Hold Patient Billing
        public bool Hold_Pt_Billing { get; set; }
        public DateTime? Hold_pt_until { get; set; }


        //Patient Insurances
        public List<PatientInsuranceViewModel> PatientInsuranceList { get; set; }


        public PatientCreateViewModel()
        {
            PTL_STATUS = false;
        }
    }

}