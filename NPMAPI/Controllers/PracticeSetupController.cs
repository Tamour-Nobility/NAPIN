using System.Threading.Tasks;
using System.Web.Http;
using NPMAPI.Models;
using NPMAPI.Models.ViewModels;
using NPMAPI.Repositories;
using NPMAPI.Services;

namespace NPMAPI.Controllers
{
    public class PracticeSetupController : BaseController
    {
        private readonly IPracticeRepository _practiceService;
        public PracticeSetupController(IPracticeRepository practiceService)
        {
            _practiceService = practiceService;
        }
        #region Practice
        public ResponseModel GetPractices()
        {
            return _practiceService.GetPractices();
        }
        [HttpGet]
        public ResponseModel GetPractice(long? practiceId)
        {
            return _practiceService.GetPractice(practiceId);
        }
        [HttpGet]
        public ResponseModel ActivateInActivePractice(long practiceId, bool isActive)
        {
            if (practiceId != 0)
                return _practiceService.ActivateInActivePractice(practiceId, isActive);
            else
                return null;
        }
        [HttpPost]
        public ResponseModel PostSavePractice(Practice model)
        {
            ResponseModel objResponse = new ResponseModel();
            if (!ModelState.IsValid)
            {
                objResponse.Status = "Error in Model";
                return objResponse;
            }
            return _practiceService.PostSavePractice(model, GetUserId());
        }
        [HttpGet]
        public ResponseModel GetPracticeSelectList(string searchText)
        {
            return _practiceService.GetPracticeSelectList(searchText);
        }
        #endregion Practice
        #region PracticeNotes
        public ResponseModel GetPracticeNotesList(long PracticeId)
        {
            return _practiceService.GetPracticeNotesList(PracticeId);
        }
        public ResponseModel GetPracticeNote(long PracticeNotesId)
        {
            return _practiceService.GetPracticeNote(PracticeNotesId);
        }
        [HttpPost]
        public ResponseModel SavePracticeNote([FromBody] Practice_Notes PracticeNote)
        {
            ResponseModel objResponse = new ResponseModel();
            if (!ModelState.IsValid)
            {
                objResponse.Status = "Error in Model";
                return objResponse;
            }
            return _practiceService.SavePracticeNote(PracticeNote);
        }
        [HttpGet]
        public ResponseModel DeletePracticetNote(long PracticeId, long PracticeNotesId)
        {
            return _practiceService.DeletePracticetNote(PracticeId, PracticeNotesId);
        }
        #endregion PracticeNotes
        #region PracticeSpecialInstruction
        public ResponseModel GetSpecialInstructionList(long PracticeId)
        {
            return _practiceService.GetSpecialInstructionList(PracticeId);
        }
        public ResponseModel GetSpecialInstruction(long QuestionId, long PracticeId)
        {
            return _practiceService.GetSpecialInstruction(QuestionId, PracticeId);
        }
        public ResponseModel GetSpecialInstructionQuestionByCategory(long CategoryId)
        {
            return _practiceService.GetSpecialInstructionQuestionByCategory(CategoryId);
        }
        [HttpPost]
        public ResponseModel SaveSpecialInstruction([FromBody] Practice_Special_Instruction_Answers model)
        {
            ResponseModel objResponse = new ResponseModel();
            if (!ModelState.IsValid)
            {
                objResponse.Status = "Error in Model";
                return objResponse;
            }
            return _practiceService.SaveSpecialInstruction(model, GetUserId());
        }
        [HttpGet]
        public ResponseModel DeleteSpecialInstruction(long QuestionId, long PracticeId)
        {
            return _practiceService.DeleteSpecialInstruction(QuestionId, PracticeId);
        }
        #endregion PracticeSpecialInstruction
        #region Provider
        public ResponseModel GetProviders(long PracticeId)
        {
            return _practiceService.GetProviders(PracticeId);
        }
        public ResponseModel GetProvider(long providerId, long PracticeId)
        {
            return _practiceService.GetProvider(providerId, PracticeId);
        }
        [HttpPost]
        public ResponseModel SaveProvider(Provider model)
        {
            ResponseModel objResponse = new ResponseModel();
            if (!ModelState.IsValid)
            {
                objResponse.Status = "Error in Model";
                return objResponse;
            }
            return _practiceService.SaveProvider(model, GetUserId());
        }
        [HttpGet]
        public ResponseModel ActivateInActiveProvider(long providerId, long PracticeId, bool isActive)
        {
            return _practiceService.ActivateInActiveProvider(providerId, PracticeId, isActive);
        }
        #endregion Provider
        #region ProviderNotes
        public ResponseModel GetProviderNotesList(long ProviderId)
        {
            return _practiceService.GetProviderNotesList(ProviderId);
        }
        public ResponseModel GetProviderNote(long ProviderNotesId)
        {
            return _practiceService.GetProviderNote(ProviderNotesId);
        }
        [HttpPost]
        public ResponseModel SaveProviderNote([FromBody] Provider_Notes ProviderNote)
        {
            ResponseModel objResponse = new ResponseModel();
            if (!ModelState.IsValid)
            {
                objResponse.Status = "Error in Model";
                return objResponse;
            }
            return _practiceService.SaveProviderNote(ProviderNote);
        }
        [HttpGet]
        public ResponseModel DeleteProviderNote(long ProviderId, long ProviderNotesId)
        {
            return _practiceService.DeleteProviderNote(ProviderId, ProviderNotesId);
        }
        #endregion ProviderNotes
        #region ProviderWorking Hours
        public ResponseModel GetProviderWorkingHours(long ProviderId, long LocationId)
        {
            return _practiceService.GetProviderWorkingHours(ProviderId, LocationId);
        }
        #endregion ProviderWorkingHours
        #region PracticeResources
        public ResponseModel GetPracticeResources(long practiceId, long LocationId)
        {
            return _practiceService.GetPracticeResources(practiceId, LocationId);
        }
        public ResponseModel GetProviderResources(long ProviderId)
        {
            return _practiceService.GetProviderResources(ProviderId);
        }
        #endregion PracticeResources
        #region ProviderPayers
        public ResponseModel GetProviderPayers(long ProviderId, long LocationId)
        {
            return _practiceService.GetProviderPayers(ProviderId, LocationId);
        }
        #endregion ProviderPayers

        #region PracticePayers
        [HttpGet]
        public ResponseModel GetPracticePayers(long PracticeId)
        {
            return _practiceService.GetPracticePayers(PracticeId);
        }
        #endregion PracticePayers
        [HttpGet]
        public ResponseModel DeletePracticePayers(long PracticeId, string ProviderPayerId)
        {
            return _practiceService.DeletePracticePayers(PracticeId, ProviderPayerId);
        }
        //payer mapping - api (Pir Ubaid Shah)
        [HttpPost]
        public ResponseModel MapPayer([FromBody] InsuranceModel model)
        {
            return _practiceService.MapPayer(model, GetUserId());
        }


        //
        #region ProviderLocations
        public ResponseModel GetPracticeLocationList(long PracticeId)
        {
            return _practiceService.GetPracticeLocationList(PracticeId);
        }
        public ResponseModel GetPracticeLocation(long PracticeId, long PracticeLocationId)
        {
            return _practiceService.GetPracticeLocation(PracticeId, PracticeLocationId);
        }
        [HttpPost]
        public ResponseModel SavePracticeLocation([FromBody] Practice_Locations PracticeLocation)
        {
            ResponseModel objResponse = new ResponseModel();
            if (!ModelState.IsValid)
            {
                objResponse.Status = "Error in Model";
                return objResponse;
            }
            return _practiceService.SavePracticeLocation(PracticeLocation);
        }
        [HttpGet]
        public ResponseModel DeletePracticeLocation(long PracticeId, long PracticeLocationId)
        {
            return _practiceService.DeletePracticeLocation(PracticeId, PracticeLocationId);
        }
        #endregion ProviderLocations
        #region PracticeFacility
        public ResponseModel GetPracticeFacilityList(long PracticeId)
        {
            return _practiceService.GetPracticeFacilityList(PracticeId);
        }
        public ResponseModel GetPracticeFacility(long PracticeId, long PracticeFacilityId)
        {
            return _practiceService.GetPracticeFacility(PracticeId, PracticeFacilityId);
        }
        [HttpPost]
        public ResponseModel SavePracticeFacility(PRACTICE_FACILITY model)
        {
            ResponseModel objResponse = new ResponseModel();
            if (!ModelState.IsValid)
            {
                objResponse.Status = "Error in Model";
                return objResponse;
            }
            return _practiceService.SavePracticeFacility(model);
        }
        [HttpGet]
        public ResponseModel DeletePracticeFacility(long PracticeId, long PracticeFacilityId)
        {
            return _practiceService.DeletePracticeFacility(PracticeId, PracticeFacilityId);
        }
        #endregion PracticeFacility
        
        public ResponseModel GetPracticeSpecialityCategoryOne(long GroupNo)
        {
            return _practiceService.GetPracticeSpecialityCategoryOne(GroupNo);
        }
        public ResponseModel GetPracticeSpecialityCategoryTwo(long GroupNo, int CatLevel)
        {
            return _practiceService.GetPracticeSpecialityCategoryTwo(GroupNo, CatLevel);
        }
        [HttpPost]
        public ResponseModel AddPracticeSynchronization([FromBody] PracticeSynchronizationModel model)
        {
            return _practiceService.AddPracticeSynchronization(model, GetUserId());
        }
        public ResponseModel GetPracticeSynchronization(long practiceid)
        {
            return _practiceService.GetPracticeSynchronization(practiceid);
        }

        [HttpGet]
        public Task<ResponseModel> GetSpecialityGroups()
        {
            return _practiceService.GetSpecialityGroups();
        }

        [HttpGet]
        public Task<ResponseModel> GetSpecializations()
        {
            return _practiceService.GetSpecializations();
        }

        [HttpGet]
        public Task<ResponseModel> GetWcbRatings()
        {
            return _practiceService.GetWcbRatings();
        }

        [HttpGet]
        public ResponseModel GetActiveUsersForPractice(long practiceCode)
        {
            return _practiceService.GetActiveUsersForPractice(practiceCode);
        }
        [HttpGet]
        public ResponseModel GetDivisionForUser()
        {
            return _practiceService.GetDivisionForUser();
        }
        //added by Sami ullah
        [HttpGet]
        public ResponseModel IsAutoFillEnableForPractice(long practiceCode)
        {
            return _practiceService.IsAutoFillEnableForPractice(practiceCode);
        }
        [HttpGet]
        public ResponseModel GetRecentClaimDetails(long patientAccount, long practiceCode,string claimType)
        {
            var account = patientAccount.ToString();
            var code = practiceCode.ToString();
            return _practiceService.GetRecentClaimDetails(account, code, claimType);
        }

        [HttpPost]
        public ResponseModel SaveReportingDetails(Practice_Reporting model)
        {
            return _practiceService.SaveReportingDetails(model, GetUserId());
        }

        [HttpGet]
        public ResponseModel GetReportingDetailOfPracticeUser(long userID, long practiceCode)
        {
            return _practiceService.GetReportingDetailOfPracticeUser(userID, practiceCode);
        }
        //added by pir ubaid -for get taxonomy code from pracice against practice code - payer mapping
        [HttpGet]
        public ResponseModel TaxonomyCode(long PracticeId)
        {
            return _practiceService.TaxonomyCode(PracticeId);
        }

        [HttpGet]
        public ResponseModel NPICode(long PracticeId)
        {
            return _practiceService.NPICode(PracticeId);
        }
        //
    }
}
