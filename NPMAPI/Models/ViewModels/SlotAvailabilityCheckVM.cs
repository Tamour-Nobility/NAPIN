using System;
using System.ComponentModel.DataAnnotations;

namespace NPMAPI.Models.ViewModels
{
    public class SlotAvailabilityRequestVM
    {
        [Required]
        public long practiceCode { get; set; }
        [Required]
        public long providerCode { get; set; }
        [Required]
        public long locationCode { get; set; }
        public DateTime? date { get; set; }
        public string timeFrom { get; set; }
        public long? duration { get; set; }
        public long? appointmentId { get; set; }
    }

    public class SlotAvailabilityResponseVm
    {
        public bool IsBlocked { get; set; }
        public Check_Duplicate_Blocking_Rule_Result BlockedTimes { get; set; }
        public GetOfficeTiming_Result OfficeTiming { get; set; }
        public GetAppointment_Result Appointment { get; set; }
        public dynamic Reasons { get; set; }
    }
}