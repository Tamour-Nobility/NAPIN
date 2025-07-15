using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NPMAPI.Models.ViewModels
{
    public class PracticeAppointmentReasonCreateVM
    {
        [Required]
        public long PracticeCode { get; set; }
        [Required]
        public long ProviderCode { get; set; }
        [Required]
        public long LocationCode { get; set; }
        [Required]
        public List<long> ReasonsIds { get; set; }
    }
}