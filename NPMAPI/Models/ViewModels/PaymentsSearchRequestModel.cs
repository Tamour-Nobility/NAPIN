using System.Collections.Generic;

namespace NPMAPI.Models.ViewModels
{
    public class PaymentsSearchRequestModel
    {
        public string BatchNo { get; set; }

        public string paymentId { get; set; }

        public string InsuranceId { get; set; }

        public string InsuranceName { get; set; }
        public string PaymentType { get; set; }
        public string CheckNo { get; set; }

        public string paymentFrom { get; set; }

        public string PaymentStatus { get; set; }
        public string FacilityId { get; set; }

        public string practice_code { get; set; }

        public string Facility { get; set; }
        public string PatientName { get; set; }

        public string PatientAccount { get; set; }

        public string postedBy { get; set; }


        public string PaymentDateFrom { get; set; }

        public string PaymentDateTo { get; set; }



    }

    public class patientBasedClaimModel
    {
        public string practiceCode { get; set; }

        public string PatientAccount { get; set; }

        public string FacilityCode { get; set; }


        public long Balance { get; set; }



    }

    public class insBasedClaimModel
    {
        public string practiceCode { get; set; }
        public string insId { get; set; }

        public string FacilityCode { get; set; }


        public long Balance { get; set; }

        public string dateFrom { get; set; }

        public string dateTo { get; set; }





    }

    public class ClaimsPaymentDetailModel
    {

        public long claims_no { get; set; }
        public long batch_no { get; set; }

        public long Practicecode { get; set; }
        public double PostedAmount { get; set; }
        public long PatientAcount { get; set; }

        public long userID { get; set; }

        public Claim ClaimModel { get; set; }
        public List<ClaimPaymentViewModel> claimPayments { get; set; }
        public List<ClaimChargesViewModel> claimCharges { get; set; }

        public List<ClaimInsuranceViewModel> claimInusrance { get; set; }


    }





    public class AddClaims
    {
        public long claimId { get; set; }
    }

    public class postClaim
    {

        public long batchNo { get; set; }
        public string claimNo { get; set; }
        public long postAmount { get; set; }
    }



}