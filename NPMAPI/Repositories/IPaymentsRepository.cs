using System.Collections.Generic;
using NPMAPI.Models;
using NPMAPI.Models.ViewModels;

namespace NPMAPI.Repositories
{
    public interface IPaymentsRepository
    {
        ResponseModel AddInsurancePayment(InsurancePaymentViewModel request);

        ResponseModel AddPatientPayment(PatientPayment request);

        List<SelectListViewModel> GetPaymentList();

        ResponseModel SearchPayment(PaymentsSearchRequestModel SearchModel);

        ResponseModel GetClaimsSummary(string ClaimId, string practiceCode);

        ResponseModel GetClaimBypatientdetials(patientBasedClaimModel request);

        ResponseModel GetClaimByinsdetials(insBasedClaimModel request);
        ResponseModel PostClaims(postClaim request);
        ResponseModel getClaimsDetails(long claimNo, long patientaccount);
        ResponseModel SaveClaimsDetails(ClaimsPaymentDetailModel[] cpd);
        ResponseModel checkPostedClaims(long batchno, long practiceCode);



    }
}
