using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using NPMAPI.Models;
using NPMAPI.Repositories;

namespace NPMAPI.Services
{
    public class CompanyService : ICompanyRepository
    {
        #region Company
        public ResponseModel GetCompanyList()
        {
            ResponseModel objResponse = new ResponseModel();
            List<Company> objCompanyList = null;
            using (var ctx = new NPMDBEntities())
            {
                objCompanyList = ctx.Companies.Where(c => c.IsActive == true).ToList();
            }

            if (objCompanyList != null)
            {
                objResponse.Status = "Sucess";
                objResponse.Response = objCompanyList;
            }
            else
            {
                objResponse.Status = "Error";
            }
            return objResponse;
        }

        public ResponseModel GetCompanyRecord(long CompanyId)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                Company _company = null;
                using (var ctx = new NPMDBEntities())
                {
                    _company = ctx.Companies.SingleOrDefault(c => c.CompanyId == CompanyId);
                }

                if (_company != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = _company;
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
        public ResponseModel SaveCompany([FromBody] Company model)
        {
            ResponseModel objResponse = new ResponseModel();
            Company objModel = null;

            using (var ctx = new NPMDBEntities())
            {
                if (model.CompanyId != 0)
                {
                    objModel = ctx.Companies.SingleOrDefault(p => p.CompanyId == model.CompanyId);
                    if (objModel != null)
                    {
                        objModel.CompanyName = model.CompanyName;
                        ctx.SaveChanges();
                    }
                }
                else
                {
                    objModel = new Company();
                    int companyId = Convert.ToInt32(ctx.SP_TableIdGenerator("CompanyId").FirstOrDefault().ToString());//ctx.Companies.Max(p => p.CompanyId);

                    model.CompanyId = companyId;
                    model.IsActive = true;

                    ctx.Companies.Add(model);
                    ctx.SaveChanges();
                }

                if (model != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = model.CompanyId;
                }
                else
                {
                    objResponse.Status = "Error";
                }
            }

            return objResponse;
        }

        [HttpGet]
        public ResponseModel DeleteCompany(long CompanyId)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                List<Company> CompaniesList = null;
                using (var ctx = new NPMDBEntities())
                {

                    Company objCompany = ctx.Companies.SingleOrDefault(c => c.CompanyId == CompanyId);
                    if (objCompany != null)
                    {
                        objCompany.IsActive = false;
                        ctx.SaveChanges();
                    }

                    CompaniesList = ctx.Companies.Where(c => c.IsActive == null || c.IsActive == true).ToList();
                    if (CompaniesList != null)
                    {
                        objResponse.Status = "Sucess";
                        objResponse.Response = CompaniesList;
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
        public ResponseModel GetCompaniesAndRolesSelectList()
        {
            ResponseModel objResponse = new ResponseModel();
            UserSelectListsViewModel selectListsViewModel = new UserSelectListsViewModel();
            using (var ctx = new NPMDBEntities())
            {

                selectListsViewModel.Companies = ctx.Companies.Where(t => t.IsActive).Select(x => new SelectListViewModel()
                {
                    Id = x.CompanyId,
                    Name = x.CompanyName
                }).OrderBy(t => t.Name).ToList();

                selectListsViewModel.Roles = ctx.Roles.Where(r => (r.Deleted ?? false) == false).Select(x => new SelectListViewModel()
                {
                    Id = x.RoleId,
                    Name = x.RoleName
                }).OrderBy(t => t.Name).ToList();
                PracticeService cService = new PracticeService();
                ResponseModel rM = cService.GetPracticeSelectList();
                if (rM.Status == "Success")
                {
                    selectListsViewModel.Practices = cService.GetPracticeSelectList().Response;
                }
                if (selectListsViewModel != null)
                {
                    objResponse.Response = selectListsViewModel;
                    objResponse.Status = "Sucess";
                }
                else
                {
                    objResponse.Status = "Error";
                }
                return objResponse;
            }
        }
        #endregion Company

        #region Office
        public ResponseModel GetOfficeList(long? companyId)
        {
            ResponseModel objResponse = new ResponseModel();
            List<Office> objOfficeList = null;
            using (var ctx = new NPMDBEntities())
            {
                if (companyId == null || companyId == 0)
                    objOfficeList = ctx.Offices.Where(o => o.DELETED == null || o.DELETED == false).ToList();
                else
                    //objOfficeList = (from o in ctx.Offices
                    //                 join c in ctx.Companies on o.CompanyId equals c.CompanyId
                    //                 where o.DELETED == false && o.CompanyId == companyId
                    //                 select new Office()
                    //                 {
                    //                     OfficeId = o.OfficeId,
                    //                     CompanyId = o.CompanyId,
                    //                     ComapnyName = c.CompanyName,
                    //                     OfficeName = o.OfficeName,
                    //                     Address = o.Address,
                    //                     PhoneNo = o.PhoneNo,
                    //                     DELETED = o.DELETED
                    //                 }).ToList();
                    objOfficeList = ctx.Offices.Where(o => (o.DELETED == null || o.DELETED == false) && (o.CompanyId == companyId)).ToList();
            }

            if (objOfficeList != null)
            {
                objResponse.Status = "Sucess";
                objResponse.Response = objOfficeList;
            }
            else
            {
                objResponse.Status = "Error";
            }
            return objResponse;
        }

        public ResponseModel GetOfficeRecord(long OfficeId)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                Office _office = null;
                using (var ctx = new NPMDBEntities())
                {
                    _office = ctx.Offices.Where(o => o.DELETED == null || o.DELETED == false).SingleOrDefault(c => c.OfficeId == OfficeId);
                }

                if (_office != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = _office;
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

        public ResponseModel SaveOffice([FromBody] Office model)
        {
            ResponseModel objResponse = new ResponseModel();
            Office objModel = null;

            using (var ctx = new NPMDBEntities())
            {
                if (model.OfficeId != 0)
                {
                    objModel = ctx.Offices.FirstOrDefault(p => p.OfficeId == model.OfficeId);
                    if (objModel != null)
                    {
                        objModel.OfficeName = model.OfficeName;
                        objModel.Address = model.Address;
                        objModel.PhoneNo = model.PhoneNo;
                        //objModel.CompanyId = model.CompanyId;
                        ctx.SaveChanges();
                    }
                }
                else
                {
                    objModel = new Office();
                    int officeId = Convert.ToInt32(ctx.SP_TableIdGenerator("OfficeId").FirstOrDefault().ToString());//ctx.Offices.Max(p => p.OfficeId);
                    model.OfficeId = officeId;
                    model.OfficeName = model.OfficeName;
                    model.Address = model.Address;
                    model.PhoneNo = model.PhoneNo;
                    model.CompanyId = model.CompanyId;
                    ctx.Offices.Add(model);
                    ctx.SaveChanges();
                }

                if (model != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = model.OfficeId;
                }
                else
                {
                    objResponse.Status = "Error";
                }
            }
            return objResponse;
        }

        [HttpGet]
        public ResponseModel DeleteOffice(long OfficeId)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                List<Office> objOfficeList = null;
                using (var ctx = new NPMDBEntities())
                {
                    Office objShift = ctx.Offices.FirstOrDefault(c => c.OfficeId == OfficeId);
                    if (objShift != null)
                    {
                        objShift.DELETED = true;
                        ctx.SaveChanges();
                    }

                    objOfficeList = ctx.Offices.Where(o => o.DELETED == null || o.DELETED == false).ToList();
                    if (objOfficeList != null)
                    {
                        objResponse.Status = "Sucess";
                        objResponse.Response = objOfficeList;
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

        #endregion Office

        #region Shift
        public ResponseModel GetShiftList()
        {
            ResponseModel objResponse = new ResponseModel();
            List<Shift> objShiftList = null;
            using (var ctx = new NPMDBEntities())
            {
                objShiftList = ctx.Shifts.Where(s => s.DELETED != true).ToList();
            }

            if (objShiftList != null)
            {
                objResponse.Status = "Sucess";
                objResponse.Response = objShiftList;
            }
            else
            {
                objResponse.Status = "Error";
            }
            return objResponse;
        }

        public ResponseModel GetShiftRecord(long ShiftId)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                Shift objModel = null;
                using (var ctx = new NPMDBEntities())
                {
                    objModel = ctx.Shifts.SingleOrDefault(c => c.ShiftId == ShiftId);
                }

                if (objModel != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = objModel;
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

        public ResponseModel SaveShift([FromBody] Shift model)
        {
            ResponseModel objResponse = new ResponseModel();
            Shift objModel = null;

            using (var ctx = new NPMDBEntities())
            {
                if (model.ShiftId != 0)
                {
                    objModel = ctx.Shifts.SingleOrDefault(p => p.ShiftId == model.ShiftId);
                    if (objModel != null)
                    {
                        objModel.ShiftName = model.ShiftName;
                        ctx.SaveChanges();
                    }
                }
                else
                {
                    objModel = new Shift();
                    int shiftId = Convert.ToInt32(ctx.SP_TableIdGenerator("ShiftId").FirstOrDefault().ToString());// ctx.Shifts.Max(p => p.ShiftId);
                    model.ShiftId = shiftId;
                    objModel.ShiftName = model.ShiftName;
                    ctx.Shifts.Add(model);
                    ctx.SaveChanges();
                }
                if (model != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = model.ShiftId;
                }
                else
                {
                    objResponse.Status = "Error";
                }
            }
            return objResponse;
        }

        [HttpGet]
        public ResponseModel DeleteShift(long ShiftId)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                List<Shift> ShiftList = null;
                using (var ctx = new NPMDBEntities())
                {

                    Shift objShift = ctx.Shifts.SingleOrDefault(c => c.ShiftId == ShiftId);
                    if (objShift != null)
                    {
                        objShift.DELETED = true;
                        ctx.SaveChanges();
                    }

                    ShiftList = ctx.Shifts.Where(c => c.DELETED != true).ToList();
                    if (ShiftList != null)
                    {
                        objResponse.Status = "Sucess";
                        objResponse.Response = ShiftList;
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

        #endregion Shift

        #region Team
        public ResponseModel GetTeamList(int? companyId, int? officeId, int? departmentId)
        {
            ResponseModel objResponse = new ResponseModel();
            List<TeamViewModel> objTeamList = new List<TeamViewModel>();
            using (var ctx = new NPMDBEntities())
            {
                if (companyId.HasValue || officeId.HasValue || departmentId.HasValue)
                {
                    var model = ctx.GetTeamsByOffice(officeId, departmentId, companyId).ToList();
                    foreach (var item in model)
                    {
                        TeamViewModel tvModel = new TeamViewModel();
                        tvModel.Company = item.Company;
                        tvModel.CompanyName = item.CompanyName;
                        tvModel.CreatedBy = (item.CreatedBy != null ? (long)item.CreatedBy : 0);
                        tvModel.CreatedDate = (item.CreatedDate != null ? (DateTimeOffset)item.CreatedDate : DateTime.Now);
                        tvModel.Deleted = (item.Deleted != null ? (bool)item.Deleted : false);
                        tvModel.DepartmentId = item.DepartmentId;
                        tvModel.DepartmentName = item.DepartmentName;
                        tvModel.ModifiedBy = (item.ModifiedBy != null ? (long)item.ModifiedBy : 0);
                        tvModel.ModifiedDate = (item.ModifiedDate != null ? (DateTimeOffset)item.ModifiedDate : DateTime.Now);
                        tvModel.OfficeId = item.OfficeId;
                        tvModel.OfficeName = item.OfficeName;
                        tvModel.TeamBackupLead = item.TeamBackupLead;
                        tvModel.TeamId = item.TeamId;
                        tvModel.TeamLead = item.TeamLead;
                        tvModel.TeamName = item.TeamName;
                        tvModel.TeamShift = item.TeamShift;
                        tvModel.TeamSupervisor = item.TeamSupervisor;
                        tvModel.TeamType = item.TeamType;

                        objTeamList.Add(tvModel);
                    }
                }
                else
                {
                    var companies = ctx.Companies.Where(c => c.IsActive == true).ToList();
                    var offices = ctx.Offices.Where(o => o.DELETED != true).ToList();
                    var departments = ctx.Departments.Where(d => d.Deleted != true).ToList();
                    var teams = ctx.Teams.Where(t => t.Deleted != true).ToList();
                    if (teams.Count > 0)
                    {
                        foreach (var team in teams)
                        {
                            var comp = companies.Where(c => c.CompanyId == team.Company).ToList();
                            string compName = comp.Count > 0 ? comp.FirstOrDefault().CompanyName : "";
                            var office = offices.Where(o => o.OfficeId == team.OfficeId).ToList();
                            string offName = office.Count > 0 ? office.FirstOrDefault().OfficeName : "";
                            var department = departments.Where(d => d.DepartmentId == team.DepartmentId).ToList();
                            string deptName = department.Count > 0 ? department.FirstOrDefault().DepartmentName : "";
                            TeamViewModel tvModel = new TeamViewModel
                            {
                                Company = team.Company,
                                CompanyName = compName,
                                CreatedBy = (team.CreatedBy != null ? (long)team.CreatedBy : 0),
                                CreatedDate = (team.CreatedDate != null ? (DateTimeOffset)team.CreatedDate : DateTime.Now),
                                Deleted = (team.Deleted != null ? (bool)team.Deleted : false),
                                DepartmentId = team.DepartmentId,
                                DepartmentName = deptName,
                                ModifiedBy = (team.ModifiedBy != null ? (long)team.ModifiedBy : 0),
                                ModifiedDate = (team.ModifiedDate != null ? (DateTimeOffset)team.ModifiedDate : DateTime.Now),
                                OfficeId = team.OfficeId,
                                OfficeName = offName,
                                TeamBackupLead = team.TeamBackupLead,
                                TeamId = team.TeamId,
                                TeamLead = team.TeamLead,
                                TeamName = team.TeamName,
                                TeamShift = team.TeamShift,
                                TeamSupervisor = team.TeamSupervisor,
                                TeamType = team.TeamType
                            };
                            objTeamList.Add(tvModel);
                        }
                        objResponse.Status = "Succes";
                        objResponse.Response = objTeamList;
                    }
                }
            }

            if (objTeamList != null)
            {
                objResponse.Status = "Sucess";
                objResponse.Response = objTeamList;
            }
            else
            {
                objResponse.Status = "Error";
            }
            return objResponse;
        }


        public ResponseModel GetTeamRecord(long TeamId)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                TeamViewModel tvModel = null;
                using (var ctx = new NPMDBEntities())
                {
                    var item = ctx.Teams.SingleOrDefault(c => c.TeamId == TeamId && (c.Deleted == null || c.Deleted == false));
                    if (item != null)
                    {
                        tvModel.Company = item.Company;
                        tvModel.CompanyName = ctx.Companies.FirstOrDefault(c => c.CompanyId == item.Company).CompanyName;
                        tvModel.CreatedBy = (item.CreatedBy != null ? (long)item.CreatedBy : 0);
                        tvModel.CreatedDate = (item.CreatedDate != null ? (DateTimeOffset)item.CreatedDate : DateTime.Now);
                        tvModel.Deleted = (item.Deleted != null ? (bool)item.Deleted : false);
                        tvModel.DepartmentId = item.DepartmentId;
                        tvModel.DepartmentName = ctx.Departments.FirstOrDefault(c => c.DepartmentId == item.DepartmentId).DepartmentName;
                        tvModel.ModifiedBy = (item.ModifiedBy != null ? (long)item.ModifiedBy : 0);
                        tvModel.ModifiedDate = (item.ModifiedDate != null ? (DateTimeOffset)item.ModifiedDate : DateTime.Now);
                        tvModel.OfficeId = item.OfficeId;
                        tvModel.OfficeName = ctx.Offices.FirstOrDefault(c => c.OfficeId == item.OfficeId).OfficeName;
                        tvModel.TeamBackupLead = item.TeamBackupLead;
                        tvModel.TeamId = item.TeamId;
                        tvModel.TeamLead = item.TeamLead;
                        tvModel.TeamName = item.TeamName;
                        tvModel.TeamShift = item.TeamShift;
                        tvModel.TeamSupervisor = item.TeamSupervisor;
                        tvModel.TeamType = item.TeamType;
                    }
                }

                if (tvModel != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = tvModel;
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

        public ResponseModel SaveTeam([FromBody] TeamViewModel model)
        {
            ResponseModel objResponse = new ResponseModel();
            Team objModel = null;

            using (var ctx = new NPMDBEntities())
            {
                if (model.TeamId != 0)
                {
                    objModel = ctx.Teams.SingleOrDefault(p => p.TeamId == model.TeamId);
                    if (objModel != null)
                    {
                        objModel.Company = model.Company;
                        objModel.OfficeId = model.OfficeId;
                        objModel.DepartmentId = model.DepartmentId;
                        objModel.TeamLead = model.TeamLead;
                        objModel.TeamName = model.TeamName;
                        objModel.TeamBackupLead = model.TeamBackupLead;
                        objModel.TeamShift = model.TeamShift;
                        objModel.TeamSupervisor = model.TeamSupervisor;
                        objModel.TeamType = model.TeamType;
                        objModel.ModifiedDate = DateTime.Now;
                        ctx.SaveChanges();
                    }
                }
                else
                {
                    objModel = new Team();
                    long teamId = Convert.ToInt64(ctx.SP_TableIdGenerator("TeamId").FirstOrDefault().ToString());//ctx.Teams.Max(p => p.TeamId);

                    objModel.TeamId = teamId;
                    objModel.Deleted = false;

                    objModel.Company = model.Company;
                    objModel.OfficeId = model.OfficeId;
                    objModel.DepartmentId = model.DepartmentId;
                    objModel.TeamLead = model.TeamLead;
                    objModel.TeamName = model.TeamName;
                    objModel.TeamShift = model.TeamShift;
                    objModel.TeamBackupLead = model.TeamBackupLead;
                    objModel.TeamSupervisor = model.TeamSupervisor;
                    objModel.TeamType = model.TeamType;
                    objModel.OfficeId = model.OfficeId;
                    objModel.ModifiedDate = DateTime.Now;
                    objModel.CreatedDate = DateTime.Now;
                    ctx.Teams.Add(objModel);
                    ctx.SaveChanges();
                }

                if (model != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = model.TeamId;
                }
                else
                {
                    objResponse.Status = "Error";
                }
            }

            return objResponse;
        }

        [HttpGet]
        public ResponseModel DeleteTeam(long TeamId, int OfficeId, int DepartmentId, int CompanyId)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                //List<Team> TeamList = null;
                using (var ctx = new NPMDBEntities())
                {

                    Team objCompany = ctx.Teams.SingleOrDefault(c => c.TeamId == TeamId);
                    if (objCompany != null)
                    {
                        objCompany.Deleted = true;
                        ctx.SaveChanges();
                    }

                    List<TeamViewModel> objTeamList = new List<TeamViewModel>();

                    //  objTeamList = ctx.Teams.Where(t => t.OfficeId == OfficeId && (t.Deleted == null || t.Deleted == false)).ToList();
                    var model = ctx.GetTeamsByOffice(OfficeId, DepartmentId, CompanyId).ToList();
                    foreach (var item in model)
                    {
                        TeamViewModel tvModel = new TeamViewModel();
                        tvModel.Company = item.Company;
                        tvModel.CompanyName = item.CompanyName;
                        tvModel.CreatedBy = (item.CreatedBy != null ? (long)item.CreatedBy : 0);
                        tvModel.CreatedDate = (item.CreatedDate != null ? (DateTimeOffset)item.CreatedDate : DateTime.Now);
                        tvModel.Deleted = (item.Deleted != null ? (bool)item.Deleted : false);
                        tvModel.DepartmentId = item.DepartmentId;
                        tvModel.DepartmentName = item.DepartmentName;
                        tvModel.ModifiedBy = (item.ModifiedBy != null ? (long)item.ModifiedBy : 0);
                        tvModel.ModifiedDate = (item.ModifiedDate != null ? (DateTimeOffset)item.ModifiedDate : DateTime.Now);
                        tvModel.OfficeId = item.OfficeId;
                        tvModel.OfficeName = item.OfficeName;
                        tvModel.TeamBackupLead = item.TeamBackupLead;
                        tvModel.TeamId = item.TeamId;
                        tvModel.TeamLead = item.TeamLead;
                        tvModel.TeamName = item.TeamName;
                        tvModel.TeamShift = item.TeamShift;
                        tvModel.TeamSupervisor = item.TeamSupervisor;
                        tvModel.TeamType = item.TeamType;

                        objTeamList.Add(tvModel);

                    }

                    if (objTeamList != null)
                    {
                        objResponse.Status = "Sucess";
                        objResponse.Response = objTeamList;
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

        public ResponseModel GetCompanyOfficenDepartment(long CompanyId)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                TeamDropdownViewModel tvModel = new TeamDropdownViewModel();
                using (var ctx = new NPMDBEntities())
                {
                    tvModel.DepartmentList = ctx.Departments.Where(d => d.CompanyId == CompanyId && (d.Deleted == null || d.Deleted == false)).Select(svm => new SelectListViewModel { Id = svm.DepartmentId, Name = svm.DepartmentName, CompanyId = svm.CompanyId, OfficeId = svm.OfficeId }).ToList();
                    tvModel.OfficeList = ctx.Offices.Where(d => d.CompanyId == CompanyId && (d.DELETED == null || d.DELETED == false)).Select(svm => new SelectListViewModel { Id = svm.OfficeId, Name = svm.OfficeName }).ToList();
                }

                if (tvModel != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = tvModel;
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
        #endregion Team

        #region Department
        public ResponseModel GetOfficeIdFromDept(long DeptId)
        {
            ResponseModel res = new ResponseModel();
            using (var ctx = new NPMDBEntities())
            {
                try
                {
                    var officeId = ctx.Departments.Where(d => d.DepartmentId == DeptId).FirstOrDefault().OfficeId;
                    if (officeId > 0)
                    {
                        res.Status = "success";
                        res.Response = officeId;
                    }
                    else
                    {
                        res.Response = "Error";
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
            return res;
        }
        public ResponseModel GetDepartmentList(long? officeId)
        {
            ResponseModel objResponse = new ResponseModel();
            List<DepartmentViewModel> objDepartmentList = new List<DepartmentViewModel>();
            using (var ctx = new NPMDBEntities())
            {
                if (officeId != 0 && officeId != null)
                {
                    objDepartmentList = (from d in ctx.Departments
                                         join c in ctx.Companies on d.CompanyId equals c.CompanyId
                                         join o in ctx.Offices on d.OfficeId equals o.OfficeId
                                         where c.IsActive != false
                                         && (o.DELETED != true || o.DELETED == null)
                                         && (d.Deleted != true || d.Deleted == null)
                                         && d.OfficeId == officeId
                                         select new DepartmentViewModel()
                                         {
                                             OfficeId = o.OfficeId,
                                             OfficeName = o.OfficeName,
                                             CompanyId = c.CompanyId,
                                             CompanyName = c.CompanyName,
                                             DepartmentId = d.DepartmentId,
                                             DepartmentName = d.DepartmentName
                                         }).ToList();
                    //.OrderByDescending(d => d.DepartmentId)
                }
                else
                {
                    objDepartmentList = (from d in ctx.Departments
                                         join c in ctx.Companies on d.CompanyId equals c.CompanyId
                                         join o in ctx.Offices on d.OfficeId equals o.OfficeId
                                         where d.Deleted != true
                                         && o.DELETED != true
                                         && c.IsActive != false
                                         select new DepartmentViewModel()
                                         {
                                             OfficeId = o.OfficeId,
                                             OfficeName = o.OfficeName,
                                             CompanyId = c.CompanyId,
                                             CompanyName = c.CompanyName,
                                             DepartmentId = d.DepartmentId,
                                             DepartmentName = d.DepartmentName
                                         }).ToList();
                    //.OrderByDescending(d => d.DepartmentId)
                }
            }

            if (objDepartmentList != null)
            {
                objResponse.Status = "Sucess";
                objResponse.Response = objDepartmentList;
            }
            else
            {
                objResponse.Status = "Error";
            }
            return objResponse;
        }
        public ResponseModel GetDepartmentRecord(long DepartmentId)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                DepartmentViewModel _departmentViewModel = null;
                using (var ctx = new NPMDBEntities())
                {
                    Department _department = ctx.Departments.Where(ci => (ci.Deleted == null || ci.Deleted == false) && ci.DepartmentId == DepartmentId).FirstOrDefault();
                    if (_department != null)
                    {
                        _departmentViewModel.CompanyId = _department.CompanyId;
                        _departmentViewModel.Deleted = (_department.Deleted == null ? false : false);
                        _departmentViewModel.DepartmentId = _department.DepartmentId;
                        _departmentViewModel.DepartmentName = _department.DepartmentName;
                        _departmentViewModel.OfficeId = _department.OfficeId;
                        _departmentViewModel.CompanyName = ctx.Companies.FirstOrDefault(c => c.CompanyId == _department.CompanyId).CompanyName;
                        _departmentViewModel.OfficeName = ctx.Offices.FirstOrDefault(c => c.CompanyId == c.CompanyId && c.OfficeId == _department.OfficeId).OfficeName;
                    }

                }

                if (_departmentViewModel != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = _departmentViewModel;
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

        public ResponseModel SaveDepartment([FromBody] Department model)
        {
            ResponseModel objResponse = new ResponseModel();
            Department objModel = null;

            using (var ctx = new NPMDBEntities())
            {
                if (model.DepartmentId != 0)
                {
                    string sql = "update [department] set DepartmentName = {0}, CompanyId = {1}, OfficeId = {2}, ModifiedDate = {3} where DepartmentId = {4}";
                    var res = ctx.Database.ExecuteSqlCommand(sql, model.DepartmentName, model.CompanyId, model.OfficeId, model.ModifiedDate, model.DepartmentId);
                }
                else
                {
                    objModel = new Department();
                    int deptId = Convert.ToInt32(ctx.SP_TableIdGenerator("DepartmentId").FirstOrDefault().ToString());//ctx.Departments.Max(p => p.DepartmentId);

                    model.DepartmentId = deptId;
                    model.Deleted = false;
                    objModel.CreatedDate = DateTime.Now;

                    ctx.Departments.Add(model);
                    ctx.SaveChanges();
                }

                if (model != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = model.DepartmentId;
                }
                else
                {
                    objResponse.Status = "Error";
                }
            }

            return objResponse;
        }

        [HttpGet]
        public ResponseModel DeleteDepartment(long DepartmentId)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                List<DepartmentViewModel> objDepartmentList = null;
                using (var ctx = new NPMDBEntities())
                {

                    Department objDepartment = ctx.Departments.SingleOrDefault(c => c.DepartmentId == DepartmentId);
                    if (objDepartment != null)
                    {
                        objDepartment.Deleted = true;
                        ctx.SaveChanges();
                    }

                    if (GetDepartmentList(null).Status == "Sucess")
                    {
                        objResponse.Status = "Sucess";
                        objResponse.Response = objDepartmentList;
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
        #endregion Department

        #region Designation
        public ResponseModel GetDesignationList()
        {
            ResponseModel objResponse = new ResponseModel();
            List<DesignationViewModel> objDesignationList = new List<DesignationViewModel>();
            using (var ctx = new NPMDBEntities())
            {
                try
                {
                    objDesignationList = (from des in ctx.Designations
                                          join dep in ctx.Departments on des.DepartmentID equals dep.DepartmentId
                                          join off in ctx.Offices on dep.OfficeId equals off.OfficeId
                                          join com in ctx.Companies on dep.CompanyId equals com.CompanyId
                                          where (des.Deleted != true || des.Deleted == null)
                                          && (dep.Deleted != true || dep.Deleted == null)
                                          && (off.DELETED != true || off.DELETED == null)
                                          && (com.IsActive != false)
                                          select new DesignationViewModel
                                          {
                                              DesignationId = des.DesignationId,
                                              DesignationName = des.DesignationName,
                                              DepartmentID = des.DepartmentID,
                                              DepartmentName = dep.DepartmentName,
                                              Department_Head = des.Department_Head,
                                              CreatedBy = des.CreatedBy
                                          }).ToList();
                }
                catch (Exception)
                {
                    throw;
                }
            }
            if (objDesignationList != null)
            {
                objResponse.Status = "Sucess";
                objResponse.Response = objDesignationList;
            }
            else
            {
                objResponse.Status = "Error";
            }
            return objResponse;
        }

        public ResponseModel GetDesignationRecord(long DesignationId)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                DesignationViewModel _designationViewModel = new DesignationViewModel();
                using (var ctx = new NPMDBEntities())
                {
                    Designation _designation = ctx.Designations.Where(ci => (ci.Deleted == null || ci.Deleted == false)).FirstOrDefault();
                    if (_designation != null)
                    {
                        _designationViewModel.DesignationId = _designation.DesignationId;
                        _designationViewModel.DepartmentID = _designation.DepartmentID;
                        _designationViewModel.DesignationName = _designation.DesignationName;
                        _designationViewModel.DepartmentName = ctx.Departments.FirstOrDefault(d => d.DepartmentId == _designation.DepartmentID).DepartmentName;
                        _designationViewModel.Department_Head = _designation.Department_Head;

                        _designationViewModel.DepartmentList = ctx.Departments.Where(d => d.Deleted == null && d.Deleted == false).Select(dd => new SelectListViewModel { Id = dd.DepartmentId, Name = dd.DepartmentName }).ToList();

                    }

                }

                if (_designationViewModel != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = _designationViewModel;
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

        public ResponseModel SaveDesignation([FromBody] DesignationViewModel model)
        {
            ResponseModel objResponse = new ResponseModel();
            Designation objModel = null;

            using (var ctx = new NPMDBEntities())
            {
                if (model.DesignationId != 0)
                {
                    objModel = ctx.Designations.SingleOrDefault(p => p.DesignationId == model.DesignationId);
                    if (objModel != null)
                    {
                        objModel.DepartmentID = model.DepartmentID;
                        objModel.Department_Head = model.Department_Head;
                        objModel.DesignationName = model.DesignationName;
                        objModel.ModifiedDate = DateTime.Now;
                        ctx.SaveChanges();
                    }
                }
                else
                {
                    objModel = new Designation();
                    int desigId = Convert.ToInt32(ctx.SP_TableIdGenerator("DesignationId").FirstOrDefault().ToString());//ctx.Departments.Max(p => p.DepartmentId);

                    objModel.DesignationId = desigId;
                    objModel.DepartmentID = model.DepartmentID;
                    objModel.Department_Head = model.Department_Head;
                    objModel.DesignationName = model.DesignationName;
                    objModel.Deleted = false;
                    objModel.CreatedDate = DateTime.Now;
                    objModel.ModifiedDate = DateTime.Now;

                    ctx.Designations.Add(objModel);
                    ctx.SaveChanges();
                }

                if (model != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = objModel.DesignationId;
                }
                else
                {
                    objResponse.Status = "Error";
                }
            }

            return objResponse;
        }

        [HttpGet]
        public ResponseModel DeleteDesignation(long DesignationId)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                List<DesignationViewModel> objDesignationList = new List<DesignationViewModel>();
                using (var ctx = new NPMDBEntities())
                {

                    Designation objDesignation = ctx.Designations.SingleOrDefault(c => c.DesignationId == DesignationId);
                    if (objDesignation != null)
                    {
                        objDesignation.Deleted = true;
                        ctx.SaveChanges();
                    }

                    var objDesignationInnerList = ctx.Designations.Where(ci => (ci.Deleted == null || ci.Deleted == false)).ToList();
                    foreach (var item in objDesignationInnerList)
                    {
                        DesignationViewModel objViewModel = new DesignationViewModel();
                        objViewModel.DepartmentID = item.DepartmentID;
                        if (item.DepartmentID != null && item.DepartmentID != 0)
                            objViewModel.DepartmentName = ctx.Departments.FirstOrDefault(d => d.DepartmentId == item.DepartmentID).DepartmentName;

                        objViewModel.DesignationName = item.DesignationName;
                        objViewModel.Department_Head = item.Department_Head;
                        objViewModel.DesignationId = item.DesignationId;
                        objViewModel.CreatedDate = DateTime.Now;

                        objDesignationList.Add(objViewModel);
                    }

                    if (objDesignationList != null)
                    {
                        objResponse.Status = "Sucess";
                        objResponse.Response = objDesignationList;
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



        #endregion Designation
    }
}