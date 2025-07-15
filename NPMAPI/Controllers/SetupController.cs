using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using NPMAPI.Models;
using NPMAPI.Models.ViewModels;
using NPMAPI.Repositories;

namespace NPMAPI.Controllers
{
    public class SetupController : BaseController
    {
        private readonly ISetupRepository _setupService;

        public SetupController(ISetupRepository setupService)
        {
            _setupService = setupService;
        }

        #region Facility

        public ResponseModel GetFacilityList()
        {
            return _setupService.GetFacilityList();
        }
        public ResponseModel GetFacility(long FacilityId)
        {
            return _setupService.GetFacility(FacilityId);
        }

        [HttpPost]
        public ResponseModel SaveFacility([FromBody] Facility model)
        {
            ResponseModel objResponse = new ResponseModel();
            if (!ModelState.IsValid)
            {
                objResponse.Status = "Error in Model";
                return objResponse;
            }

            return _setupService.SaveFacility(model, GetUserId());
        }

        [HttpGet]
        public ResponseModel DeleteFacility(long FacilityId)
        {
            return _setupService.DeleteFacility(FacilityId);
        }

        #endregion Facility

        #region Gurantor

        public ResponseModel GetGurantorsList()
        {
            return _setupService.GetGurantorsList();
        }
        [HttpPost]
        public ResponseModel GetGurantorsList(Guarantor model)
        {
            return _setupService.GetGurantorsList(model);
        }
        [HttpPost]
        public ResponseModel GetNDCList(NDCViewModel model)
        {
            return _setupService.GetNDCList(model);
        }

        [HttpPost]
        public ResponseModel GetDXList(DXViewModel model)
        {
            return _setupService.GetDXList(model);
        }
        public ResponseModel GetGurantor(long GurantorId)
        {
            return _setupService.GetGurantor(GurantorId);
        }

        [HttpPost]
        public ResponseModel SaveGurantor([FromBody] Guarantor model)
        {
            ResponseModel objResponse = new ResponseModel();
            if (!ModelState.IsValid)
            {
                objResponse.Status = "Error in Model";
                return objResponse;
            }

            return _setupService.SaveGurantor(model, GetUserId());
        }

        [HttpPost]
        public ResponseModel SaveDX([FromBody] Diagnosi model)
        {
            ResponseModel objResponse = new ResponseModel();
            if (!ModelState.IsValid)
            {
                objResponse.Status = "Error in Model";
                return objResponse;
            }

            return _setupService.SaveDX(model, GetUserId());
        }
        [HttpPost]
        public ResponseModel UpdateDX([FromBody] Diagnosi model)
        {
            ResponseModel objResponse = new ResponseModel();
            if (!ModelState.IsValid)
            {
                objResponse.Status = "Error in Model";
                return objResponse;
            }

            return _setupService.UpdateDX(model, GetUserId());
        }

        [HttpPost]
        public ResponseModel SaveNDC([FromBody] NDCModel model)
        {
            ResponseModel objResponse = new ResponseModel();
            if (!ModelState.IsValid)
            {
                objResponse.Status = "Error in Model";
                return objResponse;
            }

            return _setupService.SaveNDC(model, GetUserId());
        }
         [HttpGet]
       public ResponseModel DeleteNDC(long NDC_ID)
        {
            return _setupService.DeleteNDC(NDC_ID);
        }

        [HttpGet]
        public ResponseModel DeleteDX(string DX_Code)
        {
            return _setupService.DeleteDX(DX_Code);
        }

        [HttpGet]
        public ResponseModel DeleteGurantor(long GurantorId)
        {
            return _setupService.DeleteGurantor(GurantorId);
        }

        #endregion Gurantor

        #region FeeSchedule

        public ResponseModel GetStatesList()
        {
            return _setupService.GetStatesList();
        }

        public ResponseModel GetPracticeList()
        {
            return _setupService.GetPracticeList(GetUserId());
        }
        public ResponseModel GetEshaPracticeList()
        {
            return _setupService.GetEshaPracticeList(GetUserId());
        }
        
        public ResponseModel GetuserList(long PracCode)
        {
            return _setupService.GetuserList(PracCode);
        }


        public ResponseModel GetProviderFeeScheduleDD()
        {
            return _setupService.GetProviderFeeScheduleDD();
        }

        public ResponseModel GetStandardCPTFeeSchedule(string StateCode)
        {
            return _setupService.GetStandardCPTFeeSchedule(StateCode);
        }

        [HttpPost]
        public ResponseModel GetProviderPlanDetails(string ProviderCPTPlanId, Pager pager)
        {
            return _setupService.GetProviderPlanDetails(ProviderCPTPlanId, pager);
        }

        public async Task<ResponseModel>   GetRevenueCodes(string Code)
        {
            return  await _setupService.GetRevenueCode( Code);
        }


        [HttpGet]

        [HttpPost]
        public ResponseModel GetProviderFeeSchedule(ProviderFeeScheduleSearchVM model)
        {
            ResponseModel responseModel = new ResponseModel();
            if (!ModelState.IsValid)
            {
                responseModel.Status = string.Join(";", ModelState.Values.SelectMany(m => m.Errors).Select(m => m.ErrorMessage));
                return responseModel;
            }
            return _setupService.GetProviderFeeSchedule(model);
        }

        //Post Provider CPT Plan Created BY Backend_Team 10/Jan/2023
        [HttpPost]
        public ResponseModel PostproviderFeeSchedule(providercptplanModel model)
        {
            ResponseModel responseModel = new ResponseModel();
            if (!ModelState.IsValid)
            {
                responseModel.Status = string.Join(";", ModelState.Values.SelectMany(m => m.Errors).Select(m => m.ErrorMessage));
                return responseModel;
            }
            return _setupService.PostproviderFeeSchedule(model, GetUserId());
        }



        //Created BY Backend_Team 10/Jan/2023

        [HttpPost]
        public ResponseModel checkproviderFeeinformation(check_provider_cptplan_existence model)
        {
            ResponseModel responseModel = new ResponseModel();
            if (!ModelState.IsValid)
            {
                responseModel.Status = string.Join(";", ModelState.Values.SelectMany(m => m.Errors).Select(m => m.ErrorMessage));
                return responseModel;
            }
            return _setupService.checkproviderFeeinformation(model);
        }



        //Created BY Backend_Team 10/Jan/2023
        [HttpGet]
        public ResponseModel getpracticeinformationforcptplan(long Practicecode)
        {
            ResponseModel responseModel = new ResponseModel();
            if (!ModelState.IsValid)
            {
                responseModel.Status = string.Join(";", ModelState.Values.SelectMany(m => m.Errors).Select(m => m.ErrorMessage));
                return responseModel;
            }
            return _setupService.getpracticeinformationforcptplan(Practicecode);
        }



        [HttpGet]
        public ResponseModel GetProviderPlanDetails(string ProviderCPTPlanId)
        {
            return _setupService.GetProviderPlanDetails(ProviderCPTPlanId);
        }

        [HttpGet]
        public ResponseModel GetProviderFeeSchedule(long PracticeCode)
        {
            return _setupService.GetProviderFeeSchedule(PracticeCode);
        }

        [HttpGet]
        public ResponseModel GetDescriptionByCPT(string ProviderCPTPCode)
        {
            return _setupService.GetDescriptionByCPT(ProviderCPTPCode);
        }
        //.. Added by Pir Ubaid - CPT With Multiple Description
        //for check the code wethter it is CPT code or Alternate Code 
        [HttpGet]
        public ResponseModel CheckCode(string Code)
        {
            return _setupService.CheckCode(Code);
        }
        //

        [HttpGet]
        public ResponseModel GetByAlternateCode(string AlternateCode)
        {
            return _setupService.GetByAlternateCode(AlternateCode);
        }
        //..

        [HttpPost]
        public ResponseModel PostProviderCptPlanDetails(Provider_Cpt_Plan_Details model)
        {
            ResponseModel res = new ResponseModel();
            if (!ModelState.IsValid)
            {
                res.Status = string.Join(";", ModelState.Values.SelectMany(m => m.Errors).Select(m => m.ErrorMessage));
                return res;
            }
            return _setupService.PostProviderCptPlanDetails(model, GetUserId());
        }
        //.. Added by Pir Ubaid - CPT With Multiple Description
        [HttpGet]
        public ResponseModel CheckDuplicateAlternateCode(string AlternateCode, string ProviderCPTPlainId)
        {
            return _setupService.CheckDuplicateAlternateCode(AlternateCode, ProviderCPTPlainId);
        }

        //check duplicate alterenate code in claim charges 
       
        [HttpGet]
        public ResponseModel DuplicateAlternateCodeClaimCharges(string AlternateCode)
        {
            return _setupService.DuplicateAlternateCodeClaimCharges(AlternateCode);
        }
        //..
        [HttpGet]
        public ResponseModel CheckDuplicateCPT(string ProviderCPTCode, string ProviderCPTPlainId)
        {
            return _setupService.CheckDuplicateCPT(ProviderCPTCode, ProviderCPTPlainId);
        }

        [HttpPost]
        public ResponseModel InitProviderFeePlan(ProviderFeeScheduleSearchVM model)
        {
            ResponseModel responseModel = new ResponseModel();
            if (!ModelState.IsValid)
            {
                responseModel.Status = string.Join(";", ModelState.Values.SelectMany(m => m.Errors).Select(m => m.ErrorMessage));
                return responseModel;
            }
            return _setupService.InitProviderFeePlan(model);
        }

        [HttpPost]
        public ResponseModel PaginateStandardCPTFee(long practiceCode, Pager pager)
        {
            return _setupService.GetStandardCPTFeeSchedule(practiceCode, pager);
        }

        [HttpPost]
        public ResponseModel CreateProviderCPTPlan(CreateProviderCPTPlanVM model)
        {
            ResponseModel responseModel = new ResponseModel();
            if (!ModelState.IsValid)
            {
                responseModel.Status = string.Format(";", ModelState.Values.SelectMany(m => m.Errors).Select(m => m.ErrorMessage));
                return responseModel;
            }
            return _setupService.CreateProviderCPTPlan(model, GetUserId());
        }

        [HttpPost]
        public ResponseModel UpdateProviderCPTDetails(string Id, List<Provider_Cpt_Plan_Details> model)
        {
            return _setupService.UpdateProviderCPTDetails(Id, model, GetUserId());
        }

        [HttpGet]
        public ResponseModel DeleteProviderPlanAndCPT(string planId)
        {
            return _setupService.DeleteProviderPlanAndCPT(planId, GetUserId());
        }

        [HttpGet]
        public ResponseModel GetStandardNobilityCPTFeeSchedule(string StateCode)
        {
            return _setupService.GetStandardNobilityCPTFeeSchedule(StateCode);
        }

        #region Provider CPT Plan Notes

        [HttpGet]
        public ResponseModel GetProviderCPTPlanNotes(string PlanId)
        {
            return _setupService.GetProviderCPTPlanNotes(PlanId);
        }

        [HttpPost]
        public ResponseModel SaveProviderCPTNote(ProviderCptPlanNoteCreateVM note)
        {
            ResponseModel responseModel = new ResponseModel();
            if (!ModelState.IsValid)
            {
                responseModel.Status = string.Join(";", ModelState.Values.SelectMany(m => m.Errors).Select(m => m.ErrorMessage));
                return responseModel;
            }
            return _setupService.SaveProviderCPTNote(note, GetUserId());
        }

        [HttpGet]
        public ResponseModel DeleteProviderCPTNote(long NoteId)
        {
            return _setupService.DeleteProviderCPTNote(NoteId, GetUserId());
        }
        #endregion

        #endregion FeeSchedule

        #region Procedures
        [HttpGet]
        public ResponseModel GetProcedure(string procedureCode, string alternateCode)
        {
            return _setupService.GetProcedure(procedureCode, alternateCode);
        }
        [HttpGet]
        public ResponseModel GetDropdownsListForProcedures()
        {
            return _setupService.GetDropdownsListForProcedures();
        }

        [HttpGet]
        public ResponseModel DuplicateProcAndAlternateCode(string AlternateCode, string ProcCode)
        {
            return _setupService.DuplicateProcAndAlternateCode(AlternateCode,ProcCode);

        }


        [HttpPost]
        public ResponseModel SaveProcedure([FromBody] ProcedureViewModel model)
        {
            ResponseModel objResponse = new ResponseModel();
            if (!ModelState.IsValid)
            {
                objResponse.Status = "Error in Model";
                return objResponse;
            }


            return _setupService.SaveProcedure(model,GetUserId());
        }
        [HttpGet]
        public ResponseModel DeleteProcedure(string procedureCode, string alternateCode)
        {
            return _setupService.DeleteProcedure(procedureCode, alternateCode, GetUserId());
        }
        [HttpPost]
        public ResponseModel SearchProcedures(ProceduresSearchViewModel model)
        {
            return _setupService.SearchProcedures(model);
        }
        #endregion Procedures


       
    }
}