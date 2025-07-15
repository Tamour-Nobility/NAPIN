using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using NPMAPI.Models;
using NPMAPI.Models.ViewModels;

namespace NPMAPI.Repositories
{
    public interface ISetupRepository
    {
        ResponseModel GetFacilityList();
        ResponseModel GetFacility(long FacilityId);
        ResponseModel SaveFacility([FromBody] Facility Model, long userId);
        ResponseModel DeleteFacility(long FacilityId);

        #region Gurantor
        ResponseModel GetGurantorsList();
        ResponseModel GetGurantor(long GurantorId);
        ResponseModel SaveGurantor([FromBody] Guarantor Model, long v);
        ResponseModel DeleteGurantor(long GurantorId);
        #endregion Gurantor

        #region FeeSchedule

        ResponseModel GetPracticeList(long userId);
        ResponseModel GetEshaPracticeList(long userId);

        ResponseModel GetuserList(long PracCode);

        ResponseModel GetStatesList();

        ResponseModel GetProviderFeeSchedule(ProviderFeeScheduleSearchVM model);


        Task<ResponseModel> GetRevenueCode(string Code);
        ResponseModel GetProviderPlanDetails(string ProviderCPTPlanId, Pager pager);
        ResponseModel GetStandardCPTFeeSchedule(string StateCode);
        ResponseModel GetStandardCPTFeeSchedule(long practiceCode, Pager pager);
        ResponseModel GetStandardNobilityCPTFeeSchedule(string StateCode);
        ResponseModel GetGurantorsList(Guarantor model);
        ResponseModel GetProviderFeeScheduleDD();
        ResponseModel InitProviderFeePlan(ProviderFeeScheduleSearchVM model);
        ResponseModel DeleteProviderPlanAndCPT(string planId, long v);
        ResponseModel CreateProviderCPTPlan(CreateProviderCPTPlanVM model, long userId);
        ResponseModel UpdateProviderCPTDetails(string id, List<Provider_Cpt_Plan_Details> model, long v);
        ResponseModel PostproviderFeeSchedule(providercptplanModel model, long userId);
        ResponseModel checkproviderFeeinformation(check_provider_cptplan_existence model);
        ResponseModel getpracticeinformationforcptplan(long Practicecode);

        #region  Provider CPT Plan Notes
        ResponseModel GetProviderCPTPlanNotes(string planId);

        ResponseModel SaveProviderCPTNote(ProviderCptPlanNoteCreateVM note, long v);

        ResponseModel DeleteProviderCPTNote(long noteId, long v);

        #endregion

        #endregion FeeSchedule

        #region Procedure
        ResponseModel DuplicateProcAndAlternateCode(string AlternateCode, string ProcCode);
        ResponseModel GetProcedure(string procedureCode, string alternateCode);
        ResponseModel SaveProcedure(ProcedureViewModel model,long userId);
        ResponseModel DeleteProcedure(string procedureCode, string alternateCode, long userId);
        ResponseModel SearchProcedures(ProceduresSearchViewModel model);
        ResponseModel GetDropdownsListForProcedures();
        ResponseModel GetProviderFeeSchedule(long practiceCode);
        ResponseModel GetProviderPlanDetails(string providerCPTPlanId);
        ResponseModel GetDescriptionByCPT(string ProviderCPTPCode);
        //.. Added by Pir Ubaid - CPT With Multiple Description
        ResponseModel GetByAlternateCode(string AlternateCode);
        ResponseModel CheckCode(string Code);
        //..
        ResponseModel PostProviderCptPlanDetails(Provider_Cpt_Plan_Details model, long userId);
        ResponseModel CheckDuplicateCPT(string providerCPTCode, string providerCPTPlainId);
        //.. Added by Pir Ubaid - CPT With Multiple Description
        ResponseModel CheckDuplicateAlternateCode(string AlternateCode, string ProviderCPTPlainId);
        ResponseModel DuplicateAlternateCodeClaimCharges(string AlternateCode);
        //..

        #endregion Procedure
        ResponseModel SaveNDC([FromBody] NDCModel model, long v);
        ResponseModel SaveDX([FromBody] Diagnosi model, long v);
        ResponseModel UpdateDX([FromBody] Diagnosi Model, long userId);
        ResponseModel GetNDCList(NDCViewModel model);
        ResponseModel GetDXList(DXViewModel model);
        ResponseModel DeleteNDC(long NDC_ID);
        ResponseModel DeleteDX(string DX_code);

    }
}
