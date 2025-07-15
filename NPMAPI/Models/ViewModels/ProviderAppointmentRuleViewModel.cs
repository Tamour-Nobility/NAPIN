namespace NPMAPI.Models.ViewModels
{
    public class ProviderAppointmentRuleViewModel
    {
        public long Rule_Id { get; set; }
        public long Practice_Code { get; set; }
        public long Provider_code { get; set; }
        public long Location_code { get; set; }
        public System.DateTime No_Appointments_Start_Time { get; set; }
        public System.DateTime No_Appointment_End_Time { get; set; }
    }
}