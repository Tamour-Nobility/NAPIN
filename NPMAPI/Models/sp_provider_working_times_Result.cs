//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace NPMAPI.Models
{
    using System;
    
    public partial class sp_provider_working_times_Result
    {
        public Nullable<long> PRACTICE_CODE { get; set; }
        public long PROVIDER_CODE { get; set; }
        public Nullable<long> LOCATION_CODE { get; set; }
        public string weekday_id { get; set; }
        public string dayNam { get; set; }
        public Nullable<System.TimeSpan> Time_From { get; set; }
        public Nullable<System.TimeSpan> Time_To { get; set; }
        public string AMPMTIMEFROM { get; set; }
        public string AMPMTIMEto { get; set; }
        public Nullable<System.TimeSpan> Break_time_From { get; set; }
        public Nullable<System.TimeSpan> Break_Time_To { get; set; }
        public Nullable<bool> Enable_Break { get; set; }
        public Nullable<bool> Day_on { get; set; }
        public Nullable<int> time_slot_size { get; set; }
        public long Provider_Working_Days_Time_Id { get; set; }
    }
}
