using System.Collections.Generic;
using System.Web.Http;
using NPMAPI.Models;

namespace NPMAPI.Repositories
{
    public interface IUserManagementSetup
    {

        #region Modules

        ResponseModel GetModulesList();
        ResponseModel GetModule(long id);
        ResponseModel SaveModule([FromBody] ModuleViewModel Model, long uId);
        ResponseModel DeleteModule(long id, long uId);
        ResponseModel GetModulesSelectList(string searchText);

        #endregion Modules

        #region SubModules
        ResponseModel GetSubModuleList(long? moduleId);
        ResponseModel GetSubModule(long id);
        ResponseModel DeleteSubModule(long id);
        ResponseModel SaveSubModule(SubModuleViewModel model);

        #endregion SubModules

        #region UserManagement
        ResponseModel DeleteUser(long id, long uId);
        ResponseModel SaveUser(UserCreateViewModel model, long uId);
        ResponseModel GetUsersList();
        ResponseModel GetUser(long id);
        ResponseModel ResetPassword(ResetPasswordViewModel model, long uId);
        ResponseModel ResetPasswordForUser(ResetPasswordForuserViewModel model, long uId);
        ResponseModel ChangeUserStatus(UserStatusChangeViewModel status, long uId);
        ResponseModel GetListsOfSelectLists();
        ResponseModel GetUsersSelectList();
        List<SP_GetUserAuthorization_Result> GetUserRoleAndRights(long userId);
        LoggedInUserViewModel GetUser(string username, string password);
        ResponseModel verifyEmail(string vEmail);
        //added by kamran get the id 
        ResponseModel empIdExists(long vEmpId);

        #endregion

        #region Roles

        ResponseModel GetRoleList();
        ResponseModel SaveRole(RoleViewModel model);
        ResponseModel GetRole(long id);
        ResponseModel DeleteRole(long id);
        ResponseModel GetTemplateByRoleId(long roleId);
        ResponseModel GetTemplateByUserId(long userId);
        ResponseModel SaveUserModuleProperties(UserModulePropertyCreateViewModel model);

        #endregion

        #region SubModuleProperties
        ResponseModel GetSubModulePropertiesList(long? subModuleId);
        ResponseModel GetSubModuleProperty(long id);
        ResponseModel GetSubModulesSelectList(string searchText);
        ResponseModel GetSubModulesSelectList(string searchText, long moduleId);
        ResponseModel SaveSubModuleProperty(SubModulePropertiesCreateViewModel model);
        ResponseModel DeleteProperty(long id);
        ResponseModel GetRolesSelectList();
        ResponseModel SaveRoleModuleProperties(RoleModulePropertyCreateViewModel model);

        #endregion

        #region Employee
        ResponseModel GetEmployee(long id);
        ResponseModel GetEmployeesList();
        ResponseModel SaveEmployee(EmployeeCreateViewModel model);
        #endregion
    }
}