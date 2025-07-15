using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using NPMAPI.Enums;
using NPMAPI.Models;
using NPMAPI.Models.ViewModels;

namespace NPMAPI.Repositories
{
   public interface IUBClaim
    {
        Task<ResponseModel> SaveUBClaim(ClaimsViewModel cr, long userId);
        Task<ResponseModel> GetAllCodesData();
        Task<ResponseModel> ManageOccurenceSpan(List<Claims_Occurrence_Code> OSClist, long userid);
        //Task<ResponseModel> SaveDropdownsData(UbClaimDropdowns ubClaimDropdowns, long prac_code, long claim_no);
        Task<ResponseModel> GetallDropdowns();
    }
}
