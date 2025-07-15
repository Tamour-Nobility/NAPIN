using System.Collections.Generic;

namespace NPMAPI.Models
{
    public class TokenRequestModel
    {
        public string Grant_Type { get; set; }
        public string ClientId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Refresh_Token { get; set; }
    }
    public class CodeRequestModel
    {
        public long userid { get; set; }
        public string code { get; set; }
    }
    public class TokenResponse
    {
        public string Access_Token { get; set; }
        public string Refresh_Token { get; set; }
        public string Username { get; set; }
        public List<SP_GetUserAuthorization_Result> RolesAndRights { get; set; }
        public long UserId { get; set; }
        public List<UserPracticeViewModel> Practices { get; set; }
        //Added by HAMZA ZULFIQAR as per USER STORY 119: Reporting Dashboard Implementation For All Practices
        public List<int?> ExternalPractices { get; set; }

    }
}