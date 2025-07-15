using System.ComponentModel.DataAnnotations;

namespace NPMAPI.Models.ViewModels
{
    public class DoInquiryByX12DataModel
    {
        public string GediPayerID { get; set; }
        [Required]
        public string X12Input { get; set; }
    }

    public class DoInquiryModel
    {
        // Source Details
        public string GediPayerID { get; set; }
        // Receiver Details
        public string ProviderFirstName { get; set; }
        public string ProviderLastName { get; set; }
        public string Npi { get; set; }
        // Subscriber Details
        public string InsuredFirstName { get; set; }
        public string InsuredLastName { get; set; }
        public string InsuranceNum { get; set; }
        public string InsuredDOB { get; set; }
        public string InsuredGender { get; set; }
        public string InsuredSSN { get; set; }
        public string InsuredState { get; set; }
        public string ProviderId { get; set; }

        // Dependent Details
        public int DependentFirstName { get; set; }
        public int DependentLastName { get; set; }
        public int DependentDob { get; set; }
        public int DependentGender { get; set; }
    }
}