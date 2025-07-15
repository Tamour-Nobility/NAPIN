using System.Collections.Generic;
using System.Web.Http;
using NPMAPI.Models;
using NPMAPI.Models.ViewModels;
using NPMAPI.Repositories;

namespace NPMAPI.Controllers
{
    public class PaymentsController : BaseController
    {
        private readonly IPaymentsRepository _paymentsService;

        public PaymentsController(IPaymentsRepository paymentsService)
        {
            _paymentsService = paymentsService;
        }


        [HttpPost]
        public ResponseModel SearchPayment(PaymentsSearchRequestModel request)
        {

            return _paymentsService.SearchPayment(request);

        }

        [HttpPost]
        public ResponseModel AddInsurancePayment([FromBody] InsurancePaymentViewModel request)
        {
            if (ModelState.IsValid)
                return _paymentsService.AddInsurancePayment(request);
            else
                return new ResponseModel
                {
                    Status = "Error",
                    Response = "Validation"
                };
        }

        [HttpPost]
        public ResponseModel AddPatientPayment([FromBody] PatientPayment request)
        {
            if (ModelState.IsValid)
                return _paymentsService.AddPatientPayment(request);
            else
                return new ResponseModel
                {
                    Status = "Error",
                    Response = "Validation"
                };
        }

        public List<SelectListViewModel> GetPaymentList()
        {
            return _paymentsService.GetPaymentList();
        }

        [HttpGet]
        public ResponseModel GetClaimsSummary(string ClaimId, string practiceCode)
        {

            return _paymentsService.GetClaimsSummary(ClaimId, practiceCode);

        }

        [HttpPost]
        public ResponseModel GetClaimBypatient(patientBasedClaimModel request)
        {

            return _paymentsService.GetClaimBypatientdetials(request);

        }

        [HttpPost]
        public ResponseModel GetClaimByins(insBasedClaimModel request)
        {

            return _paymentsService.GetClaimByinsdetials(request);

        }


        [HttpPost]
        public ResponseModel PostClaims(postClaim request)
        {

            return _paymentsService.PostClaims(request);

        }

        [HttpGet]
        public ResponseModel getClaimsDetails(long claimNo, long patientaccount)
        {

            return _paymentsService.getClaimsDetails(claimNo, patientaccount);

        }

        [HttpPost]
        public ResponseModel SaveClaimsDetails(ClaimsPaymentDetailModel[] cpd)
        {

            return _paymentsService.SaveClaimsDetails(cpd);

        }

        [HttpGet]
        public ResponseModel checkPostedClaims(long batchno , long practiceCode)
        {

            return _paymentsService.checkPostedClaims(batchno ,  practiceCode);

        }






    }
}
