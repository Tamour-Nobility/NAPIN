using System;
using System.Security.Claims;
using System.Web;
using System.Web.Http;

namespace NPMAPI.Controllers
{
    [Authorize]
    public class BaseController : ApiController
    {
        private static ClaimsIdentity _Identity;
        public static long GetUserId()
        {
            _Identity = HttpContext.Current.User.Identity as ClaimsIdentity;
            long userId = 0;
            if (_Identity != null)
            {
                userId = Convert.ToInt64(_Identity.FindFirst("UserId").Value);
            }
            return userId;
        }
        public static string GetUserName()
        {
            _Identity = HttpContext.Current.User.Identity as ClaimsIdentity;
            string userName = null;
            if (_Identity != null)
            {
                userName = _Identity.FindFirst(ClaimTypes.Name).Value;
            }
            return userName;
        }
        public static string GetUserRole()
        {
            _Identity = HttpContext.Current.User.Identity as ClaimsIdentity;
            string role = null;
            if (_Identity != null)
            {
                role = _Identity.FindFirst(ClaimTypes.Role).Value;
            }
            return role;
        }
    }
}
