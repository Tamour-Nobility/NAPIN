using NPMAPI.Models;
using NPMAPI.Services;
using System;
using System.Security.Claims;
using System.Web;
using System.Web.Http;

namespace NPMAPI.Controllers
{
    public class ClaimAssignmentController : BaseController
    {
        private readonly ClaimAssignmentService _claimassignmentService;
        public ClaimAssignmentController(ClaimAssignmentService claimassignmentService)
        {
            _claimassignmentService = claimassignmentService;
        }

        [HttpGet]
        public ResponseModel GetUsersList(long practicecode)
        {
            return _claimassignmentService.GetSelectedUserList(practicecode);
        }

        //[HttpPost]
        public ResponseModel PostAllAssignedClaims(claimassigneeModel model)
        {
            return _claimassignmentService.PostAllAssignedClaimsforUser(model, GetUserId());
        }

        [HttpGet]
        public ResponseModel GetAssignedClaimData(long claimnumber)
        {
            return _claimassignmentService.GetAssignedClaimDataforUser(claimnumber);
        }

        [HttpPost]
        public ResponseModel EditAssignedClaims(editassignedclaimModel model)
        {
            return _claimassignmentService.EditAssignedClaimsforUser(model, GetUserId());
        }

        [HttpGet]
        public ResponseModel GetAllAssignedClaims(long practice_code, long assignedByuserid)
        {
            if (assignedByuserid == 0)
            {
                return _claimassignmentService.GetAllAssignedClaimsforspecificuser(practice_code, GetUserId());
            }
            else
            {
                return _claimassignmentService.GetAllAssignedClaimsforspecificuser(practice_code, assignedByuserid);
            }

        }

        [HttpGet]
        public ResponseModel GetAllAssignedClaimsForPractice(long practice_code)
        {
            return _claimassignmentService.GetAllAssignedClaimsForPracticeuser(practice_code);
        }

        [HttpGet]
        public ResponseModel GetSpecificAssignedClaimData(long claimassignee_id)
        {
            return _claimassignmentService.GetSpecificAssignedClaimDataforuser(claimassignee_id);
        }

        [HttpGet]
        public ResponseModel GetSpecificAssignedClaimNotes(long ClaimAssignee_notes_ID)
        {
            return _claimassignmentService.GetSpecificAssignedClaimNotesforuser(ClaimAssignee_notes_ID);
        }

        [HttpGet]
        public ResponseModel GetAllAssignedClaimsNotifications(long practice_code)
        {
            return _claimassignmentService.GetAllAssignedClaimsNotificationsforuser(practice_code, GetUserId());
        }

        [HttpGet]
        public ResponseModel GetAllAssignedAccountsNotifications(long practice_code)
        {
            return _claimassignmentService.GetAllAssignedAccountsNotificationsforuser(practice_code, GetUserId());
        }

        //Account Level ******************************************************************

        [HttpPost]
        public ResponseModel PostAllAssignedAccounts(accountassigneeModel model)
        {
            return _claimassignmentService.PostAllAssignedAccountforUser(model, GetUserId());
        }



        [HttpGet]
        public ResponseModel GetAllAssignedAccounts(long practice_code, long assignedByuserid)
        {
            if (assignedByuserid == 0)
            {
                return _claimassignmentService.GetAllAssignedAccountsforspecificuser(practice_code, GetUserId());
            }
            else
            {
                return _claimassignmentService.GetAllAssignedAccountsforspecificuser(practice_code, assignedByuserid);
            }

        }

        [HttpGet]
        public ResponseModel GetAllAssignedAccountsForPractice(long practice_code)
        {
            return _claimassignmentService.GetAllAssignedAccountsForPracticeuser(practice_code);
        }

        [HttpGet]
        public ResponseModel GetSpecificAssignedAccountNotes(long AccountAssignee_notes_ID)
        {
            return _claimassignmentService.GetSpecificAssignedAccountNotesforuser(AccountAssignee_notes_ID);
        }

        [HttpGet]
        public ResponseModel GetSpecificAssignedAccountData(long accountassignee_id)
        {
            return _claimassignmentService.GetSpecificAssignedAccountDataforuser(accountassignee_id);
        }

        [HttpPost]
        public ResponseModel EditAssignedAccounts(editassignedaccountModel model)
        {
            return _claimassignmentService.EditAssignedAccountforUser(model, GetUserId());
        }




    }
}
