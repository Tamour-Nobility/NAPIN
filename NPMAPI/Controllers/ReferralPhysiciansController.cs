using NPMAPI.Models;
using NPMAPI.Repositories;
using System.Threading.Tasks;
using System.Web.Http;

namespace NPMAPI.Controllers
{ 
    public class ReferralPhysiciansController : BaseController
    {
        private readonly IReferralPhysicianRepository _repository;

        public ReferralPhysiciansController(IReferralPhysicianRepository repository)
        {
            _repository = repository;
        }

        [HttpPost]
        public async Task<ResponseModel> CreateReferralPhysician(ReferralPhysicianViewModel model)
        {
            return await _repository.CreateReferralPhysician(model, GetUserId());
        }

        [HttpPost]
        public async Task<ResponseModel> UpdateReferralPhysician(ReferralPhysicianViewModel model)
        {
            return await _repository.UpdateReferralPhysician(model, GetUserId());
        }

        [HttpGet]
        public async Task<ResponseModel> GetReferralPhysicianByRefCode(long refCode)
        {
            return await _repository.GetReferralPhysician(refCode);
        }
        [HttpGet]
        public async Task<ResponseModel> GetReferralPhysicians(int page, int count, string pattern)
        {
            return await _repository.GetReferralPhysicians(page, count, pattern);
        }
        [HttpPost]
        public async Task<ResponseModel> ChangeDeleteStatus(long refCode)
        {
            return await _repository.ChangeDeleteStatus(refCode);
        }
        [HttpPost]
        public async Task<ResponseModel> ChangeActiveStatus(long refCode)
        {
            return await _repository.ChangeActiveStatus(refCode);
        }
        [HttpGet]
        public async Task<ResponseModel> GetActiveReferralPhysicians()
        {
            return await _repository.GetActiveReferralPhysicians();
        }

        [HttpGet]
        public async Task<ResponseModel> GetActiveInactiveReferralPhysicians()
        {
            return await _repository.GetActiveInactiveReferralPhysicians();
        }
    }
}