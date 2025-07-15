using NPMAPI.Models;
using System.Threading.Tasks;

namespace NPMAPI.Repositories
{
    public interface IReferralPhysicianRepository
    {
        Task<ResponseModel> CreateReferralPhysician (ReferralPhysicianViewModel model, long userId);
        Task<ResponseModel> UpdateReferralPhysician (ReferralPhysicianViewModel model, long userId);
        Task<ResponseModel> GetReferralPhysician(long refCode);
        Task<ResponseModel> ChangeDeleteStatus(long refCode);
        Task<ResponseModel> ChangeActiveStatus (long refCode);
        Task<ResponseModel> GetReferralPhysicians(int page, int count, string pattern);
        Task<ResponseModel> GetActiveReferralPhysicians();
        Task<ResponseModel> GetActiveInactiveReferralPhysicians();
    }
}
