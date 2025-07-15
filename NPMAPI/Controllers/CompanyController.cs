using System.Web.Http;
using NPMAPI.Models;
using NPMAPI.Repositories;

namespace NPMAPI.Controllers
{
    public class CompanyController : BaseController
    {
        private readonly ICompanyRepository _companyService;

        public CompanyController(ICompanyRepository companyService)
        {
            _companyService = companyService;
        }

        // GET: Company

        #region Company

        public ResponseModel GetCompanyList()
        {
            return _companyService.GetCompanyList();
        }
        public ResponseModel GetCompanyRecord(long CompanyId)
        {
            return _companyService.GetCompanyRecord(CompanyId);
        }

        [HttpPost]
        public ResponseModel SaveCompany([FromBody] Company model)
        {
            ResponseModel objResponse = new ResponseModel();
            if (!ModelState.IsValid)
            {
                objResponse.Status = "Error in Model";
                return objResponse;
            }

            return _companyService.SaveCompany(model);
        }

        [HttpGet]
        public ResponseModel DeleteCompany(long CompanyId)
        {
            return _companyService.DeleteCompany(CompanyId);
        }
        [HttpGet]
        public ResponseModel GetCompaniesAndRolesSelectList()
        {
            return _companyService.GetCompaniesAndRolesSelectList();
        }
        #endregion Company

        #region Office

        public ResponseModel GetOfficeList(long? companyId)
        {
            return _companyService.GetOfficeList(companyId);
        }
        public ResponseModel GetOfficeRecord(long OfficeId)
        {
            return _companyService.GetOfficeRecord(OfficeId);
        }

        [HttpPost]
        public ResponseModel SaveOffice([FromBody] Office model)
        {
            ResponseModel objResponse = new ResponseModel();
            if (!ModelState.IsValid)
            {
                objResponse.Status = "Error in Model";
                return objResponse;
            }

            return _companyService.SaveOffice(model);
        }

        [HttpGet]
        public ResponseModel DeleteOffice(long OfficeId)
        {
            return _companyService.DeleteOffice(OfficeId);
        }

        #endregion Office

        #region Shift

        public ResponseModel GetShiftList()
        {
            return _companyService.GetShiftList();
        }
        public ResponseModel GetShiftRecord(long ShiftId)
        {
            return _companyService.GetShiftRecord(ShiftId);
        }

        [HttpPost]
        public ResponseModel SaveShift([FromBody] Shift model)
        {
            ResponseModel objResponse = new ResponseModel();
            if (!ModelState.IsValid)
            {
                objResponse.Status = "Error in Model";
                return objResponse;
            }

            return _companyService.SaveShift(model);
        }

        [HttpGet]
        public ResponseModel DeleteShift(long shiftId)
        {
            return _companyService.DeleteShift(shiftId);
        }

        #endregion Shift

        #region Team
        //public ResponseModel GetAllTeamsList()
        //{
        //    return _companyService.GetAllTeamsList();
        //}
        public ResponseModel GetTeamList(int? companyId, int? officeId, int? departmentId)
        {
            return _companyService.GetTeamList(companyId, officeId, departmentId);
        }
        public ResponseModel GetTeamRecord(long TeamId)
        {
            return _companyService.GetTeamRecord(TeamId);
        }

        [HttpPost]
        public ResponseModel SaveTeam([FromBody] TeamViewModel model)
        {
            ResponseModel objResponse = new ResponseModel();
            if (!ModelState.IsValid)
            {
                objResponse.Status = "Error in Model";
                return objResponse;
            }

            return _companyService.SaveTeam(model);
        }

        [HttpGet]
        public ResponseModel DeleteTeam(long TeamId, int OfficeId, int DepartmentId, int CompanyId)
        {
            return _companyService.DeleteTeam(TeamId, OfficeId, DepartmentId, CompanyId);
        }


        public ResponseModel GetCompanyOfficenDepartment(long CompanyId)
        {
            return _companyService.GetCompanyOfficenDepartment(CompanyId);
        }

        #endregion Team

        #region Department
        public ResponseModel GetOfficeIdFromDept(long DeptId)
        {
            return _companyService.GetOfficeIdFromDept(DeptId);
        }
        public ResponseModel GetDepartmentList(long? officeId)
        {
            return _companyService.GetDepartmentList(officeId);
        }
        public ResponseModel GetDepartmentRecord(long DepartmentId)
        {
            return _companyService.GetDepartmentRecord(DepartmentId);
        }

        [HttpPost]
        public ResponseModel SaveDepartment([FromBody] Department model)
        {
            ResponseModel objResponse = new ResponseModel();
            if (!ModelState.IsValid)
            {
                objResponse.Status = "Error in Model";
                return objResponse;
            }

            return _companyService.SaveDepartment(model);
        }

        [HttpGet]
        public ResponseModel DeleteDepartment(long DepartmentId)
        {
            return _companyService.DeleteDepartment(DepartmentId);
        }

        #endregion Department

        #region Designation

        public ResponseModel GetDesignationList()
        {
            return _companyService.GetDesignationList();
        }
        public ResponseModel GetDesignationRecord(long DesignationId)
        {
            return _companyService.GetDesignationRecord(DesignationId);
        }

        [HttpPost]
        public ResponseModel SaveDesignation([FromBody] DesignationViewModel model)
        {
            ResponseModel objResponse = new ResponseModel();
            if (!ModelState.IsValid)
            {
                objResponse.Status = "Error in Model";
                return objResponse;
            }

            return _companyService.SaveDesignation(model);
        }

        [HttpGet]
        public ResponseModel DeleteDesignation(long DesignationId)
        {
            return _companyService.DeleteDesignation(DesignationId);
        }

        #endregion Designation

    }
}