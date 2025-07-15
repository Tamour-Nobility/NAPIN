using System;
using System.ComponentModel.DataAnnotations;

namespace NPMAPI.Models.ViewModels
{
    public class ClaimPaymentsDetailRequest
    {
        [Required]
        public int PracCode { get; set; }
        public string PaymentType { get; set; }
        public string PaymentFrom { get; set; }
        public string CheckNo { get; set; }
        public string PatientName { get; set; }
        public string InsuranceName { get; set; }
        public Nullable<DateTime> ReceivedDateFrom { get; set; }
        public Nullable<DateTime> ReceivedDateTo { get; set; }
    }
}