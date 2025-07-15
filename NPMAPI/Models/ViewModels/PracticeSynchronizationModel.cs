using System.ComponentModel.DataAnnotations;

namespace NPMAPI.Models.ViewModels
{
    public class PracticeSynchronizationModel
    {
        [Required]
        public long PracticeId { get; set; }
        [Required]
        public long VendorId { get; set; }
    }
}