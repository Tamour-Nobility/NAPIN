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
    using System.Collections.Generic;
    
    public partial class Provider_Working_Days_Time
    {
        public long Provider_Code { get; set; }
        public Nullable<long> Practice_Code { get; set; }
        public Nullable<short> WorkDay_Option_Id { get; set; }
        public Nullable<long> Location_Code { get; set; }
        public string Weekday_Id { get; set; }
        public Nullable<System.DateTime> Time_From { get; set; }
        public Nullable<System.DateTime> Time_To { get; set; }
        public Nullable<System.DateTime> Break_Time_From { get; set; }
        public Nullable<System.DateTime> Break_Time_To { get; set; }
        public Nullable<bool> Enable_Break { get; set; }
        public Nullable<System.DateTime> Date_From { get; set; }
        public Nullable<long> Created_By { get; set; }
        public Nullable<System.DateTimeOffset> Created_Date { get; set; }
        public Nullable<long> Modified_By { get; set; }
        public Nullable<System.DateTimeOffset> Modified_Date { get; set; }
        public Nullable<bool> Day_On { get; set; }
        public long Provider_Working_Days_Time_Id { get; set; }
        public Nullable<System.DateTime> Date_To { get; set; }
        public string WeekofMonth { get; set; }
        public Nullable<bool> is_advanced_time { get; set; }
        public Nullable<int> Time_slot_size { get; set; }
        public Nullable<long> Template_Id { get; set; }
    }
}
