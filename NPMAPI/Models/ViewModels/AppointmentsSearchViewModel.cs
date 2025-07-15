using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace NPMAPI.Models.ViewModels
{
    public class AppointmentsSearchViewModel
    {
        [Required]
        [DisplayName("Date From")]
        public DateTime sDate { get; set; }
        [Required]
        [DisplayName("Date To")]
        public DateTime eDate { get; set; }
        [Required]
        [DisplayName("Practice Code")]
        public long practiceCode { get; set; }

        public long patientAccount { get; set; }
        [Required]
        [DisplayName("Provider")]
        public List<SelectListViewModel> providers { get; set; }
        [Required]
        [DisplayName("Locations")]
        public List<SelectListViewModel> locations { get; set; }

    }
}