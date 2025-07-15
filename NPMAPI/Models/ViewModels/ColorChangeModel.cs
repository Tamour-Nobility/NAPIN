using System.ComponentModel.DataAnnotations;

namespace NPMAPI.Models.ViewModels
{
    public class ColorChangeModel
    {
        [Required]
        [Display(Name = "Reason Id")]
        public long PracAppReasonID { get; set; }
        [Required]
        [Display(Name = "Color")]
        public string ReasonColor { get; set; }
    }
}