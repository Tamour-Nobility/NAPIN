using System.Web.Http;
using NPMAPI.Models;
using NPMAPI.Models.ViewModels;
using NPMAPI.Repositories;


namespace NPMAPI.Controllers
{
    public class InsuranceSetupController : BaseController
    {
        private readonly IInsuranceSetupRepository _insuranceSetupService;

        public InsuranceSetupController(IInsuranceSetupRepository insurancesetupService)
        {
            _insuranceSetupService = insurancesetupService;
        }


        #region InsuranceGroup
        public ResponseModel GetInsuranceGroupList()
        {
            return _insuranceSetupService.GetInsuranceGroupList();
        }
        public ResponseModel GetInsuranceGroup(long InsuranceGroupId)
        {
            return _insuranceSetupService.GetInsuranceGroup(InsuranceGroupId);
        }
        [HttpPost]
        public ResponseModel SaveInsuranceGroup([FromBody] Insurance_Groups model)
        {
            ResponseModel objResponse = new ResponseModel();
            if (!ModelState.IsValid)
            {
                objResponse.Status = "Error in Model";
                return objResponse;
            }

            return _insuranceSetupService.SaveInsuranceGroup(model, GetUserId());
        }
        [HttpGet]
        public ResponseModel DeleteInsuranceGroup(long InsuranceGroupId)
        {
            return _insuranceSetupService.DeleteInsuranceGroup(InsuranceGroupId);
        }
        [HttpGet]
        public ResponseModel GetInsuranceGroupsSelectList()
        {
            return _insuranceSetupService.GetInsuranceGroupsSelectList();
        }
        [HttpGet]
        public ResponseModel GetSmartInsuranceGroupsSelectList(string searchText)
        {
            ResponseModel responseModel = new ResponseModel();
            if (string.IsNullOrWhiteSpace(searchText))
            {
                responseModel.Status = "Please provide any search criteria.";
                return responseModel;
            }
            return _insuranceSetupService.GetSmartInsuranceGroupsSelectList(searchText);
        }
        #endregion InsuranceGroup

        #region InsuranceName

        public ResponseModel GetInsuranceNameList(long InsuranceGroupId)
        {
            return _insuranceSetupService.GetInsuranceNameList(InsuranceGroupId);
        }
        public ResponseModel GetInsuranceName(long InsuranceNameId)
        {
            return _insuranceSetupService.GetInsuranceName(InsuranceNameId);
        }
        [HttpGet]
        public ResponseModel GetInsuranceNameModel(long? InsuranceNameId)
        {
            return _insuranceSetupService.GetInsuranceNameModel(InsuranceNameId);
        }
        [HttpGet]
        public ResponseModel GetInsuranceNameSelectList(long? InsuranceGroupId)
        {
            return _insuranceSetupService.GetInsuranceNameSelectList(InsuranceGroupId);
        }
        [HttpGet]
        public ResponseModel GetSmartInsuranceNameList(long? InsuranceGroupId, string searchText)
        {
            return _insuranceSetupService.GetSmartInsuranceNameList(InsuranceGroupId, searchText);
        }
        [HttpPost]
        public ResponseModel SaveInsuranceName([FromBody] InsuranceNameModelViewModel model)
        {
            ResponseModel objResponse = new ResponseModel();
            if (!ModelState.IsValid)
            {
                objResponse.Status = "Error in Model";
                return objResponse;
            }

            return _insuranceSetupService.SaveInsuranceName(model, GetUserId());
        }
        [HttpGet]
        public ResponseModel DeleteInsuranceName(long InsuranceNameId)
        {
            return _insuranceSetupService.DeleteInsuranceName(InsuranceNameId);
        }

        #endregion InsuranceName

        #region InsurancePayer
        [HttpGet]
        public ResponseModel GetInsurancePayerList(long InsuranceGroupId, long InsuranceNameId)
        {
            return _insuranceSetupService.GetInsurancePayerList(InsuranceGroupId, InsuranceNameId);
        }
        [HttpGet]
        public ResponseModel GetInsurancePayer(long InsurancePayerId)
        {
            return _insuranceSetupService.GetInsurancePayer(InsurancePayerId);
        }
        [HttpGet]
        public ResponseModel GetInsurancePayerModel(long? InsurancePayerId)
        {
            return _insuranceSetupService.GetInsurancePayerModel(InsurancePayerId);
        }
        [HttpPost]
        public ResponseModel SaveInsurancePayer([FromBody] InsurancePayerViewModel model)
        {
            ResponseModel objResponse = new ResponseModel();
            if (!ModelState.IsValid)
            {
                objResponse.Status = "Error in Model";
                return objResponse;
            }

            return _insuranceSetupService.SaveInsurancePayer(model, GetUserId());
        }
        [HttpGet]
        public ResponseModel DeleteInsurancePayer(long InsurancePayerId, long InsuranceNameId)
        {
            return _insuranceSetupService.DeleteInsurancePayer(InsurancePayerId, InsuranceNameId);
        }
        [HttpGet]
        public ResponseModel GetSmartInsurancePayersList(string searchText)
        {
            ResponseModel responseModel = new ResponseModel();
            if (string.IsNullOrWhiteSpace(searchText))
            {
                responseModel.Status = "Please provide any criteria to search.";
                return responseModel;
            }
            return _insuranceSetupService.GetSmartInsurancePayersList(searchText);

        }
        #endregion InsurancePayer

        #region Insurances
        [HttpGet]
        public ResponseModel GetInsPayerList()
        {
            return _insuranceSetupService.GetInsPayerList();
        }
        [HttpGet]
        public ResponseModel GetInsPayerById(string InsurancePayerId)
        {
            return _insuranceSetupService.GetInsPayerById(InsurancePayerId);
        }
        [HttpGet]
        public ResponseModel GetInsPayerByState(string InsurancePayerState)
        {
            return _insuranceSetupService.GetInsPayerByState(InsurancePayerState);
        }
        public ResponseModel GetInsuranceList(long? GroupId, string insuranceName, string insurancePayerName)
        {
            return _insuranceSetupService.GetInsuranceList(GroupId

                
                ,insuranceName,insurancePayerName);
        }
        [HttpGet]
        public ResponseModel GetInsurance(long InsuranceId)
        {
            return _insuranceSetupService.GetInsurance(InsuranceId);
        }
        [HttpGet]
        public ResponseModel GetInsuranceModel(long? InsuranceId)
        {
            return _insuranceSetupService.GetInsuranceModel(InsuranceId);
        }
        [HttpPost]
        public ResponseModel SaveInsurance([FromBody] Insurance model)
        {
            ResponseModel objResponse = new ResponseModel();
            if (!ModelState.IsValid)
            {
                objResponse.Status = "Error in Model";
                return objResponse;
            }

            return _insuranceSetupService.SaveInsurance(model, GetUserId());
        }
        [HttpGet]
        public ResponseModel DeleteInsurance(long InsuranceId, long InsurancePayerId)
        {
            return _insuranceSetupService.DeleteInsurance(InsuranceId, InsurancePayerId);
        }
        [HttpGet]
        public ResponseModel GetRelationsSelectList()
        {
            return _insuranceSetupService.GetRelationsSelectList();
        }
        #endregion Insurances

        #region InsuranceRewamp

        [HttpPost]
        public ResponseModel AddInsurance([FromBody] AddInsuranceRequest addInsuranceRequest)
        {
            return _insuranceSetupService.AddInsurance(addInsuranceRequest, GetUserId());
        }

        #endregion InsuranceRewamp
    }
}