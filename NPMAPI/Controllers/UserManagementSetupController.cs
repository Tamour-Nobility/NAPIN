using System.Linq;
using System.Web.Http;
using NPMAPI.Models;
using NPMAPI.Repositories;

namespace NPMAPI.Controllers
{
    public class UserManagementSetupController : BaseController
    {
        private readonly IUserManagementSetup _UserManagementService;
        private readonly IEncryption _EncryptionService;
        public UserManagementSetupController(IUserManagementSetup userManagementService, IEncryption userEncryptionService)
        {
            _UserManagementService = userManagementService;
            _EncryptionService = userEncryptionService;

        }

        #region Modules
        public ResponseModel GetModulesList()
        {
            return _UserManagementService.GetModulesList();
        }
        [HttpGet]
        public ResponseModel GetModule(long id)
        {
            return _UserManagementService.GetModule(id);
        }
        [HttpGet]
        public ResponseModel DeleteModule(long id)
        {
            return _UserManagementService.DeleteModule(id, GetUserId());
        }
        [HttpPost]
        public ResponseModel SaveModule([FromBody] ModuleViewModel Model)
        {
            ResponseModel objResponse = new ResponseModel();
            if (!ModelState.IsValid)
            {
                objResponse.Status = string.Join("; ", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage));
                return objResponse;
            }

            return _UserManagementService.SaveModule(Model, GetUserId());
        }
        #endregion Modules

        #region UserManagement

        [HttpGet]
        [ActionName("GetUsersList")]
        public ResponseModel GetUsersList()
        {
            return _UserManagementService.GetUsersList();
        }
        [HttpGet]

        public ResponseModel verifyEmail(string vEmail)
        {
            return _UserManagementService.verifyEmail(vEmail);
        }

        // added by kamran for id exist
        [HttpGet]

        public ResponseModel empIdExists(long vEmpId)
        {
            return _UserManagementService.empIdExists(vEmpId);
        }

        [HttpPost]
        [ActionName("SaveUser")]
        public ResponseModel SaveUser([FromBody] UserCreateViewModel model)
        {
            ResponseModel resp = new ResponseModel();
            if (model.UserId != 0)
            {
                ModelState["model.Password"].Errors.Clear();
                ModelState["model.ConfirmPassword"].Errors.Clear();
            }
            else
            {
                model.Password = _EncryptionService.HashPassword(model.Password);
                if (_UserManagementService.verifyEmail(model.Email).Response)
                {
                    //ModelState["model.Email"].Errors.Add("Email already taken");
                    ModelState.AddModelError("Make", "Email alrady taken");
                }

            }
            if (!ModelState.IsValid)
            {
                resp.Status = string.Join(";", ModelState.Values.SelectMany(e => e.Errors).Select(e => e.ErrorMessage));
                return resp;
            }
            return _UserManagementService.SaveUser(model, GetUserId());
        }
        [HttpGet]
        [ActionName("GetUser")]
        public ResponseModel GetUser(long Id)
        {
            return _UserManagementService.GetUser(Id);
        }
        [HttpPost]
        [ActionName("ResetPassword")]
        public ResponseModel ResetPassword([FromBody] ResetPasswordViewModel model)
        {
            ResponseModel res = new ResponseModel();
            if (!ModelState.IsValid)
            {
                res.Status = string.Join(";", ModelState.Values.SelectMany(e => e.Errors).Select(e => e.ErrorMessage));
                return res;
            }
            else
            {
                model.Password = _EncryptionService.HashPassword(model.Password);
                return _UserManagementService.ResetPassword(model, GetUserId());
            }

        }
        [HttpPost]
        [ActionName("ResetPasswordByUser")]
        public ResponseModel ResetPasswordByUser([FromBody] ResetPasswordForuserViewModel model)
        {
            ResponseModel res = new ResponseModel();
            if (!ModelState.IsValid)
            {
                res.Status = string.Join(";", ModelState.Values.SelectMany(e => e.Errors).Select(e => e.ErrorMessage));
                return res;
            }
            else
            {
                var User = _EncryptionService.VerifyUser(model.UserName, model.OldPassword);
                if (User != null)
                {
                    model.Password = _EncryptionService.HashPassword(model.Password);
                    return _UserManagementService.ResetPasswordForUser(model, GetUserId());
                }
                else
                {
                    res.Status = "You have entered an invalid password";
                    return res;
                }
              

                
            }

        }
        [HttpGet]
        public ResponseModel DeleteUser(long id)
        {
            return _UserManagementService.DeleteUser(id, GetUserId());
        }
        [HttpPost]
        public ResponseModel ChangeUserStatus([FromBody] UserStatusChangeViewModel model)
        {
            ResponseModel res = new ResponseModel();
            if (!ModelState.IsValid)
            {
                res.Status = string.Join(";", ModelState.Values.SelectMany(e => e.Errors).Select(e => e.ErrorMessage));
                return res;
            }
            return _UserManagementService.ChangeUserStatus(model, GetUserId());
        }

        [HttpGet]
        public ResponseModel GetUsersSelectList()
        {
            return _UserManagementService.GetUsersSelectList();
        }
        #endregion

        #region Roles
        [HttpGet]
        [ActionName("GetRoleList")]
        public ResponseModel GetRoleList()
        {
            return _UserManagementService.GetRoleList();
        }
        [HttpPost]
        [ActionName("SaveRole")]
        public ResponseModel SaveRole([FromBody] RoleViewModel model)
        {
            ResponseModel objResponse = new ResponseModel();
            if (!ModelState.IsValid)
            {
                objResponse.Status = string.Join("; ", ModelState.Values.SelectMany(e => e.Errors).Select(e => e.ErrorMessage));
                return objResponse;
            }
            return _UserManagementService.SaveRole(model);
        }
        [HttpGet]
        [ActionName("GetRole")]
        public ResponseModel GetRole(long id)
        {
            return _UserManagementService.GetRole(id);
        }
        [HttpGet]
        [ActionName("DeleteRole")]
        public ResponseModel DeleteRole(long id)
        {
            return _UserManagementService.DeleteRole(id);
        }
        [HttpGet]
        [ActionName("GetRolesSelectList")]
        public ResponseModel GetRolesSelectList()
        {
            return _UserManagementService.GetRolesSelectList();
        }
        [HttpGet]
        [ActionName("GetTemplateByRoleId")]
        public ResponseModel GetTemplateByRoleId(long roleId)
        {
            return _UserManagementService.GetTemplateByRoleId(roleId);
        }
        [HttpGet]
        [ActionName("GetTemplateByUserId")]
        public ResponseModel GetTemplateByUserId(long userId)
        {
            return _UserManagementService.GetTemplateByUserId(userId);
        }
        [HttpPost]
        [ActionName("SaveRoleModuleProperties")]
        public ResponseModel SaveRoleModuleProperties([FromBody] RoleModulePropertyCreateViewModel model)
        {
            ResponseModel responseModel = new ResponseModel();
            if (!ModelState.IsValid)
            {
                responseModel.Status = string.Join(";", ModelState.Values.SelectMany(t => t.Errors).Select(e => e.ErrorMessage));
                return responseModel;
            }
            return _UserManagementService.SaveRoleModuleProperties(model);
        }
        [HttpPost]
        [ActionName("SaveUserModuleProperties")]
        public ResponseModel SaveUserModuleProperties([FromBody] UserModulePropertyCreateViewModel model)
        {
            ResponseModel responseModel = new ResponseModel();
            if (!ModelState.IsValid)
            {
                responseModel.Status = string.Join(";", ModelState.Values.SelectMany(t => t.Errors).Select(e => e.ErrorMessage));
                return responseModel;
            }
            return _UserManagementService.SaveUserModuleProperties(model);
        }
        #endregion

        #region SubModule
        [HttpGet]
        public ResponseModel GetSubModuleList(long? moduleId)
        {
            return _UserManagementService.GetSubModuleList(moduleId);
        }
        [HttpGet]
        public ResponseModel GetModulesSelectList(string searchText)
        {
            return _UserManagementService.GetModulesSelectList(searchText);
        }
        [HttpGet]
        public ResponseModel GetSubModule(long id)
        {
            return _UserManagementService.GetSubModule(id);
        }
        [HttpPost]
        public ResponseModel SaveSubModule([FromBody] SubModuleViewModel model)
        {
            ResponseModel objResponse = new ResponseModel();
            if (!ModelState.IsValid)
            {
                objResponse.Status = string.Join("; ", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage));
                return objResponse;
            }
            return _UserManagementService.SaveSubModule(model);
        }
        [HttpGet]
        [ActionName("DeleteSubModule")]
        public ResponseModel DeleteSubModule(long id)
        {
            return _UserManagementService.DeleteSubModule(id);
        }
        #endregion

        #region SubModuleProperties
        [HttpGet]
        public ResponseModel GetSubModulePropertiesList(long? subModuleId)
        {
            return _UserManagementService.GetSubModulePropertiesList(subModuleId);
        }
        [HttpGet]
        public ResponseModel GetSubModuleProperty(long id)
        {
            return _UserManagementService.GetSubModuleProperty(id);
        }
        [HttpGet]
        public ResponseModel GetSubModulesSelectList(string searchText)
        {
            return _UserManagementService.GetSubModulesSelectList(searchText);
        }
        [HttpGet]
        public ResponseModel GetSubModulesSelectList(string searchText, long moduleId)
        {
            return _UserManagementService.GetSubModulesSelectList(searchText, moduleId);
        }
        [HttpPost]
        public ResponseModel SaveSubModuleProperty([FromBody] SubModulePropertiesCreateViewModel model)
        {
            ResponseModel resModel = new ResponseModel();
            if (!ModelState.IsValid)
            {
                resModel.Status = string.Join(";", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage));
                return resModel;
            }
            return _UserManagementService.SaveSubModuleProperty(model);
        }
        [HttpGet]
        public ResponseModel DeleteProperty(long id)
        {
            return _UserManagementService.DeleteProperty(id);
        }

        #endregion SubModuleProperties

        #region Emoloyees

        [HttpGet]
        [ActionName("GetEmployeesList")]
        public ResponseModel GetEmployeesList()
        {
            return _UserManagementService.GetEmployeesList();
        }
        [HttpPost]
        [ActionName("SaveEmployee")]
        public ResponseModel SaveEmployee([FromBody] EmployeeCreateViewModel model)
        {
            ResponseModel resp = new ResponseModel();
            if (!ModelState.IsValid)
            {
                resp.Status = string.Join(";", ModelState.Values.SelectMany(e => e.Errors).Select(e => e.ErrorMessage));
                return resp;
            }
            return _UserManagementService.SaveEmployee(model);
        }
        [HttpGet]
        [ActionName("GetEmployee")]
        public ResponseModel GetEmployee(long Id)
        {
            return _UserManagementService.GetEmployee(Id);
        }
        [HttpGet]
        [ActionName("GetListsOfSelectLists")]
        public ResponseModel GetListsOfSelectLists()
        {
            return _UserManagementService.GetListsOfSelectLists();
        }
        #endregion
    }
}