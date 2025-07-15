using System;

namespace NPMAPI.Models.ViewModels
{
    public class InsurancePaymentRequestModel
    {
        public int InsuranceId { get; set; }
        public string PaymentType { get; set; }
        public string CheckNo { get; set; }
        public long BatchNo { get; set; }
        public float Amount { get; set; }
        public int FacilityId { get; set; }
        public DateTime EOBDate { get; set; }
        public DateTime DepositDate { get; set; }
        public DateTime ReceivedDate { get; set; }
    }
}