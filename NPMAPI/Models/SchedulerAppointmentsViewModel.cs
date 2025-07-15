using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace NPMAPI.Models
{
    public class AppointmentViewModel
    {
        public long? AppointmentId { get; set; }
        public string Notes { get; set; }
        public long? PatientAccount { get; set; }
        public SelectListViewModel Patient { get; set; }
        public DateTime? AppointmentDateTime { get; set; }
        public string TimeFrom { get; set; }
        public long? Duration { get; set; }
        public long? ReasonId { get; set; }
        public SelectListViewModel Reason { get; set; }
        public int? StatusId { get; set; }
        public List<SelectListViewModel> Statuses { get; set; }
        public long? LocationCode { get; set; }
        public SelectListViewModel Location { get; set; }
        public long? AttendingPhysician { get; set; }
        public long? ProviderCode { get; set; }
        public SelectListViewModel Provider { get; set; }
        public dynamic Reasons { get; set; }
    }

    public class AppointmentCreateModel
    {
        [MaxLength(500)]
        [Display(Name = "Comment")]
        public string Notes { get; set; }
        [Required]
        [Display(Name = "Patient")]
        public long PatientAccount { get; set; }
        [Required]
        [Display(Name = "Appointment Date")]
        public DateTime AppointmentDateTime { get; set; }
        [MaxLength(15)]
        [Required]
        [Display(Name = "Appointment Time")]
        public string TimeFrom { get; set; }
        [Required]
        [Display(Name = "Appointment Duration")]
        public long Duration { get; set; }
        [Required]
        [Display(Name = "Appointment Reason")]
        public long ReasonId { get; set; }
        [Required]
        [Display(Name = "Appointment Status")]
        public int StatusId { get; set; }
        public long? AppointmentId { get; set; }
        [Required]
        [Display(Name = "Location")]
        public long LocationCode { get; set; }
        [Required]
        [Display(Name = "Provider")]
        public long ProviderCode { get; set; }
        [Required]
        public long PracticeCode { get; set; }
    }
}