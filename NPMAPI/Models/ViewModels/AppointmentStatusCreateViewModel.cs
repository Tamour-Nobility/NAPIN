using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NPMAPI.Models.ViewModels
{
    public class PracticeAppointmentStatusCreateVM
    {
        [Required]
        [Display(Name = "Appointment Status Id")]
        public List<int> AppointmentStatusesId { get; set; }
        [Required]
        [Display(Name = "Practice Code")]
        public long PracticeCode { get; set; }
    }
}