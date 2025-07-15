

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace NPMAPI.Models.ViewModels
{
    public class ProviderWorkingDayTimeViewModel
    {
        [Required]
        [DisplayName("Practice Code")]
        public long PRACTICE_CODE { get; set; }
        [Required]
        [DisplayName("Provider Code")]
        public long PROVIDER_CODE { get; set; }
        [Required]
        [DisplayName("Location Code")]
        public long LOCATION_CODE { get; set; }
        [Required]
        public string weekday_id { get; set; }
        public string dayNam { get; set; }
        public TimeSpan? Time_From { get; set; }
        public TimeSpan? Time_To { get; set; }
        public string AMPMTIMEFROM { get; set; }
        public string AMPMTIMEto { get; set; }
        public TimeSpan? Break_time_From { get; set; }
        public TimeSpan? Break_Time_To { get; set; }
        public bool Enable_Break { get; set; }
        [Required]
        public bool Day_on { get; set; }
        public int? time_slot_size { get; set; }
        public long Provider_Working_Days_Time_Id { get; set; }
    }
    public class OfficeTimingCreateViewModel
    {
        public List<ProviderWorkingDayTimeViewModel> timings { get; set; }
        public ProviderSchedulesViewModel schedule { get; set; }
    }
}