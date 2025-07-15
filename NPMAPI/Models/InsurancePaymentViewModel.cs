namespace NPMAPI.Models
{
    using System;

    public class InsurancePaymentViewModel
    {
        public long BatchNo { get; set; }
        public Nullable<long> InsuranceID { get; set; }
        public Nullable<int> PaymentTypeID { get; set; }
        public string CheckNo { get; set; }
        public Nullable<System.DateTimeOffset> CheckDate { get; set; }
        public Nullable<decimal> Amount { get; set; }
        public Nullable<long> FacilityID { get; set; }
        public Nullable<System.DateTimeOffset> EOBDate { get; set; }
        public Nullable<System.DateTimeOffset> DepositDate { get; set; }
        public Nullable<System.DateTimeOffset> ReceivedDate { get; set; }
        public string NOtes { get; set; }
        public string prac_code { get; set; }

    }
}