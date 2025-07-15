using System.ComponentModel.DataAnnotations;

namespace NPMAPI.Models.ViewModels
{
    public class PracticeDxModel
    {
        [Required]
        [Display(Name = "Practice Code")]
        public long PracticeCode { get; set; }

        [Required]
        [Display(Name = "Diagnosis Code")]
        public string DiagCode { get; set; }
    }
}