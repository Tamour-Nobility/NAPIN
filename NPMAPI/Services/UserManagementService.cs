using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using NPMAPI.Enums;
using NPMAPI.Models;
using NPMAPI.Repositories;

namespace NPMAPI.Services
{
    public class UserManagementService : IUserManagementSetup
    {
        #region Module
        public ResponseModel GetModule(long id)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                ModuleViewModel objModule = null;
                using (var ctx = new NPMDBEntities())
                {
                    objModule = (from m in ctx.Modules
                                 join sm in ctx.Sub_Module on m.Module_Id equals sm.Module_Id into msm
                                 from sm in msm.DefaultIfEmpty()
                                 where m.Module_Id == id
                                 select new ModuleViewModel()
                                 {
                                     CreatedBy = m.CreatedBy,
                                     CreatedDate = m.CreatedDate,
                                     Deleted = m.Deleted,
                                     ModifiedBy = m.ModifiedBy,
                                     ModifiedDate = m.ModifiedDate,
                                     Module_Id = m.Module_Id,
                                     Module_Name = m.Module_Name,
                                     SubModules = msm.Where(t => (t.Deleted ?? false) == false).Select(subModule => new SubModuleViewModel()
                                     {
                                         Module_Name = m.Module_Name,
                                         Module_Id = subModule.Module_Id,
                                         ModifiedDate = subModule.ModifiedDate,
                                         ModifiedBy = subModule.ModifiedBy,
                                         Deleted = subModule.Deleted,
                                         CreatedDate = subModule.CreatedDate,
                                         CreatedBy = subModule.CreatedBy,
                                         Sub_Module_Id = subModule.Sub_Module_Id,
                                         Sub_Module_Name = subModule.Sub_Module_Name
                                     }).ToList()
                                 }).FirstOrDefault();
                }

                if (objModule != null)
                {
                    objResponse.Status = "Success";
                    objResponse.Response = objModule;
                }
                else
                {
                    objResponse.Status = "Error";
                }
            }
            catch (Exception)
            {
                objResponse.Status = "Error";
            }
            return objResponse;
        }
        public ResponseModel GetModulesList()
        {
            ResponseModel objResponse = new ResponseModel();
            List<ModuleViewModel> ModuleList = null;
            using (var ctx = new NPMDBEntities())
            {
                try
                {
                    ModuleList = (from m in ctx.Modules
                                  join uCreated in ctx.Users on m.CreatedBy equals uCreated.UserId into uCreatedm
                                  from uCreated in uCreatedm.DefaultIfEmpty()
                                  join uModified in ctx.Users on m.ModifiedBy equals uModified.UserId into uModifiedm
                                  from uModified in uModifiedm.DefaultIfEmpty()
                                  select new ModuleViewModel()
                                  {
                                      Module_Id = m.Module_Id,
                                      Module_Name = m.Module_Name,
                                      CreatedByString = uCreated.UserName,
                                      CreatedDate = m.CreatedDate,
                                      Deleted = m.Deleted,
                                      ModifiedByString = uModified.UserName,
                                      ModifiedDate = m.ModifiedDate
                                  }).Where(x => x.Deleted != true).OrderBy(t => t.Module_Name).ToList();

                }
                catch (Exception ex)
                {
                    objResponse.Status = ex.ToString();
                    return objResponse;
                }
            }

            if (ModuleList != null)
            {
                objResponse.Status = "Success";
                objResponse.Response = ModuleList;
            }
            else
            {
                objResponse.Status = "Error";
            }
            return objResponse;
        }
        public ResponseModel SaveModule([FromBody] ModuleViewModel Model, long uId)
        {
            ResponseModel objResponse = new ResponseModel();
            Module objModel = null;
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    if (Model.Module_Id != 0)
                    {
                        objModel = ctx.Modules.SingleOrDefault(p => p.Module_Id == Model.Module_Id);
                        if (objModel != null)
                        {
                            objModel.Module_Name = Model.Module_Name;
                            objModel.ModifiedBy = uId;
                            objModel.ModifiedDate = DateTime.Now;
                            objModel.Deleted = Model.Deleted;
                            ctx.Entry(objModel).State = System.Data.Entity.EntityState.Modified;
                        }
                    }
                    else
                    {
                        objModel = new Module();
                        long moduleId = Convert.ToInt64(ctx.SP_TableIdGenerator("Module_Id").FirstOrDefault().ToString());
                        objModel.Module_Id = moduleId;
                        objModel.Module_Name = Model.Module_Name;
                        objModel.CreatedBy = uId;
                        objModel.CreatedDate = DateTime.Now;
                        ctx.Modules.Add(objModel);
                        SaveSubModule(new SubModuleViewModel()
                        {
                            Module_Id = moduleId,
                            Sub_Module_Name = objModel.Module_Name
                        });
                    }
                    if (ctx.SaveChanges() > 0)
                    {
                        objResponse.Status = "Success";
                        objResponse.Response = Model.Module_Id;
                    }
                    else
                    {
                        objResponse.Status = "Error";
                    }
                }
            }
            catch (Exception ex)
            {
                objResponse.Status = ex.ToString();
            }
            return objResponse;
        }
        public ResponseModel DeleteModule(long id, long uId)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    Module objModule = ctx.Modules.SingleOrDefault(c => c.Module_Id == id);
                    if (objModule != null)
                    {
                        objModule.Deleted = true;
                        objModule.ModifiedBy = uId;
                        objModule.ModifiedDate = DateTime.Now;
                        if (ctx.SaveChanges() > 0)
                        {
                            objResponse.Status = "Success";
                        }
                    }
                    else
                    {
                        objResponse.Status = "Error";
                    }
                }
            }
            catch (Exception)
            {
                objResponse.Status = "Error";
            }
            return objResponse;
        }
        public ResponseModel GetModulesSelectList(string searchText)
        {
            ResponseModel objResponse = new ResponseModel();
            List<SelectListViewModel> selectListViewModel = null;
            using (var ctx = new NPMDBEntities())
            {
                if (!string.IsNullOrEmpty(searchText))
                {
                    selectListViewModel = ctx.Modules.Where(t => t.Module_Name.Contains(searchText) && (t.Deleted ?? false) == false).Select(x => new SelectListViewModel()
                    {
                        Id = x.Module_Id,
                        Name = x.Module_Name
                    }).OrderBy(t => t.Name).ToList();
                }
                else
                {
                    selectListViewModel = ctx.Modules.Where(t => (t.Deleted ?? false) == false).Select(x => new SelectListViewModel()
                    {
                        Id = x.Module_Id,
                        Name = x.Module_Name
                    }).OrderBy(t => t.Name).ToList();
                }
                if (selectListViewModel != null)
                {
                    objResponse.Response = selectListViewModel;
                    objResponse.Status = "Success";
                }
                else
                {
                    objResponse.Status = "Error";
                }
                return objResponse;
            }
        }

        #endregion Module

        #region UserManagement
        public ResponseModel GetUsersList()
        {
            ResponseModel responseModel = new ResponseModel();
            List<UserViewModel> usersList = null;
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    //usersList = (from u in ctx.Users
                                 //added by kamran
                    usersList = (from u in ctx.Users

                                 join c in ctx.Companies on u.CompanyId equals c.CompanyId into uc
                                 from c in uc.DefaultIfEmpty()
                                 join uCreated in ctx.Users on u.CreatedBy equals uCreated.UserId into uCreatedu
                                 from uCreated in uCreatedu.DefaultIfEmpty()
                                 join uModified in ctx.Users on u.ModifiedBy equals uModified.UserId into uModifiedu
                                 from uModified in uModifiedu.DefaultIfEmpty()
                                 where (u.IsDeleted ?? false) == false
                                 select new UserViewModel()
                                 {
                                     IsDeleted = u.IsDeleted,
                                     Address = u.Address,
                                     City = u.City,
                                     CompanyId = u.CompanyId,
                                     CompanyName = c.CompanyName,
                                     UserId = u.UserId,
                                     CreatedByString = uCreated.UserName,
                                     CreatedDate = u.CreatedDate,
                                     Email = u.Email,
                                     FirstName = u.FirstName,
                                     IsActive = u.IsActive,
                                     IsEmployee = u.IsEmployee,
                                     LastName = u.LastName,
                                     MiddleInitial = u.MiddleInitial,
                                     ModfiedByString = uModified.UserName,
                                     ModifiedDate = u.ModifiedDate,
                                     PostalCode = u.PostalCode,
                                     State = u.State,
                                     UserName = u.UserName
                                 }).ToList();
                }
                if (usersList != null)
                {
                    responseModel.Status = "Success";
                    responseModel.Response = usersList;
                }
                else
                {
                    responseModel.Status = "No Records found.";
                }
            }
            catch (Exception ex)
            {
                responseModel.Status = ex.ToString();
            }
            return responseModel;
        }
        public ResponseModel SaveUser(UserCreateViewModel model, long uId)
        {
            ResponseModel resp = new ResponseModel();
            // User user;
            //added by kamran
            User user;
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    if (model.UserId != 0)
                    {
                        // user = ctx.Users.FirstOrDefault(t => t.UserId == model.UserId);
                        // added by kamran
                        user = ctx.Users.FirstOrDefault(t => t.UserId == model.UserId);
                        if (user != null)
                        {
                            user.IsActive = model.IsActive;
                            user.CompanyId = model.CompanyId;
                            user.IsEmployee = model.IsEmployee;
                            user.Address = model.Address;
                            user.City = model.City;
                            user.Email = model.Email;
                            user.FirstName = model.FirstName;
                            user.LastName = model.LastName;
                            user.MiddleInitial = model.MiddleInitial;
                            user.ModifiedBy = uId;
                            user.ModifiedDate = DateTime.Now;
                            user.PostalCode = model.PostalCode;
                            user.State = model.State;
                            user.RoleId = model.RoleId;
                            user.EmpId = model.Empid;
                            ctx.Entry(user).State = System.Data.Entity.EntityState.Modified;
                        }
                    }
                    else
                    {
                        var usernameAplhaPart = model.FirstName.Substring(0, 1) + model.LastName.Substring(0, 1);
                        int count = ctx.Users.Where(u => u.UserName.Substring(0, 2).ToLower() == usernameAplhaPart.ToLower()).Count();
                        count++;
                        model.UserId = Convert.ToInt64(ctx.SP_TableIdGenerator("UserId").FirstOrDefault().ToString());
                        //ctx.Users.Add(user = new User()
                        ctx.Users.Add(user = new User()
                        {
                            IsActive = model.IsActive,
                            CompanyId = model.CompanyId,
                            IsEmployee = model.IsEmployee,
                            Address = model.Address,
                            City = model.City,
                            CreatedBy = uId,
                            CreatedDate = DateTime.Now,
                            Email = model.Email,
                            FirstName = model.FirstName,
                            LastName = model.LastName,
                            MiddleInitial = model.MiddleInitial,
                            Password = model.Password,
                            PostalCode = model.PostalCode,
                            State = model.State,
                            UserId = model.UserId,
                            RoleId = model.RoleId,
                            // addded by kamran 
                            EmpId = model.Empid,
                            UserName = usernameAplhaPart + count
                        });
                    }
                    if (ctx.SaveChanges() > 0)
                    {
                        AddUpdateUserPractices(model, uId);
                        MarkUserAsAnEmployee(user.UserId, uId);
                        resp.Status = "Success";
                    }
                    else
                    {
                        resp.Status = "Error";
                    }
                }
            }
            catch (Exception ex)
            {
                resp.Status = ex + "";
            }
            return resp;
        }
        private ResponseModel AddUpdateUserPractices(UserCreateViewModel model, long uId)
        {
            ResponseModel res = new ResponseModel();

            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var existingUserPractices = ctx.Users_Practice_Provider.Where(p => p.User_Id == model.UserId).ToList();
                    if (existingUserPractices.Count() > 0)
                    {
                        existingUserPractices.ForEach(p => p.Deleted = true);
                        existingUserPractices.ForEach(p => p.Modified_By = uId);
                        existingUserPractices.ForEach(p => p.Modified_Date = DateTime.Now);
                        ctx.SaveChanges();
                    }
                    var newUserPractices = new List<Users_Practice_Provider>();
                    foreach (var item in model.Practices)
                    {
                        var practice = ctx.Practices.FirstOrDefault(p => p.Practice_Code == item.Id);
                        if (practice != null)
                        {
                            newUserPractices.Add(new Users_Practice_Provider()
                            {
                                Created_By = uId,
                                Created_Date = DateTime.Now,
                                Practice_Code = practice.Practice_Code,
                                UserProviderId = Convert.ToInt64(ctx.SP_TableIdGenerator("UserProviderId").FirstOrDefault().ToString()),
                                User_Id = model.UserId
                            });
                        }
                    }
                    ctx.Users_Practice_Provider.AddRange(newUserPractices);
                    ctx.SaveChanges();
                    res.Status = "Success";
                }
            }
            catch (Exception ex)
            {
                res.Status = ex.ToString();
            }
            return res;
        }
        private ResponseModel MarkUserAsAnEmployee(long userId, long uId)
        {
            ResponseModel responseModel = new ResponseModel();
            EmployeeInformation employeeInformation;
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    employeeInformation = ctx.EmployeeInformations.FirstOrDefault(e => e.UserId == userId);
                    if (employeeInformation == null)
                    {
                        ctx.EmployeeInformations.Add(employeeInformation = new EmployeeInformation()
                        {
                            Employee_Id = Convert.ToInt64(ctx.SP_TableIdGenerator("Employee_Id").FirstOrDefault().ToString()),
                            Created_By = uId,
                            Created_Date = DateTime.Now,
                            UserId = userId
                        });
                        if (ctx.SaveChanges() > 0)
                        {
                            responseModel.Status = "Success";
                            responseModel.Response = employeeInformation;
                        }
                        else
                        {
                            responseModel.Status = "Error";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                responseModel.Status = ex.ToString();
            }
            return responseModel;
        }
        public ResponseModel GetUser(long Id)
        {
            ResponseModel resp = new ResponseModel();
            UserViewModel user;
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    //user = (from u in ctx.Users
                    //added by kamran
                    user = (from u in ctx.Users
                            join c in ctx.Companies on u.CompanyId equals c.CompanyId into uc
                            from c in uc.DefaultIfEmpty()
                            join upp in ctx.Users_Practice_Provider on u.UserId equals upp.User_Id into uupp
                            from upp in uupp.DefaultIfEmpty()
                            where u.UserId == Id
                            select new UserViewModel()
                            {
                                UserId = u.UserId,
                                Address = u.Address,
                                City = u.City,
                                CompanyId = u.CompanyId,
                                CreatedBy = u.CreatedBy,
                                CreatedDate = u.CreatedDate,
                                Email = u.Email,
                                FirstName = u.FirstName,
                                IsActive = u.IsActive,
                                IsDeleted = u.IsDeleted,
                                IsEmployee = u.IsEmployee,
                                LastName = u.LastName,
                                MiddleInitial = u.MiddleInitial,
                                ModifiedBy = u.ModifiedBy,
                                ModifiedDate = u.ModifiedDate,
                                PasswordChangeDate = u.PasswordChangeDate,
                                PostalCode = u.PostalCode,
                                State = u.State,
                                CompanyName = c.CompanyName,
                                RoleId = u.RoleId,
                                Empid = u.EmpId,
                                Practices = (from p in ctx.Practices
                                             join upu in uupp on p.Practice_Code equals upu.Practice_Code
                                             where (upu.Deleted ?? false) == false
                                             select new SelectListViewModel()
                                             {
                                                 Id = p.Practice_Code,
                                                 Name = p.Practice_Code + "|" + p.Prac_Name
                                             }).ToList()
                            }).FirstOrDefault();
                    if (user != null)
                    {
                        resp.Status = "Success";
                        resp.Response = user;
                    }
                }
            }
            catch (Exception ex)
            {
                resp.Status = ex + "";
            }
            return resp;
        }
        public ResponseModel ResetPassword(ResetPasswordViewModel model, long uId)
        {
            ResponseModel res = new ResponseModel();
            User user;
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    user = ctx.Users.FirstOrDefault(u => u.UserId == model.UserId);
                    if (user != null)
                    {
                        user.Password = model.Password;
                        user.PasswordChangeDate = DateTime.Now;
                        user.ModifiedBy = uId;
                        user.ModifiedDate = DateTime.Now;
                        ctx.Entry(user).State = System.Data.Entity.EntityState.Modified;
                        if (ctx.SaveChanges() > 0)
                        {
                            res.Status = "Success";
                        }
                        else
                        {
                            res.Status = "Error";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                res.Status = ex + "";
            }
            return res;
        }
        public ResponseModel ResetPasswordForUser(ResetPasswordForuserViewModel model, long uId)
        {
            ResponseModel res = new ResponseModel();
            User user;
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    user = ctx.Users.FirstOrDefault(u => u.UserId == model.UserId);
                    if (user != null)
                    {
                        user.Password = model.Password;
                        user.PasswordChangeDate = DateTime.Now;
                        user.ModifiedBy = uId;
                        user.ModifiedDate = DateTime.Now;
                        ctx.Entry(user).State = System.Data.Entity.EntityState.Modified;
                        if (ctx.SaveChanges() > 0)
                        {
                            res.Status = "Success";
                        }
                        else
                        {
                            res.Status = "Error";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                res.Status = ex + "";
            }
            return res;
        }
        public ResponseModel DeleteUser(long id, long uId)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    User objUser = ctx.Users.SingleOrDefault(c => c.UserId == id);
                    if (objUser != null)
                    {
                        objUser.IsDeleted = true;
                        objUser.ModifiedBy = uId;
                        objUser.ModifiedDate = DateTime.Now;
                        if (ctx.SaveChanges() > 0)
                        {
                            objResponse.Status = "Success";
                        }
                    }
                    else
                    {
                        objResponse.Status = "Error";
                    }
                }
            }
            catch (Exception)
            {
                objResponse.Status = "Error";
            }
            return objResponse;
        }
        public ResponseModel ChangeUserStatus(UserStatusChangeViewModel model, long uId)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    User objUser = ctx.Users.SingleOrDefault(c => c.UserId == model.UserId);
                    if (objUser != null)
                    {
                        objUser.IsActive = model.Status;
                        objUser.ModifiedDate = DateTime.Now;
                        objUser.ModifiedBy = uId;
                        if (ctx.SaveChanges() > 0)
                        {
                            objResponse.Status = "Success";
                        }
                    }
                    else
                    {
                        objResponse.Status = "Error";
                    }
                }
            }
            catch (Exception)
            {
                objResponse.Status = "Error";
            }
            return objResponse;
        }
        public ResponseModel GetUsersSelectList()
        {
            ResponseModel objResponse = new ResponseModel();
            List<SelectListViewModel> selectListViewModel = null;
            using (var ctx = new NPMDBEntities())
            {

                selectListViewModel = (from u in ctx.Users.OrderBy(u => u.LastName)
                                       join r in ctx.Roles on u.RoleId equals r.RoleId into uc
                                       from r in uc.DefaultIfEmpty()
                                       where (u.IsDeleted ?? false) == false
                                       select new SelectListViewModel()
                                       {
                                           Id = u.UserId,
                                           Name = u.UserId + " - " + u.LastName + ", " + u.FirstName
                                       }).ToList();

                if (selectListViewModel != null)
                {
                    objResponse.Response = selectListViewModel;
                    objResponse.Status = "Success";
                }
                else
                {
                    objResponse.Status = "Error";
                }
                return objResponse;
            }
        }
        public LoggedInUserViewModel GetUser(string username, string password)
        {
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    return (from u in ctx.Users
                            join r in ctx.Roles on u.RoleId equals r.RoleId into ur
                            join upp in ctx.Users_Practice_Provider on u.UserId equals upp.User_Id into uupp
                            from upp in uupp.DefaultIfEmpty()
                            from r in ur.DefaultIfEmpty()
                            where (u.IsActive ?? false) == true &&
                            (u.IsDeleted ?? false) == false &&
                            (u.UserName.Trim().ToLower() == username.Trim().ToLower() || u.Email.Trim().ToLower() == username.Trim().ToLower()) &&
                            u.Password.Trim() == password.Trim()
                            select new LoggedInUserViewModel()
                            {
                                FirstName = u.FirstName,
                                LastName = u.LastName,
                                UserId = u.UserId,
                                Username = u.UserName,
                                RoleId = r.RoleId,
                                Role = r.RoleName,
                                Practices = (from p in ctx.Practices
                                             join puupp in uupp.OrderBy(d => d.UserProviderId) on p.Practice_Code equals puupp.Practice_Code
                                             where (puupp.Deleted ?? false) == false
                                             select new UserPracticeViewModel()
                                             {
                                                 PracticeCode = p.Practice_Code,
                                                 PracticeName = p.Prac_Name
                                             }).ToList()
                            }).FirstOrDefault();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        public List<SP_GetUserAuthorization_Result> GetUserRoleAndRights(long userId)
        {
            List<SP_GetUserAuthorization_Result> result = new List<SP_GetUserAuthorization_Result>();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    result = ctx.SP_GetUserAuthorization(userId).ToList();

                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return result;
        }
        public ResponseModel verifyEmail(string vEmail)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    //if (ctx.Users.Where(s => s.Email == vEmail && (s.IsDeleted ?? false) == false).FirstOrDefault() != null)
                    // added by kamran by email id check
                    if (ctx.Users.Where(s => s.Email == vEmail && (s.IsDeleted ?? false) == false).FirstOrDefault() != null)
                    {
                        objResponse.Response = true;
                    }
                    else
                    {
                        objResponse.Response = false;
                    }
                    objResponse.Status = "Success";
                }

            }
            catch (Exception ex)
            {
                objResponse.Status = ex.ToString();
            }
            return objResponse;
        }
        //added by kamran empId is exist
        public ResponseModel empIdExists(long vEmpId)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    if (ctx.Users.Where(s => s.EmpId == vEmpId && (s.IsDeleted ?? false) == false).FirstOrDefault() != null)
                    {
                        objResponse.Response = true;
                    }
                    else
                    {
                        objResponse.Response = false;
                    }
                    objResponse.Status = "Success";
                }

            }
            catch (Exception ex)
            {
                objResponse.Status = ex.ToString();
            }
            return objResponse;
        }

        #endregion

        #region Roles
        public ResponseModel GetRoleList()
        {
            ResponseModel objResponse = new ResponseModel();
            List<RoleViewModel> RoleList = null;
            using (var ctx = new NPMDBEntities())
            {
                try
                {
                    RoleList = ctx.Roles.Where(r => (r.Deleted ?? false) == false).Select(r => new RoleViewModel()
                    {
                        Deleted = r.Deleted,
                        CreatedBy = r.CreatedBy,
                        CreatedDate = r.CreatedDate,
                        ModifiedBy = r.ModifiedBy,
                        ModifiedDate = r.ModifiedDate,
                        RoleId = r.RoleId,
                        RoleName = r.RoleName
                    }).ToList();
                }
                catch (Exception ex)
                {
                    objResponse.Status = ex.ToString();
                    return objResponse;
                }
            }

            if (RoleList != null)
            {
                objResponse.Status = "Success";
                objResponse.Response = RoleList;
            }
            else
            {
                objResponse.Status = "Error";
            }
            return objResponse;
        }
        public ResponseModel SaveRole(RoleViewModel model)
        {
            ResponseModel responseModel = new ResponseModel();
            Role role = null;
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    role = ctx.Roles.SingleOrDefault(r => r.RoleId == model.RoleId);
                    if (role != null)
                    {
                        role.RoleName = model.RoleName;
                        role.ModifiedBy = 1;
                        role.ModifiedDate = DateTime.Now;
                        role.Deleted = model.Deleted;
                        ctx.Entry(role).State = System.Data.Entity.EntityState.Modified;
                    }
                    else
                    {
                        role = new Role()
                        {
                            RoleId = Convert.ToInt64(ctx.SP_TableIdGenerator("RoleId").FirstOrDefault().ToString()),
                            Deleted = false,
                            RoleName = model.RoleName
                        };
                        ctx.Roles.Add(role);
                    }
                    if (ctx.SaveChanges() > 0)
                    {
                        responseModel.Status = "Success";
                        responseModel.Response = model.RoleId;
                    }
                }
            }
            catch (Exception ex)
            {
                responseModel.Status = ex.ToString();
                responseModel.Response = model.RoleId;
            }
            return responseModel;
        }
        public ResponseModel GetRole(long id)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                Role ObjRole = null;
                using (var ctx = new NPMDBEntities())
                {
                    ObjRole = ctx.Roles.SingleOrDefault(c => c.RoleId == id);
                }

                if (ObjRole != null)
                {
                    objResponse.Status = "Success";
                    objResponse.Response = ObjRole;
                }
                else
                {
                    objResponse.Status = "Error";
                }
            }
            catch (Exception)
            {
                objResponse.Status = "Error";
            }
            return objResponse;
        }
        public ResponseModel DeleteRole(long id)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    Role objRole = ctx.Roles.SingleOrDefault(c => c.RoleId == id);
                    if (objRole != null)
                    {
                        objRole.Deleted = true;
                        if (ctx.SaveChanges() > 0)
                        {
                            objResponse.Status = "Success";
                        }
                    }
                    else
                    {
                        objResponse.Status = "Error";
                    }
                }
            }
            catch (Exception)
            {
                objResponse.Status = "Error";
            }
            return objResponse;
        }
        public ResponseModel GetRolesSelectList()
        {
            ResponseModel resModel = new ResponseModel();
            List<SelectListViewModel> rolesSelectList;
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    rolesSelectList = ctx.Roles.Where(t => (t.Deleted ?? false) == false).Select(x => new SelectListViewModel()
                    {
                        Id = x.RoleId,
                        Name = x.RoleName
                    }).ToList();
                    if (rolesSelectList != null)
                    {
                        resModel.Status = "Success";
                        resModel.Response = rolesSelectList;
                    }
                    else
                    {
                        resModel.Status = "No Record found.";
                    }
                }
            }
            catch (Exception ex)
            {
                resModel.Status = ex.ToString();
            }
            return resModel;
        }
        public ResponseModel GetTemplateByRoleId(long roleId)
        {
            ResponseModel resModel = new ResponseModel();
            List<TreeViewItem> response;
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    response = new List<TreeViewItem>();
                    var modules = ctx.Modules.Where(m => (m.Deleted ?? false) == false).ToList();
                    foreach (var m in modules)
                    {
                        var item = new TreeViewItem()
                        {
                            text = m.Module_Name,
                            value = m.Module_Id,
                            collapsed = true,
                            @checked = false,
                            children = ctx.Sub_Module.Where(sm => sm.Module_Id == m.Module_Id && (sm.Deleted ?? false) == false).Select(sm => new TreeViewItem1()
                            {
                                text = sm.Sub_Module_Name,
                                value = sm.Sub_Module_Id,
                                collapsed = true,
                                @checked = false,
                                children = ctx.Module_Properties.Where(p => p.Sub_Module_Id == sm.Sub_Module_Id && (sm.Deleted ?? false) == false).Select(p => new TreeViewItem()
                                {
                                    text = p.Property_Name,
                                    value = p.Property_Id,
                                    collapsed = true,
                                    @checked = false
                                }).ToList()
                            }).ToList()
                        };
                        response.Add(item);
                    }
                    foreach (var mod in response)
                    {
                        foreach (var subMo in mod.children)
                        {
                            foreach (var prop in subMo.children)
                            {
                                var p = ctx.Roles_Module_Properties.Where(i => i.Module_Id == mod.value && i.Sub_Module_Id == subMo.value && i.Property_Id == prop.value && i.Role_Id == roleId).FirstOrDefault();
                                if (p != null)
                                {
                                    prop.@checked = true;
                                }
                            }
                        }
                    }

                    if (response != null)
                    {
                        resModel.Status = "Success";
                        resModel.Response = response;
                    }
                    else
                    {
                        resModel.Status = "No Record Found";
                    }
                }
            }
            catch (Exception ex)
            {
                resModel.Status = ex.ToString();
            }
            return resModel;
        }
        public ResponseModel SaveRoleModuleProperties(RoleModulePropertyCreateViewModel model)
        {
            ResponseModel response = new ResponseModel();
            List<Roles_Module_Properties> newProperties;
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var existingRoleModuleProperties = ctx.Roles_Module_Properties.Where(r => r.Role_Id == model.RoleId);
                    if (existingRoleModuleProperties.Count() > 0)
                    {
                        ctx.Roles_Module_Properties.RemoveRange(existingRoleModuleProperties);
                    }
                    newProperties = new List<Roles_Module_Properties>();
                    foreach (var item in model.Properties)
                    {
                        newProperties.Add(new Roles_Module_Properties()
                        {
                            Role_Id = item.Role_Id,
                            CREATED_BY = 0,
                            CREATED_DATE = DateTime.Now,
                            Module_Id = item.Module_Id,
                            Property_Id = item.Property_Id,
                            Sub_Module_Id = item.Sub_Module_Id
                        });
                    }
                    ctx.Roles_Module_Properties.AddRange(newProperties);
                    if (ctx.SaveChanges() > 0)
                    {
                        response.Status = "Success";
                    }
                    else
                    {
                        response.Status = "Error";
                    }
                }
            }
            catch (Exception ex)
            {
                response.Status = ex.ToString();
            }
            return response;
        }
        public ResponseModel GetTemplateByUserId(long userId)

        {
            ResponseModel resModel = new ResponseModel();
            List<TreeViewItem> response;
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    response = new List<TreeViewItem>();
                    var modules = ctx.Modules.Where(m => (m.Deleted ?? false) == false).ToList();
                    foreach (var m in modules)
                    {
                        var item = new TreeViewItem()
                        {
                            text = m.Module_Name,
                            value = m.Module_Id,
                            collapsed = true,
                            @checked = false,
                            children = ctx.Sub_Module.Where(sm => sm.Module_Id == m.Module_Id && (sm.Deleted ?? false) == false).Select(sm => new TreeViewItem1()
                            {
                                text = sm.Sub_Module_Name,
                                value = sm.Sub_Module_Id,
                                collapsed = true,
                                @checked = false,
                                children = ctx.Module_Properties.Where(p => p.Sub_Module_Id == sm.Sub_Module_Id && (sm.Deleted ?? false) == false).Select(p => new TreeViewItem()
                                {
                                    text = p.Property_Name,
                                    value = p.Property_Id,
                                    collapsed = true,
                                    @checked = false
                                }).ToList()
                            }).ToList()
                        };
                        response.Add(item);
                    }
                    foreach (var mod in response)
                    {
                        foreach (var subMo in mod.children)
                        {
                            foreach (var prop in subMo.children)
                            {
                                var p = ctx.Users_Module_Properties.Where(i => i.ModuleId == mod.value && i.Sub_Module_Id == subMo.value && i.PropertyId == prop.value && i.UserId == userId).FirstOrDefault();
                                if (p != null)
                                {
                                    prop.@checked = true;
                                }
                            }
                        }
                    }

                    if (response != null)
                    {
                        resModel.Status = "Success";
                        resModel.Response = response;
                    }
                    else
                    {
                        resModel.Status = "No Record Found";
                    }
                }
            }
            catch (Exception ex)
            {
                resModel.Status = ex.ToString();
            }
            return resModel;
        }
        public ResponseModel SaveUserModuleProperties(UserModulePropertyCreateViewModel model)
        {
            ResponseModel response = new ResponseModel();
            List<Users_Module_Properties> newProperties;
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var existingUserModuleProperties = ctx.Users_Module_Properties.Where(r => r.UserId == model.UserId);
                    if (existingUserModuleProperties.Count() > 0)
                    {
                        ctx.Users_Module_Properties.RemoveRange(existingUserModuleProperties);
                    }
                    newProperties = new List<Users_Module_Properties>();
                    foreach (var item in model.Properties)
                    {
                        newProperties.Add(new Users_Module_Properties()
                        {
                            UserId = item.User_Id,
                            CREATED_BY = 0,
                            CREATED_DATE = DateTime.Now,
                            ModuleId = item.Module_Id,
                            PropertyId = item.Property_Id,
                            Sub_Module_Id = item.Sub_Module_Id
                        });
                    }
                    ctx.Users_Module_Properties.AddRange(newProperties);
                    if (ctx.SaveChanges() > 0)
                    {
                        response.Status = "Success";
                    }
                    else
                    {
                        response.Status = "Error";
                    }
                }
            }
            catch (Exception ex)
            {
                response.Status = ex.ToString();
            }
            return response;
        }

        #endregion

        #region SubModule
        public ResponseModel GetSubModule(long id)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                SubModuleViewModel objSubModule = null;
                using (var ctx = new NPMDBEntities())
                {
                    objSubModule = (from sm in ctx.Sub_Module
                                    join m in ctx.Modules on sm.Module_Id equals m.Module_Id
                                    where sm.Sub_Module_Id == id
                                    select new SubModuleViewModel()
                                    {
                                        Sub_Module_Id = sm.Sub_Module_Id,
                                        CreatedBy = sm.CreatedBy,
                                        CreatedDate = sm.CreatedDate,
                                        Deleted = sm.Deleted,
                                        ModifiedBy = sm.ModifiedBy,
                                        ModifiedDate = sm.ModifiedDate,
                                        Module_Id = sm.Module_Id,
                                        Module_Name = m.Module_Name,
                                        Sub_Module_Name = sm.Sub_Module_Name,
                                        module = m
                                    }).FirstOrDefault();
                }
                if (objSubModule != null)
                {
                    objResponse.Status = "Success";
                    objResponse.Response = objSubModule;
                }
                else
                {
                    objResponse.Status = "Error";
                }
            }
            catch (Exception)
            {
                objResponse.Status = "Error";
            }
            return objResponse;
        }
        public ResponseModel GetSubModuleList(long? moduleId)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                List<SubModuleViewModel> subModulesList = null;
                using (var ctx = new NPMDBEntities())
                {
                    subModulesList = (from sm in ctx.Sub_Module
                                      join m in ctx.Modules on sm.Module_Id equals m.Module_Id
                                      where (sm.Deleted ?? false) == false
                                      select new SubModuleViewModel()
                                      {
                                          Module_Id = sm.Module_Id,
                                          Module_Name = m.Module_Name,
                                          CreatedBy = sm.CreatedBy,
                                          CreatedDate = sm.CreatedDate,
                                          Deleted = sm.Deleted,
                                          ModifiedBy = sm.ModifiedBy,
                                          ModifiedDate = sm.ModifiedDate,
                                          Sub_Module_Id = sm.Sub_Module_Id,
                                          Sub_Module_Name = sm.Sub_Module_Name
                                      }).OrderBy(m => m.Sub_Module_Name).ToList();
                    if (moduleId != 0)
                    {
                        subModulesList = subModulesList.Where(m => m.Module_Id == moduleId).ToList();
                    }
                    if (subModulesList != null)
                    {
                        objResponse.Status = "Success";
                        objResponse.Response = subModulesList;
                    }
                    else
                    {
                        objResponse.Status = "Error";
                    }
                }
            }
            catch (Exception ex)
            {
                objResponse.Status = ex + "";
            }
            return objResponse;
        }
        public ResponseModel SaveSubModule(SubModuleViewModel model)
        {
            ResponseModel objResponse = new ResponseModel();
            Sub_Module objSubModule = null;
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    if (model.Sub_Module_Id != 0)
                    {
                        objSubModule = ctx.Sub_Module.Where(sm => sm.Sub_Module_Id == model.Sub_Module_Id).FirstOrDefault();
                        if (objSubModule != null)
                        {
                            objSubModule.Module_Id = model.Module_Id;
                            objSubModule.Sub_Module_Name = model.Sub_Module_Name;
                            objSubModule.ModifiedBy = 0;
                            objSubModule.ModifiedDate = DateTime.Now;
                            ctx.Entry(objSubModule).State = System.Data.Entity.EntityState.Modified;
                        }
                    }
                    else
                    {
                        objSubModule = new Sub_Module()
                        {
                            Sub_Module_Id = Convert.ToInt64(ctx.SP_TableIdGenerator("Sub_Module_Id").FirstOrDefault().ToString()),
                            Module_Id = model.Module_Id,
                            Sub_Module_Name = model.Sub_Module_Name
                        };
                        ctx.Sub_Module.Add(objSubModule);
                        if ((model.IsDefaultPropertiesCheck ?? false))
                        {
                            var defaultProperties = new List<Module_Properties>();
                            foreach (DefaultProperties Property in (DefaultProperties[])Enum.GetValues(typeof(DefaultProperties)))
                            {
                                defaultProperties.Add(new Module_Properties()
                                {
                                    Property_Id = Convert.ToInt64(ctx.SP_TableIdGenerator("Property_Id").FirstOrDefault().ToString()),
                                    CreatedBy = 0,
                                    CreatedDate = DateTime.Now,
                                    Property_Name = Property.ToString(),
                                    Sub_Module_Id = objSubModule.Sub_Module_Id
                                });
                            }
                            ctx.Module_Properties.AddRange(defaultProperties);
                        }
                    };
                    if (ctx.SaveChanges() > 0)
                    {
                        objResponse.Status = "Success";
                    }
                    else
                    {
                        objResponse.Status = "Error";
                    }
                }
            }
            catch (Exception ex)
            {
                objResponse.Status = ex.ToString();
            }
            return objResponse;
        }
        public ResponseModel DeleteSubModule(long id)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    Sub_Module objSubModule = ctx.Sub_Module.SingleOrDefault(c => c.Sub_Module_Id == id);
                    if (objSubModule != null)
                    {
                        objSubModule.Deleted = true;
                        if (ctx.SaveChanges() > 0)
                        {
                            objResponse.Status = "Success";
                        }
                    }
                    else
                    {
                        objResponse.Status = "Error";
                    }
                }
            }
            catch (Exception)
            {
                objResponse.Status = "Error";
            }
            return objResponse;
        }
        #endregion

        #region SubModuleProperties
        public ResponseModel GetSubModulePropertiesList(long? subModuleId)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                List<SubModulePropertiesViewModel> modulePropertiesList = null;
                using (var ctx = new NPMDBEntities())
                {
                    modulePropertiesList = (from mp in ctx.Module_Properties
                                            join sm in ctx.Sub_Module on mp.Sub_Module_Id equals sm.Sub_Module_Id into mpsm
                                            from sm in mpsm.DefaultIfEmpty()
                                            where (mp.Deleted ?? false) == false
                                            select new SubModulePropertiesViewModel()
                                            {
                                                Deleted = mp.Deleted,
                                                CreatedBy = mp.CreatedBy,
                                                CreatedDate = mp.CreatedDate,
                                                ModifiedBy = mp.ModifiedBy,
                                                ModifiedDate = mp.ModifiedDate,
                                                Property_Id = mp.Property_Id,
                                                Property_Name = mp.Property_Name,
                                                Sub_Module_Id = mp.Sub_Module_Id,
                                                Sub_Module_Name = sm.Sub_Module_Name
                                            }).OrderBy(t => t.Property_Name).ToList();
                    if (subModuleId != 0)
                    {
                        modulePropertiesList = modulePropertiesList.Where(m => m.Sub_Module_Id == subModuleId).ToList();
                    }
                    if (modulePropertiesList != null)
                    {
                        objResponse.Status = "Success";
                        objResponse.Response = modulePropertiesList;
                    }
                    else
                    {
                        objResponse.Status = "Error";
                    }
                }
            }
            catch (Exception ex)
            {
                objResponse.Status = "Error";
            }
            return objResponse;

        }
        public ResponseModel GetSubModuleProperty(long id)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                SubModulePropertiesViewModel objModuleProperties = null;
                using (var ctx = new NPMDBEntities())
                {
                    objModuleProperties = (from mp in ctx.Module_Properties
                                           join sm in ctx.Sub_Module on mp.Sub_Module_Id equals sm.Sub_Module_Id into mpsm
                                           from sm in mpsm.DefaultIfEmpty()
                                           join m in ctx.Modules on sm.Module_Id equals m.Module_Id
                                           where mp.Property_Id == id
                                           select new SubModulePropertiesViewModel()
                                           {
                                               CreatedBy = mp.CreatedBy,
                                               CreatedDate = mp.CreatedDate,
                                               Deleted = mp.Deleted,
                                               ModifiedBy = mp.ModifiedBy,
                                               ModifiedDate = mp.ModifiedDate,
                                               Property_Id = mp.Property_Id,
                                               Property_Name = mp.Property_Name,
                                               Sub_Module_Id = mp.Sub_Module_Id,
                                               Sub_Module_Name = sm.Sub_Module_Name,
                                               subModule = sm,
                                               ModuleId = m.Module_Id,
                                               ModuleName = m.Module_Name
                                           }).FirstOrDefault();
                }
                if (objModuleProperties != null)
                {
                    objResponse.Status = "Success";
                    objResponse.Response = objModuleProperties;
                }
                else
                {
                    objResponse.Status = "Error";
                }
            }
            catch (Exception)
            {
                objResponse.Status = "Error";
            }
            return objResponse;
        }
        public ResponseModel GetSubModulesSelectList(string searchText)
        {
            ResponseModel objResponse = new ResponseModel();
            List<SelectListViewModel> selectListViewModel = null;
            using (var ctx = new NPMDBEntities())
            {
                if (!string.IsNullOrEmpty(searchText))
                {
                    selectListViewModel = ctx.Sub_Module.Where(t => t.Sub_Module_Name.Contains(searchText) && (t.Deleted ?? false) == false).Select(x => new SelectListViewModel()
                    {
                        Id = x.Sub_Module_Id,
                        Name = x.Sub_Module_Name
                    }).OrderBy(t => t.Name).ToList();
                }
                else
                {
                    selectListViewModel = ctx.Sub_Module.Where(t => (t.Deleted ?? false) == false).Select(x => new SelectListViewModel()
                    {
                        Id = x.Sub_Module_Id,
                        Name = x.Sub_Module_Name
                    }).OrderBy(t => t.Name).ToList();
                }
                if (selectListViewModel != null)
                {
                    objResponse.Response = selectListViewModel;
                    objResponse.Status = "Success";
                }
                else
                {
                    objResponse.Status = "Error";
                }
                return objResponse;
            }
        }
        public ResponseModel GetSubModulesSelectList(string searchText, long moduleId)
        {
            ResponseModel objResponse = new ResponseModel();
            List<SelectListViewModel> selectListViewModel = null;
            using (var ctx = new NPMDBEntities())
            {
                if (!string.IsNullOrEmpty(searchText))
                {
                    selectListViewModel = ctx.Sub_Module.Where(t => t.Sub_Module_Name.Contains(searchText) && (t.Deleted ?? false) == false && t.Module_Id == moduleId).Select(x => new SelectListViewModel()
                    {
                        Id = x.Sub_Module_Id,
                        Name = x.Sub_Module_Name
                    }).OrderBy(t => t.Name).ToList();
                }
                else
                {
                    selectListViewModel = ctx.Sub_Module.Where(t => (t.Deleted ?? false) == false && t.Module_Id == moduleId).Select(x => new SelectListViewModel()
                    {
                        Id = x.Sub_Module_Id,
                        Name = x.Sub_Module_Name
                    }).OrderBy(t => t.Name).ToList();
                }
                if (selectListViewModel != null)
                {
                    objResponse.Response = selectListViewModel;
                    objResponse.Status = "Success";
                }
                else
                {
                    objResponse.Status = "Error";
                }
                return objResponse;
            }
        }
        public ResponseModel SaveSubModuleProperty(SubModulePropertiesCreateViewModel model)
        {
            ResponseModel objResponse = new ResponseModel();
            Module_Properties objModel = null;
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    if (model.Property_Id != 0)
                    {
                        objModel = ctx.Module_Properties.SingleOrDefault(p => p.Property_Id == model.Property_Id);
                        if (objModel != null)
                        {
                            objModel.Property_Name = model.Property_Name;
                            objModel.Sub_Module_Id = model.Sub_Module_Id;
                            objModel.ModifiedBy = 0;
                            objModel.ModifiedDate = DateTime.Now;
                            ctx.Entry(objModel).State = System.Data.Entity.EntityState.Modified;

                        }
                    }
                    else
                    {
                        objModel = new Module_Properties()
                        {
                            Property_Id = Convert.ToInt64(ctx.SP_TableIdGenerator("Property_Id").FirstOrDefault().ToString()),
                            CreatedBy = 0,
                            CreatedDate = DateTime.Now,
                            Property_Name = model.Property_Name,
                            Sub_Module_Id = model.Sub_Module_Id
                        };
                        ctx.Module_Properties.Add(objModel);
                    }
                    if (ctx.SaveChanges() > 0)
                    {
                        objResponse.Status = "Success";
                        objResponse.Response = model.Property_Id;
                    }
                    else
                    {
                        objResponse.Status = "Error";
                    }
                }
            }
            catch (Exception ex)
            {
                objResponse.Status = ex.ToString();
            }
            return objResponse;
        }
        public ResponseModel DeleteProperty(long id)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    Module_Properties objProperty = ctx.Module_Properties.SingleOrDefault(c => c.Property_Id == id);
                    if (objProperty != null)
                    {
                        objProperty.Deleted = true;
                        if (ctx.SaveChanges() > 0)
                        {
                            objResponse.Status = "Success";
                        }
                    }
                    else
                    {
                        objResponse.Status = "Error";
                    }
                }
            }
            catch (Exception)
            {
                objResponse.Status = "Error";
            }
            return objResponse;
        }
        #endregion

        #region Employees
        public ResponseModel GetEmployeesList()
        {
            ResponseModel objResponse = new ResponseModel();
            List<EmployeeViewModel> EmployeeList = null;
            try
            {
                using (var ctx = new NPMDBEntities())
                {

                    EmployeeList = (from e in ctx.EmployeeInformations
                                    join u in ctx.Users on e.UserId equals u.UserId
                                    join g in ctx.Genders on e.GenderId equals g.GenderId into eg
                                    from g in eg.DefaultIfEmpty()
                                    join ms in ctx.MaritalStatus on e.MaritalStatusId equals ms.MaritalStatusId into ems
                                    from ms in ems.DefaultIfEmpty()
                                    where (u.IsEmployee ?? false) == true && (u.IsDeleted ?? false) == false
                                    select new EmployeeViewModel()
                                    {
                                        Employee_Id = e.Employee_Id,
                                        Nic_Number = e.Nic_Number,
                                        Date_Of_Birth = e.Date_Of_Birth,
                                        Gender = g.Name,
                                        GenderId = e.GenderId,
                                        Marital_Status = ms.Name,
                                        MaritalStatusId = ms.MaritalStatusId,
                                        FirstName = u.FirstName,
                                        LastName = u.LastName,
                                        MiddleInitial = u.MiddleInitial
                                    }).ToList();
                }
                if (EmployeeList != null)
                {
                    objResponse.Status = "Success";
                    objResponse.Response = EmployeeList;
                }
                else
                {
                    objResponse.Status = "Error";
                }
            }
            catch (Exception ex)
            {
                objResponse.Status = ex + "";
            }
            return objResponse;
        }
        public ResponseModel GetEmployee(long id)
        {
            ResponseModel resp = new ResponseModel();
            EmployeeViewModel employee;
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    employee = (from e in ctx.EmployeeInformations
                                join u in ctx.Users on e.UserId equals u.UserId
                                join g in ctx.Genders on e.GenderId equals g.GenderId into eg
                                from g in eg.DefaultIfEmpty()
                                join ms in ctx.MaritalStatus on e.MaritalStatusId equals ms.MaritalStatusId into ems
                                from ms in ems.DefaultIfEmpty()
                                where (u.IsDeleted ?? false) == false && e.Employee_Id == id
                                select new EmployeeViewModel()
                                {
                                    FirstName = u.FirstName,
                                    Employee_Id = e.Employee_Id,
                                    LastName = u.LastName,
                                    MiddleInitial = u.MiddleInitial,
                                    Nic_Number = e.Nic_Number,
                                    Gender = ms.Name,
                                    GenderId = e.GenderId,
                                    MaritalStatusId = e.MaritalStatusId,
                                    Marital_Status = ms.Name,
                                    Date_Of_Birth = e.Date_Of_Birth,
                                    Email_Personal = e.Email_Personal,
                                    Personal_Contact_Number = e.Personal_Contact_Number,
                                    Home_Contact_Number = e.Home_Contact_Number,
                                    Emergency_Contact_Number = e.Emergency_Contact_Number,
                                    Permanent_Address = e.Permanent_Address,
                                    Permanent_Postal_Code = e.Permanent_Postal_Code,
                                    Permanent_City = e.Permanent_City,
                                    Permanent_State = e.Permanent_State
                                }).FirstOrDefault();
                    if (employee != null)
                    {
                        resp.Status = "Success";
                        resp.Response = employee;
                    }
                    else
                    {
                        resp.Status = "Error";
                    }
                }
            }
            catch (Exception ex)
            {
                resp.Status = ex.ToString();
            }
            return resp;
        }
        public ResponseModel SaveEmployee(EmployeeCreateViewModel model)
        {
            ResponseModel objResponse = new ResponseModel();
            EmployeeInformation objModel = null;
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    if (model.Employee_Id != 0)
                    {
                        objModel = ctx.EmployeeInformations.FirstOrDefault(e => e.Employee_Id == model.Employee_Id);
                        if (objModel != null)
                        {
                            objModel.Nic_Number = model.Nic_Number;
                            //if (getAge(model.Date_Of_Birth) > 17)
                            //{
                            objModel.Date_Of_Birth = model.Date_Of_Birth;
                            //}
                            //else
                            //{
                            //    objResponse.Status = "Age";
                            //    return objResponse;
                            //}    
                            objModel.GenderId = model.GenderId;
                            objModel.MaritalStatusId = model.MaritalStatusId;
                            objModel.Permanent_Address = model.Permanent_Address;
                            objModel.Permanent_City = model.Permanent_City;
                            objModel.Permanent_State = model.Permanent_State;
                            objModel.Permanent_Postal_Code = model.Permanent_Postal_Code;
                            objModel.Personal_Contact_Number = model.Personal_Contact_Number;
                            objModel.Home_Contact_Number = model.Home_Contact_Number;
                            objModel.Emergency_Contact_Number = model.Emergency_Contact_Number;
                            objModel.Email_Personal = model.Email_Personal;
                            ctx.Entry(objModel).State = System.Data.Entity.EntityState.Modified;
                            if (ctx.SaveChanges() > 0)
                            {
                                objResponse.Status = "Success";
                            }
                            else
                            {
                                objResponse.Status = "Error";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                objResponse.Status = ex.ToString();
            }
            return objResponse;
        }

        //private int getAge(DateTime? date_Of_Birth)
        //{
        //    DateTime today = DateTime.Today;
        //    DateTime dob = (DateTime)date_Of_Birth;
        //    int age = today.Year - dob.Year;
        //    if (date_Of_Birth > today.AddYears(-age))
        //        age--;
        //    return age;
        //}

        public ResponseModel GetListsOfSelectLists()
        {
            ResponseModel response = new ResponseModel();
            EmployeeSelectListsViewModel lists = new EmployeeSelectListsViewModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    lists.Genders = ctx.Genders.Where(g => (g.Deleted ?? false) == false).Select(t => new SelectListViewModel()
                    {
                        Id = t.GenderId,
                        Name = t.Name
                    }).ToList();
                    lists.MaritalStatus = ctx.MaritalStatus.Where(m => (m.Deleted ?? false) == false).Select(m => new SelectListViewModel()
                    {
                        Id = m.MaritalStatusId,
                        Name = m.Name
                    }).ToList();
                    if (lists != null)
                    {
                        response.Status = "Success";
                        response.Response = lists;
                    }
                    else
                    {
                        response.Status = "Error";
                    }
                }
            }
            catch (Exception ex)
            {
                response.Status = ex + "";
            }
            return response;
        }
        #endregion
    }
}