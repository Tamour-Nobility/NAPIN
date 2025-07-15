using System.ComponentModel.DataAnnotations;

namespace NPMAPI.Models
{
    public class ProviderFeeScheduleSearchVM
    {
        [Required]
        [Display(Name = "Practice Code")]
        public long PracticeCode { get; set; }
        public long? ProviderCode { get; set; }
        public string State { get; set; }
        public long? FacilityCode { get; set; }
        public long? LocationCode { get; set; }
        public long? InsuranceId { get; set; }
        public string InsuranceOrSelfPay { get; set; }
        public string FaciltiyOrLocation { get; set; }
    }
    public class CreateProviderCPTPlanVM : ProviderFeeScheduleSearchVM
    {
        [Required(ErrorMessage = "Please specify standard/percentage.")]
        [Display(Name = "Standard or Percentage")]
        public string StandardOrPercentAge { get; set; }
        public decimal PercentageHigher { get; set; }
        public bool Customize { get; set; }
        public bool ModificationAllowed { get; set; }
        public bool Computed { get; set; }
    }
}