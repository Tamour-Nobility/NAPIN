using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Security.Cryptography;
using EdiFabric.Templates.Hipaa5010;
using iTextSharp.text;
using NPMAPI.Models;
using NPMAPI.Repositories;

namespace NPMAPI.Services
{
    public class EncryptionService : IEncryption
    {
        public string HashPassword(string password)
        {
            byte[] salt;
            byte[] buffer2;
            if (password == null)
            {
                throw new ArgumentNullException("password");
            }
            using (Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes(password, 0x10, 0x3e8))
            {
                salt = bytes.Salt;
                buffer2 = bytes.GetBytes(0x20);
            }
            byte[] dst = new byte[0x31];
            Buffer.BlockCopy(salt, 0, dst, 1, 0x10);
            Buffer.BlockCopy(buffer2, 0, dst, 0x11, 0x20);
            return Convert.ToBase64String(dst);
        }
        public bool VerifyHashedPassword(string hashedPassword, string password)
        {
            byte[] buffer4;
            if (hashedPassword == null)
            {
                return false;
            }
            if (password == null)
            {
                throw new ArgumentNullException("password");
            }
            byte[] src = Convert.FromBase64String(hashedPassword);
            if ((src.Length != 0x31) || (src[0] != 0))
            {
                return false;
            }
            byte[] dst = new byte[0x10];
            Buffer.BlockCopy(src, 1, dst, 0, 0x10);
            byte[] buffer3 = new byte[0x20];
            Buffer.BlockCopy(src, 0x11, buffer3, 0, 0x20);
            using (Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes(password, dst, 0x3e8))
            {
                buffer4 = bytes.GetBytes(0x20);
            }
            return ByteArraysEqual(buffer3, buffer4);

        }
        public LoggedInUserbyCodeViewModel VerifyUser(string username, string password)
        {
            if (username == null || password == null)
                return null;
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var user = (from u in ctx.Users
                                where (u.IsActive ?? false) == true &&
                                (u.IsDeleted ?? false) == false &&
                                (u.UserName.Trim().ToLower() == username.Trim().ToLower() || u.Email.Trim().ToLower() == username.Trim().ToLower())
                                select new
                                {
                                    FirstName = u.FirstName,
                                    LastName = u.LastName,
                                    UserId = u.UserId,
                                    email = u.Email,
                                    Username = u.UserName,
                                    HashedPassword = u.Password,

                                }).FirstOrDefault();
                    if (user != null && user.UserId > 0)
                    {
                        if (VerifyHashedPassword(user.HashedPassword, password))
                        {
                            return new LoggedInUserbyCodeViewModel()
                            {
                                FirstName = user.FirstName,
                                LastName = user.LastName,
                                UserId = user.UserId,
                                Username = user.Username,
                                email = user.email,
                            };
                        }
                        return null;
                    }
                    return null;
                }
            }
                catch (Exception ex)
            {
                throw ex;
            }
        }
        public LoggedInUserbyCodeViewModel VerifyUserResend(long userid)
        {
            if (userid == 0)
                return null;
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var user = (from u in ctx.Users
                                where (u.IsActive ?? false) == true &&
                                (u.IsDeleted ?? false) == false &&
                                (u.UserId == userid)
                                select new
                                {
                                    FirstName = u.FirstName,
                                    LastName = u.LastName,
                                    UserId = u.UserId,
                                    email = u.Email,
                                    Username = u.UserName,
                                    HashedPassword = u.Password,

                                }).FirstOrDefault();
                    if (user != null && user.UserId > 0)
                    {

                        return new LoggedInUserbyCodeViewModel()
                        {
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            UserId = user.UserId,
                            Username = user.Username,
                            email = user.email,
                        };

                        return null;
                    }
                    return null;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }


        public LoggedInUserViewModel VerifyUserCode(string code, long userid)
        {
            if (code == null || userid == null)
                return null;
            try
            {

                List<Nullable<int>> isExpired = null;
                bool isExp = false;

                using (
                    var ctx = new NPMDBEntities())
                {
                    long ccc = Convert.ToInt64(code);


                    var verification = ctx.TWO_FACTOR_AUTHORAZITION.Where(t => (t.OTP == ccc) && (t.UserId == userid)).ToList();
                    isExpired = ctx.uspAuthTWOFAC(userid, ccc).ToList();

                    if (verification.Count > 0)
                    {
                        if (isExpired.Count > 0)
                        {
                            foreach (var ver in isExpired)
                            {

                                if (ver == 1)
                                {
                                    isExp = true;
                                    foreach (var ver2 in verification)
                                    {
                                        ver2.ISEXPIXED = isExp;
                                        ver2.VerificationStatus = false;
                                        ctx.SaveChanges();
                                    }
                                    return new LoggedInUserViewModel
                                    {

                                        FirstName = null,
                                        LastName = null,
                                        UserId = 0,
                                        Username = null,
                                        Practices = null,
                                        Role = null,
                                        email = null,
                                        RoleId = 0,
                                    };
                                }
                                else
                                {

                                    var user = (from u in ctx.Users
                                                join r in ctx.Roles on u.RoleId equals r.RoleId
                                                join upp in ctx.Users_Practice_Provider on u.UserId equals upp.User_Id into uupp
                                                from upp in uupp.DefaultIfEmpty()
                                                where (u.IsActive ?? false) == true &&
                                                (u.IsDeleted ?? false) == false &&
                                                (u.UserId == userid)
                                                select new
                                                {
                                                    FirstName = u.FirstName,
                                                    LastName = u.LastName,
                                                    UserId = u.UserId,
                                                    email = u.Email,
                                                    Username = u.UserName,
                                                    RoleId = r.RoleId,
                                                    Role = r.RoleName,
                                                    HashedPassword = u.Password,
                                                    Practices = (from p in ctx.Practices
                                                                     join puupp in uupp.OrderBy(d => d.UserProviderId) on p.Practice_Code equals puupp.Practice_Code
                                                                     join pr in ctx.Practice_Reporting on p.Practice_Code equals pr.Practice_Code into prGroup
                                                                     from pr in prGroup.DefaultIfEmpty() // Left join to allow missing division
                                                                     where (puupp.Deleted ?? false) == false && (p.Is_Active ?? true) == true
                                                                     select new UserPracticeViewModel
                                                                     {
                                                                         PracticeCode = p.Practice_Code,
                                                                         PracticeName = p.Prac_Name,
                                                                         Billing_Type = p.Billing_Type,
                                                                         Division = pr != null && pr.Division_ID != null ? pr.Division_ID : 0, // Handle null division
                                                                         ClaimNotePopUp=p.ClaimNote_PopUp
                                                                     }).ToList(),

                                    //Practices = (from p in ctx.Practices
                                    //                             join puupp in uupp.OrderBy(d => d.UserProviderId) on p.Practice_Code equals puupp.Practice_Code
                                    //                             where (puupp.Deleted ?? false) == false && (p.Is_Active ?? true) == true
                                    //                             select new UserPracticeViewModel()
                                    //                             {
                                    //                                 PracticeCode = p.Practice_Code,
                                    //                                 PracticeName = p.Prac_Name,
                                    //                                 Billing_Type = p.Billing_Type
                                    //                             }).ToList(),
                                                    //Added by HAMZA ZULFIQAR as per USER STORY 119: Reporting Dashboard Implementation For All Practices
                                                    //    ExternalPractices = ctx.Practice_Reporting
                                                    //                         .Where(pr => (pr.Deleted ?? false) == false)
                                                    //                         .Select(pr => pr.Practice_Code)
                                                    //                         .ToList()
                                                    //}).FirstOrDefault();
                                                    //..Above change is commented to resolve the COSS Practice issue occured due to Practice_Reporting table change
                                                    ExternalPractices = ctx.ExternalPractices_Reporting
                                                                         .Where(pr => (pr.Deleted ?? false) == false)
                                                                         .Select(pr => pr.Practice_Code)
                                                                         .ToList()
                                                }).FirstOrDefault();
                                    foreach (var ver2 in verification)
                                    {
                                        ver2.ISEXPIXED = isExp;
                                        ver2.VerificationStatus = true;
                                        ctx.SaveChanges();
                                    }
                                    if (user != null && user.UserId > 0)
                                    {

                                        return new LoggedInUserViewModel()
                                        {
                                            FirstName = user.FirstName,
                                            LastName = user.LastName,
                                            UserId = user.UserId,
                                            Username = user.Username,
                                            Practices = user.Practices,
                                            Role = user.Role,
                                            email = user.email,
                                            RoleId = user.RoleId
                                        };

                                    }

                                }

                            }
                        }




                    }



                    return null;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private bool ByteArraysEqual(byte[] a, byte[] b)
        {

            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (a == null || b == null || a.Length != b.Length)
            {
                return false;
            }

            bool areSame = true;
            for (int i = 0; i < a.Length; i++)
            {
                areSame &= (a[i] == b[i]);
            }
            return areSame;
        }
    }
}