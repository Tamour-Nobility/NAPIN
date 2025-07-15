namespace NPMAPI.Models
{
    public class PatientPayment
    {
        public long BatchNo { get; set; }
        public int PaymentTypeID { get; set; }
        public string CheckNo { get; set; }
        public System.DateTimeOffset CheckDate { get; set; }
        public decimal Amount { get; set; }
        public long FacilityID { get; set; }
        public System.DateTimeOffset DepositDate { get; set; }
        public long PatientAccount { get; set; }
        public string PatientName { get; set; }
        public string prac_code { get; set; }

    }
}