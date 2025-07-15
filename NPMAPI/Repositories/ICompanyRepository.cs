using System.Web.Http;
using NPMAPI.Models;

namespace NPMAPI.Repositories
{
    public interface ICompanyRepository
    {

        #region Company
        ResponseModel GetCompanyList();

        ResponseModel GetCompanyRecord(long CompanyId);

        [HttpPost]
        ResponseModel SaveCompany([FromBody] Company model);

        [HttpGet]
        ResponseModel DeleteCompany(long CompanyId);
        #endregion Company

        #region Office
        ResponseModel GetOfficeList(long? companyId);

        ResponseModel GetOfficeRecord(long OfficeId);

        [HttpPost]
        ResponseModel SaveOffice([FromBody] Office model);

        ResponseModel DeleteOffice(long OfficeId);
        #endregion Office

        #region Shift
        ResponseModel GetShiftList();

        ResponseModel GetShiftRecord(long ShiftId);

        [HttpPost]
        ResponseModel SaveShift([FromBody] Shift model);

        ResponseModel DeleteShift(long ShiftId);
        #endregion Shift

        #region
        ResponseModel GetTeamList(int? companyId, int? officeId, int? departmentId);

        ResponseModel GetTeamRecord(long TeamId);

        [HttpPost]
        ResponseModel SaveTeam([FromBody] TeamViewModel model);

        [HttpGet]
        ResponseModel DeleteTeam(long TeamId, int OfficeId, int DepartmentId, int CompanyId);

        ResponseModel GetCompanyOfficenDepartment(long CompanyId);
        #endregion Team

        #region Department
        ResponseModel GetOfficeIdFromDept(long DeptId);
        ResponseModel GetDepartmentList(long? officeId);
        ResponseModel GetDepartmentRecord(long DepartmentId);

        [HttpPost]
        ResponseModel SaveDepartment([FromBody] Department model);

        [HttpGet]
        ResponseModel DeleteDepartment(long DepartmentId);
        #endregion Department

        #region Designation
        ResponseModel GetDesignationList();

        ResponseModel GetDesignationRecord(long DesignationId);

        [HttpPost]
        ResponseModel SaveDesignation([FromBody] DesignationViewModel model);
        [HttpGet]
        ResponseModel DeleteDesignation(long DesignationId);
        ResponseModel GetCompaniesAndRolesSelectList();
        #endregion Designation

    }
}
