namespace NPMAPI.Models.ViewModels
{
    public class PracticeAppointmentStatusViewModel
    {
        public long PracAppSID { get; set; }
        public long? PracCode { get; set; }
        public long? ProvdID { get; set; }
        public long? LocID { get; set; }
        public long AppStatusID { get; set; }
        public string Appointment_Status_Description { get; set; }
    }
}