using System;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Web.Http;
using NPMAPI.App_Start;
using NPMAPI.Models;
using NPMAPI.Repositories;
using System.Net.Http;
using System.Configuration;
using System.Web;
using System.IO;

namespace NPMAPI.Controllers
{

    public class TokenController : ApiController
    {
        private readonly IUserManagementSetup _userManagementService;
        private readonly IEncryption _EncryptionService;
        public TokenController(IUserManagementSetup userManagementService, IEncryption userEncryptionService)
        {
            _userManagementService = userManagementService;
            _EncryptionService = userEncryptionService;
        }
        [AllowAnonymous]
        public IHttpActionResult Auth([FromBody] TokenRequestModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(string.Join(";", ModelState.Values.SelectMany(e => e.Errors).Select(e => e.ErrorMessage)));
            }
            switch (model.Grant_Type)
            {
                case "password":
                    return CreateAccessToken(model);
                case "refresh_token":
                    return RefreshToken(model);
                default:
                    return BadRequest("Invalid grant type");
            }
        }
        [AllowAnonymous]
        public IHttpActionResult AuthCode([FromBody] CodeRequestModel model)
        
        
        {
            try
            
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(string.Join(";", ModelState.Values.SelectMany(e => e.Errors).Select(e => e.ErrorMessage)));
                }
                var user = _EncryptionService.VerifyUserCode(model.code, model.userid);
                if (user != null)
                {
                    if (user.UserId == 0)
                    {
                        return BadRequest("OTP EXPIRED");
                    }
                    var userRoleAndRights = _userManagementService.GetUserRoleAndRights(user.UserId);
                    RefreshToken refreshToken = CreateRefreshToken(user.UserId);
                    string token = JWTManager.GenerateAccessToken(user.Username.ToLower(), user.Role, user.UserId);
                    return Ok(new TokenResponse() { Access_Token = token, Refresh_Token = refreshToken.Token, Practices = user.Practices, RolesAndRights = userRoleAndRights });
                }
                return Unauthorized();

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        [AllowAnonymous]
        [HttpGet]
        public IHttpActionResult AuthCodeResend(long userid)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(string.Join(";", ModelState.Values.SelectMany(e => e.Errors).Select(e => e.ErrorMessage)));
                }
                var user = _EncryptionService.VerifyUserResend(userid);
                if (user != null)
                {
                   var access_code = myauthmailsender(user.email, user.LastName);

                    if (access_code != null)
                    {
                        maintainlogofOTP(user.UserId, access_code);

                        return Ok(user.UserId);
                    }
                }

                return Unauthorized();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private IHttpActionResult RefreshToken(TokenRequestModel model)
        {
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var refreshTokenFromDb = ctx.RefreshTokens.FirstOrDefault(t => t.Token == model.Refresh_Token);
                    if (refreshTokenFromDb == null)
                    {
                        return Unauthorized();
                    }
                    if (refreshTokenFromDb.ExpiresUtc < DateTime.Now.ToUniversalTime())
                    {
                        return Unauthorized();
                    }
                    RefreshToken rt = CreateRefreshToken(refreshTokenFromDb.UserId);
                    var ur = (from u in ctx.Users
                              join r in ctx.Roles on u.RoleId equals r.RoleId
                              where u.UserId == rt.UserId
                              select new
                              {
                                  Username = u.UserName,
                                  Role = r.RoleName,
                                  u.UserId
                              }).FirstOrDefault();
                    if (ur == null)
                    {
                        return Unauthorized();
                    } 
                    string token = JWTManager.GenerateAccessToken(ur.Username, ur.Role, ur.UserId);
                    return Ok(new TokenResponse() { Refresh_Token = rt.Token, Access_Token = token });
                }            
            }
            catch (Exception)
            {
                throw;
            }
        }
        public string myauthmailsender(string receiverEmail, string name)
        {
            try
            {
                Random rnd = new Random();
                var fromAddress = new MailAddress(ConfigurationManager.AppSettings["username"].ToString(), "NOBILITY RCM");
                var toAddress = new MailAddress(receiverEmail);
                string pass = ConfigurationManager.AppSettings["password"].ToString();
                string fromPassword = pass;
                const string subject = "Two-Factor verification code";
                string uniqueOTP = (rnd.Next(100000, 999999)).ToString();
                string body = "Verify your unique OTP: " + uniqueOTP;
                //  Console.WriteLine(opt);

                //Fetching Email Body Text from EmailTemplate File.  
                string FilePath = HttpContext.Current.Server.MapPath("~/Views/Email Templates/Emailtemplate.html"); ;

                StreamReader str = new StreamReader(FilePath);
                string MailText = str.ReadToEnd();
                str.Close();

                var smtp = new SmtpClient
                {
                    Host = ConfigurationManager.AppSettings["smtp"].ToString(),
                    Port = Convert.ToInt16(ConfigurationManager.AppSettings["portnumber"]),
                    //Port = 25,
                    EnableSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["IsSSL"]),
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                };
                MailText = MailText.Replace("abcdef", uniqueOTP);
                MailText = MailText.Replace("Username", name);

                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    IsBodyHtml = true,
                    Subject = subject,
                    Body = MailText,
                    // Body = body
                })
                {
                    smtp.Send(message);
                }

                return uniqueOTP;

            }
            catch (SmtpFailedRecipientException)
            {
                throw;
            }
        }

        public bool maintainlogofOTP(long userid, string Otp)
        {
            try
            {
                DateTime today = DateTime.Now;
                DateTime exp = today.AddMinutes(5);
                using (var ctx = new NPMDBEntities())
                {
                    TWO_FACTOR_AUTHORAZITION model = new TWO_FACTOR_AUTHORAZITION();
                    model.UserId = userid;
                    model.OTP = Convert.ToInt64(Otp);
                    model.CreatedBy = userid;
                    model.CreatedDate = DateTime.Now;
                    model.EXPIREDDate = exp;
                    model.ISEXPIXED = false;
                    model.ModifiedBy = null;
                    model.ModifiedDate = null;
                    ctx.TWO_FACTOR_AUTHORAZITION.Add(model);
                    ctx.SaveChanges();
                }
                return true;
            }
            catch (Exception)
            {
                throw;

            }
        }

        private IHttpActionResult CreateAccessToken(TokenRequestModel model)
        {
            try
            {
                TokenResponse tokenResponse = new TokenResponse();
                using (var ctx = new NPMDBEntities())
                {
                    var user = _EncryptionService.VerifyUser(model.Username, model.Password);
                    if (user != null)
                    {

                      var access_code = myauthmailsender(user.email, user.FirstName);
                    /// var access_code = "123456";
                        if (access_code != null)
                        {
                            maintainlogofOTP(user.UserId, access_code);
                            var newEmail = user.email.Split('@');
                            if (newEmail != null)
                            {
                                var topemail = newEmail[0].Select(x => new string(x, 1)).ToArray();
                                foreach (var item in topemail.Select((value, i) => (value, i)))
                                {
                                    var value = item.value;
                                    var index = item.i;
                                    if (index < (topemail.Length * 0.2))
                                    {
                                        topemail[index] = value;
                                    }
                                    else
                                    {
                                        topemail[index] = "*";
                                    }
                                    //    index > topemail.Length -1-4 ? value : index < topemail.Length * 0.2 ? value : '*'
                                }
                                var trueTopemail = String.Join("", topemail);
                                var bottomemail = newEmail[1].Select(x => new string(x, 1)).ToArray();

                                foreach (var item in bottomemail.Select((value, i) => (value, i)))
                                {
                                    var value = item.value;
                                    var index = item.i;
                                    if (index < (bottomemail.Length * 0.2))
                                    {
                                        bottomemail[index] = value;
                                    }
                                    else
                                    {
                                        bottomemail[index] = "*";
                                   }
                                }
                                var trueBottomemail = String.Join("", bottomemail);
                                var newemailm = trueTopemail + "@" + trueBottomemail;

                                user.email = newemailm;
                                user.UserId = user.UserId * 23443;


                            }
                            return Ok(new { user.UserId, user.email });
                        }
                    }
                    return Unauthorized();
                }
            }
            catch (Exception  ex)
            {
                throw ex; 
            }
        }

        private RefreshToken CreateRefreshToken(long userId)
        {
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    // Removing any existing refresh tokens for current user
                    //ctx.RefreshTokens.RemoveRange(ctx.RefreshTokens.Where(u => u.UserId == userId));
                    // Generating and saving new refresh token
                    var refreshToken = ctx.RefreshTokens.Add(new RefreshToken()
                    {
                        UserId = userId,
                        ExpiresUtc = DateTime.Now.AddDays(GlobalVariables.RefreshTokenDuration).ToUniversalTime(),
                        IssuedUtc = DateTime.Now.ToUniversalTime(),
                        Token = JWTManager.GenerateRefreshToken()
                    });
                    ctx.SaveChanges();
                    return refreshToken;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
