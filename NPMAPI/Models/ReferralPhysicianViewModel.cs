using System;
using System.ComponentModel.DataAnnotations;

namespace NPMAPI.Models
{
    public class ReferralPhysicianViewModel
    {
        public long Referral_Code { get; set; }
        public string Referral_Lname { get; set; }
        public string Referral_Fname { get; set; }
        public string Referral_Mi { get; set; }
        public string Referral_Address { get; set; }
        public string Referral_City { get; set; }
        [MaxLength(2)]
        public string Referral_State { get; set; }
        public string Referral_Zip { get; set; }
        public string Referral_Phone { get; set; }
        public string Referral_Contact_Person { get; set; }
        public string Referral_Tax_Id { get; set; }
        public string Referral_License { get; set; }
        public string Referral_Upin { get; set; }
        public string Referral_Ssn { get; set; }
        public bool? Exported { get; set; } // Nullable boolean
        public bool? Recent_Use { get; set; } // Nullable boolean
        public DateTime? Export_Date { get; set; } // Nullable DateTime
        public bool? Deleted { get; set; } // Nullable boolean
        public long? Created_By { get; set; } // Nullable long
        public DateTimeOffset? Created_Date { get; set; } // Nullable DateTimeOffset
        public long? Modified_By { get; set; } // Nullable long
        public DateTimeOffset? Modified_Date { get; set; } // Nullable DateTimeOffset
        public string Pin { get; set; }
        public string NPI { get; set; }
        public string Referral_Fax { get; set; }
        public string Title { get; set; }
        public string Referral_Email { get; set; }
        public bool? In_Active { get; set; } // Nullable boolean
        public string Referral_Taxonomy_Code { get; set; }
        public long? SpecialityGroupNo { get; set; } // Nullable long
        public DateTime? Date_Of_Birth { get; set; } // Nullable DateTime
        public string Phone_Home { get; set; }
        public string Phone_Office { get; set; }
        public string Phone_Cell { get; set; }
        public string group_npi { get; set; }
        public string DEA_No { get; set; }
        public DateTime? DEA_Expiry_Date { get; set; } // Nullable DateTime
        public string WCB_Rating_Code { get; set; }
        public string WCB_Authorization_No { get; set; }
        public string SPECIALIZATION_CODE { get; set; }
    }
}