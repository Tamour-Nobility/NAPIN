using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using NPMAPI.Models;
using NPMAPI.Models.ViewModels;
using NPMAPI.Repositories;

namespace NPMAPI.Services
{
    public class InsuranceSetupService : IInsuranceSetupRepository
    {
        #region InsuranceGroup
        public ResponseModel DeleteInsuranceGroup(long InsuranceGroupId)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                List<Insurance_Groups> InsuranceGroupList = null;
                using (var ctx = new NPMDBEntities())
                {
                    //validate its not used in any insurance
                    var result = ctx.ValidateInsGroupDeletion(InsuranceGroupId).ToList();

                    if (result == null || result.Count == 0)
                    {
                        Insurance_Groups objInsuranceGroup = ctx.Insurance_Groups.SingleOrDefault(c => c.Insgroup_Id == InsuranceGroupId);
                        if (objInsuranceGroup != null)
                        {
                            objInsuranceGroup.Deleted = true;
                            ctx.SaveChanges();
                        }

                        InsuranceGroupList = ctx.Insurance_Groups.Where(c => (c.Deleted ?? false) == false).OrderByDescending(d => d.Created_Date).ToList();
                        if (InsuranceGroupList != null)
                        {
                            objResponse.Status = "Sucess";
                            objResponse.Response = InsuranceGroupList;
                        }
                        else
                        {
                            objResponse.Status = "Error";
                        }
                    }
                    else
                    {
                        objResponse.Status = "Can't Delete this group its used in insurances";
                    }
                }
            }
            catch (Exception)
            {
                objResponse.Status = "Error";
            }
            return objResponse;
        }
        public ResponseModel GetInsuranceGroup(long InsuranceGroupId)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                InsuranceGroupViewModel _InsuranceGroup = null;
                using (var ctx = new NPMDBEntities())
                {
                    _InsuranceGroup = (from insGroup in ctx.Insurance_Groups
                                       join userCreated in ctx.Users on insGroup.Created_By equals userCreated.UserId into uc
                                       from userCreated in uc.DefaultIfEmpty()
                                       join userModified in ctx.Users on insGroup.Modified_By equals userModified.UserId into um
                                       from userModified in um.DefaultIfEmpty()
                                       where (insGroup.Deleted ?? false) == false
                                       where insGroup.Insgroup_Id == InsuranceGroupId
                                       select new InsuranceGroupViewModel()
                                       {
                                           Deleted = insGroup.Deleted,
                                           Created_By = insGroup.Created_By,
                                           Created_By_Name = userCreated != null ? userCreated.LastName + ", " + userCreated.FirstName : "",
                                           Created_Date = insGroup.Created_Date,
                                           Insgroup_Id = insGroup.Insgroup_Id,
                                           Insgroup_Name = insGroup.Insgroup_Name,
                                           Modified_By = insGroup.Modified_By,
                                           Modified_By_Name = userModified != null ? userModified.LastName + ", " + userModified.FirstName : "",
                                           Modified_Date = insGroup.Modified_Date
                                       }).FirstOrDefault();
                }

                if (_InsuranceGroup != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = _InsuranceGroup;
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
        public ResponseModel GetInsuranceGroupList()
        {
            ResponseModel objResponse = new ResponseModel();
            List<InsuranceGroupViewModel> InsuranceGroupList = null;
            using (var ctx = new NPMDBEntities())
            {
                InsuranceGroupList = (from insGroup in ctx.Insurance_Groups
                                      join userCreated in ctx.Users on insGroup.Created_By equals userCreated.UserId into uc
                                      from userCreated in uc.DefaultIfEmpty()
                                      join userModified in ctx.Users on insGroup.Modified_By equals userModified.UserId into um
                                      from userModified in um.DefaultIfEmpty()
                                      where (insGroup.Deleted ?? false) == false
                                      select new InsuranceGroupViewModel()
                                      {
                                          Deleted = insGroup.Deleted,
                                          Created_By = insGroup.Created_By,
                                          Created_By_Name = userCreated != null ? userCreated.LastName + ", " + userCreated.FirstName : "",
                                          Created_Date = insGroup.Created_Date,
                                          Insgroup_Id = insGroup.Insgroup_Id,
                                          Insgroup_Name = insGroup.Insgroup_Name,
                                          Modified_By = insGroup.Modified_By,
                                          Modified_By_Name = userModified != null ? userModified.LastName + ", " + userModified.FirstName : "",
                                          Modified_Date = insGroup.Modified_Date
                                      }).OrderByDescending(ing => ing.Created_Date).ToList();
            }

            if (InsuranceGroupList != null)
            {
                objResponse.Status = "Sucess";
                objResponse.Response = InsuranceGroupList;
            }
            else
            {
                objResponse.Status = "Error";
            }
            return objResponse;
        }
        public ResponseModel SaveInsuranceGroup([FromBody] Insurance_Groups Model, long userId)
        {
            ResponseModel objResponse = new ResponseModel();
            Insurance_Groups objModel = new Insurance_Groups();

            using (var ctx = new NPMDBEntities())
            {
                // Check if the insurance group with the same name already exists (ignoring case and leading/trailing spaces)
                var duplicateGroup = ctx.Insurance_Groups
                    .FirstOrDefault(p => p.Insgroup_Name.Trim().ToLower() == Model.Insgroup_Name.Trim().ToLower()
                                        && (p.Deleted == false)
                                        && p.Insgroup_Id != Model.Insgroup_Id);

                if (duplicateGroup != null)
                {
                    objResponse.Status = "Duplicate Group Name";
                    objResponse.Response = "Insurance Group with the same name already exists.";
                    return objResponse;
                }

                if (Model.Insgroup_Id != 0)
                {
                    // Update existing group
                    objModel = ctx.Insurance_Groups.SingleOrDefault(p => p.Insgroup_Id == Model.Insgroup_Id);
                    if (objModel != null)
                    {
                        objModel.Insgroup_Name = Model.Insgroup_Name.Trim();
                        objModel.Modified_By = userId;
                        objModel.Modified_Date = DateTime.Now;
                        ctx.SaveChanges();
                    }
                }
                else
                {
                    // Create new group
                    objModel = new Insurance_Groups();
                    long InsGroupId = Convert.ToInt64(ctx.SP_TableIdGenerator("Insgroup_Id").FirstOrDefault().ToString());
                    Model.Insgroup_Id = InsGroupId;
                    Model.Insgroup_Name = Model.Insgroup_Name.Trim();
                    Model.Deleted = false;
                    Model.Created_By = userId;
                    Model.Created_Date = DateTime.Now;
                    ctx.Insurance_Groups.Add(Model);
                    ctx.SaveChanges();
                }

                if (Model != null)
                {
                    objResponse.Status = "Success";
                    objResponse.Response = Model.Insgroup_Id;
                }
                else
                {
                    objResponse.Status = "Error";
                }
            }

            return objResponse;
        }


        public ResponseModel GetInsuranceGroupsSelectList()
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var groupsSelectList = ctx.Insurance_Groups.Where(ig => (ig.Deleted ?? false) == false).Select(ig => new SelectListViewModel()
                    {
                        Id = ig.Insgroup_Id,
                        IdStr = ig.Insgroup_Id + "",
                        Name = ig.Insgroup_Name
                    }).OrderBy(i => i.Name).ToList();
                    responseModel.Response = groupsSelectList;
                    responseModel.Status = "Success";
                }
            }
            catch (Exception ex)
            {
                responseModel.Status = ex.ToString();
            }
            return responseModel;
        }
        public ResponseModel GetSmartInsuranceGroupsSelectList(string searchText)
        {
            ResponseModel responseModel = new ResponseModel();
            List<SelectListViewModel> selectLists = new List<SelectListViewModel>();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    selectLists = ctx.Insurance_Groups.Where(i => !(i.Deleted ?? false) && i.Insgroup_Name.Contains(searchText)).Select(i => new SelectListViewModel()
                    {
                        Id = i.Insgroup_Id,
                        IdStr = i.Insgroup_Id.ToString(),
                        Name = i.Insgroup_Name
                    }).ToList();
                    responseModel.Status = "Success";
                    responseModel.Response = selectLists;
                }
            }
            catch (Exception ex)
            {
                responseModel.Status = ex.ToString();
            }
            return responseModel;
        }
        #endregion InsuranceGroup

        #region InsuranceName
        public ResponseModel DeleteInsuranceName(long InsuranceNameId)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                List<InsuranceNameViewModel> _InsuranceNamesList = new List<InsuranceNameViewModel>();
                using (var ctx = new NPMDBEntities())
                {
                    Insurance_Names objInsuranceName = ctx.Insurance_Names.SingleOrDefault(c => c.Insname_Id == InsuranceNameId);
                    if (objInsuranceName != null)
                    {
                        objInsuranceName.Deleted = true;
                        ctx.SaveChanges();
                        var insuranceNamesList = ctx.GetInsuranceNamesByGroup(objInsuranceName.Insgroup_Id).ToList();
                        if (insuranceNamesList != null)
                        {
                            objResponse.Status = "Sucess";
                            objResponse.Response = insuranceNamesList;
                        }
                        else
                        {
                            objResponse.Status = "Error";
                        }
                    }
                }

            }
            catch (Exception)
            {
                objResponse.Status = "Error";
            }
            return objResponse;
        }
        public ResponseModel GetInsuranceName(long InsuranceNameId)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                InsuranceNameViewModel _InsuranceGroup = new InsuranceNameViewModel();
                using (var ctx = new NPMDBEntities())
                {
                    _InsuranceGroup = (from inn in ctx.Insurance_Names
                                       join ig in ctx.Insurance_Groups on inn.Insgroup_Id equals ig.Insgroup_Id
                                       join ucre in ctx.Users on inn.Created_By equals ucre.UserId into ucreInn
                                       from ucre in ucreInn.DefaultIfEmpty()
                                       join umod in ctx.Users on inn.Modified_By equals umod.UserId into umodInn
                                       from umod in umodInn.DefaultIfEmpty()
                                       where !(inn.Deleted ?? false) && inn.Insname_Id == InsuranceNameId
                                       select new InsuranceNameViewModel()
                                       {
                                           Deleted = inn.Deleted,
                                           Created_By = inn.Created_By,
                                           Created_By_Name = ucre == null ? "" : ucre.LastName + ", " + ucre.FirstName,
                                           Created_Date = inn.Created_Date,
                                           InsuranceGroupId = inn.Insgroup_Id,
                                           InsuranceGroupName = ig.Insgroup_Name,
                                           InsuranceNameDescription = inn.Insname_Description,
                                           InsuranceNameId = inn.Insname_Id,
                                           Modified_By = inn.Modified_By,
                                           Modified_By_Name = umod == null ? "" : umod.LastName + ", " + umod.FirstName,
                                           Modified_Date = inn.Modified_Date
                                       }).FirstOrDefault();
                    responseModel.Status = "Sucess";
                    responseModel.Response = _InsuranceGroup;
                }
            }
            catch (Exception)
            {
                responseModel.Status = "Error";
            }
            return responseModel;
        }
        public ResponseModel GetInsuranceNameModel(long? insuranceNameId)
        {
            ResponseModel responseModel = new ResponseModel();
            InsuranceNameModelViewModel insName = new InsuranceNameModelViewModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {

                    if (insuranceNameId != null)
                        insName = ctx.Insurance_Names.Where(inn => (inn.Deleted ?? false) == false && inn.Insname_Id == insuranceNameId).Select(inn => new InsuranceNameModelViewModel()
                        {
                            Deleted = inn.Deleted,
                            Created_By = inn.Created_By,
                            Created_Date = inn.Created_Date,
                            Insgroup_Id = inn.Insgroup_Id,
                            Insname_Description = inn.Insname_Description,
                            Insname_Id = inn.Insname_Id,
                            InsuranceGroup = ctx.Insurance_Groups.Where(ig => !(ig.Deleted ?? false) && ig.Insgroup_Id == inn.Insgroup_Id).Select(ig => new SelectListViewModel()
                            {
                                Id = ig.Insgroup_Id,
                                IdStr = ig.Insgroup_Id.ToString(),
                                Name = ig.Insgroup_Name
                            }).FirstOrDefault(),
                            Modified_By = inn.Modified_By,
                            Modified_Date = inn.Modified_Date
                        }).FirstOrDefault();
                    responseModel.Status = "Success";
                    responseModel.Response = insName;
                }
            }
            catch (Exception ex)
            {
                responseModel.Status = ex.ToString();
            }
            return responseModel;
        }
        public ResponseModel GetInsuranceNameSelectList(long? insuranceGroupId)
        {
            ResponseModel responseModel = new ResponseModel();
            List<SelectListViewModel> nameSelectList = null;
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    if (insuranceGroupId != null && insuranceGroupId > 0)
                    {
                        nameSelectList = ctx.Insurance_Names.Where(iname => (iname.Deleted ?? false) == false && iname.Insgroup_Id == insuranceGroupId).Select(iname => new SelectListViewModel()
                        {
                            Id = iname.Insname_Id,
                            IdStr = iname.Insname_Id + "",
                            Name = iname.Insname_Description
                        }).OrderBy(t => t.Name).ToList();
                    }
                    else
                    {
                        nameSelectList = ctx.Insurance_Names.Where(iname => (iname.Deleted ?? false) == false).Select(iname => new SelectListViewModel()
                        {
                            Id = iname.Insname_Id,
                            IdStr = iname.Insname_Id + "",
                            Name = iname.Insname_Description
                        }).OrderBy(t => t.Name).ToList();
                    }
                    responseModel.Response = nameSelectList;
                    responseModel.Status = "Success";
                }
            }
            catch (Exception ex)
            {
                responseModel.Status = ex.ToString();
            }
            return responseModel;
        }
        public ResponseModel GetSmartInsuranceNameList(long? insuranceGroupId, string searchText)
        {
            ResponseModel responseModel = new ResponseModel();
            List<SelectListViewModel> selectLists = new List<SelectListViewModel>();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    if (insuranceGroupId != null)
                    {
                        selectLists = ctx.Insurance_Names.Where(inn => !(inn.Deleted ?? false) && inn.Insgroup_Id == insuranceGroupId && inn.Insname_Description.Contains(searchText)).Select(inn => new SelectListViewModel()
                        {
                            Id = inn.Insname_Id,
                            IdStr = inn.Insname_Id.ToString(),
                            Name = inn.Insname_Description
                        }).ToList();
                    }
                    else
                    {
                        selectLists = ctx.Insurance_Names.Where(inn => !(inn.Deleted ?? false) && inn.Insname_Description.Contains(searchText)).Select(inn => new SelectListViewModel()
                        {
                            Id = inn.Insname_Id,
                            IdStr = inn.Insname_Id.ToString(),
                            Name = inn.Insname_Description
                        }).ToList();
                    }
                    responseModel.Status = "Success";
                    responseModel.Response = selectLists;
                }
            }
            catch (Exception ex)
            {
                responseModel.Status = ex.ToString();
            }
            return responseModel;
        }
        public ResponseModel GetInsuranceNameList(long InsuranceGroupId)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var insuranceNamesList = ctx.GetInsuranceNamesByGroup(InsuranceGroupId).ToList();
                    if (insuranceNamesList != null)
                    {
                        objResponse.Status = "Sucess";
                        objResponse.Response = insuranceNamesList;
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
        public ResponseModel SaveInsuranceName([FromBody] InsuranceNameModelViewModel Model, long userId)
        {
            ResponseModel objResponse = new ResponseModel();
            Insurance_Names objModel = null;

            using (var ctx = new NPMDBEntities())
            {
                if (Model.Insname_Id != 0)
                {
                    objModel = ctx.Insurance_Names.SingleOrDefault(p => p.Insname_Id == Model.Insname_Id);
                    if (objModel != null)
                    {
                        objModel.Insgroup_Id = Model.Insgroup_Id;
                        objModel.Insname_Description = Model.Insname_Description;
                        objModel.Modified_By = userId;
                        objModel.Modified_Date = DateTime.Now;
                        ctx.SaveChanges();
                    }
                }
                else
                {
                    objModel = new Insurance_Names();
                    long InsNameId = Convert.ToInt64(ctx.SP_TableIdGenerator("Insname_Id").FirstOrDefault().ToString());
                    objModel.Insname_Id = InsNameId;
                    objModel.Insgroup_Id = Model.Insgroup_Id;
                    objModel.Insname_Description = Model.Insname_Description;
                    objModel.Deleted = false;
                    objModel.Created_By = userId;
                    objModel.Created_Date = DateTime.Now;
                    ctx.Insurance_Names.Add(objModel);
                    ctx.SaveChanges();
                }

                if (Model != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = Model.Insname_Id;
                }
                else
                {
                    objResponse.Status = "Error";
                }
            }

            return objResponse;
        }
        #endregion InsuranceName

        #region InsurancePayer
        public ResponseModel GetInsurancePayerList(long InsuranceGroupId, long InsuranceNameId)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                List<GetInsurancePayersByName_Result> _InsuranceNamesList = null;
                using (var ctx = new NPMDBEntities())
                {
                    _InsuranceNamesList = ctx.GetInsurancePayersByName(InsuranceNameId).ToList();
                }


                if (_InsuranceNamesList != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = _InsuranceNamesList;
                }
                else
                {
                    objResponse.Status = "Error";
                }
            }
            catch (Exception)
            {
                throw;
            }
            return objResponse;
        }
        public ResponseModel GetInsPayerById(string InsurancePayerId)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                GetInsurancePayersByInsPayerId_Result obj = new GetInsurancePayersByInsPayerId_Result();
                using (var ctx = new NPMDBEntities())
                {
                    obj = ctx.GetInsurancePayersByInsPayerId(InsurancePayerId).FirstOrDefault();
                }
                if (obj != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = obj;
                }
                else
                {
                    objResponse.Status = "Error";
                }
            }
            catch (Exception)
            {
                throw;
            }
            return objResponse;
        }
        public ResponseModel GetInsPayerByState(string InsurancePayerState)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                List<GetInsurancePayersByInsPayerState_Result> res = null;
                using (var ctx = new NPMDBEntities())
                {
                    res = ctx.GetInsurancePayersByInsPayerState(InsurancePayerState).ToList();
                }
                if (res != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = res;
                }
                else
                {
                    objResponse.Status = "Error";
                }
            }
            catch (Exception)
            {
                throw;
            }
            return objResponse;
        }
        public ResponseModel GetInsurancePayer(long InsurancePayerId)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var objInsPayer = (from insPayer in ctx.Insurance_Payers
                                       join insName in ctx.Insurance_Names on insPayer.Insname_Id equals insName.Insname_Id
                                       join insGroup in ctx.Insurance_Groups on insName.Insgroup_Id equals insGroup.Insgroup_Id
                                       where (insPayer.Deleted ?? false) == false && (insName.Deleted ?? false) == false && (insGroup.Deleted ?? false) == false && insPayer.Inspayer_Id == InsurancePayerId
                                       select new InsurancePayerModelVM()
                                       {
                                           Inspayer_Id = insPayer.Inspayer_Id,
                                           Is_Part_A = insPayer.Is_Part_A,
                                           Is_RTA_Payer = insPayer.Is_RTA_Payer,
                                           Submission_type = insPayer.Submission_type,
                                           Timely_Filing_Days = insPayer.Timely_Filing_Days,
                                           Insgroup_Id = insName.Insgroup_Id,
                                           Inspayer_State = insPayer.Inspayer_State,
                                           Edisetup_Required = insPayer.EDISETUPREQUIRED,
                                           Erasetup_Required = insPayer.ERASETUPREQUIRED,
                                           Insgroup_name = insGroup.Insgroup_Name,
                                           Insname_Description = insName.Insname_Description,
                                           Insname_Id = insPayer.Insname_Id,
                                           Inspayer_Description = insPayer.Inspayer_Description,
                                           Inspayer_Plan = insPayer.Inspayer_Plan,
                                           Inspayer_835_Id = insPayer.Inspayer_835_Id,
                                           Inspayer_837_Id = insPayer.Inspayer_837_Id,
                                           Created_By = insPayer.Created_By,
                                           Created_Date = insPayer.Created_Date,
                                           Modified_By = insPayer.Modified_By,
                                           Modified_Date = insPayer.Modified_Date
                                       }).FirstOrDefault();
                    if (objInsPayer != null)
                    {
                        objResponse.Status = "Sucess";
                        objResponse.Response = objInsPayer;
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
        public ResponseModel GetInsurancePayerModel(long? InsurancePayerId)
        {
            ResponseModel objResponse = new ResponseModel();
            InsurancePayerModelVM insPayermodel = new InsurancePayerModelVM();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    if (InsurancePayerId != null && InsurancePayerId > 0)
                    {
                        insPayermodel = (from insPayer in ctx.Insurance_Payers
                                         join insName in ctx.Insurance_Names on insPayer.Insname_Id equals insName.Insname_Id
                                         join insGroup in ctx.Insurance_Groups on insName.Insgroup_Id equals insGroup.Insgroup_Id
                                         join insList in ctx.insurancelists on insPayer.Inspayer_Id equals insList.inspayer_id into insListGroup
                                         from insList in insListGroup.DefaultIfEmpty() // Left join to handle missing data in insurancelist
                                         where (insPayer.Deleted ?? false) == false && (insName.Deleted ?? false) == false && (insGroup.Deleted ?? false) == false && insPayer.Inspayer_Id == InsurancePayerId
                                         select new InsurancePayerModelVM()
                                         {
                                             Inspayer_Id = insPayer.Inspayer_Id,
                                             Is_Part_A = insPayer.Is_Part_A,
                                             Is_RTA_Payer = insPayer.Is_RTA_Payer,
                                             Submission_type = insPayer.Submission_type,
                                             Timely_Filing_Days = insPayer.Timely_Filing_Days,
                                             Insgroup_Id = insName.Insgroup_Id,
                                             Inspayer_State = insPayer.Inspayer_State,
                                             Inspayer_837_Id = insPayer.Inspayer_837_Id,
                                             Inspayer_835_Id = insPayer.Inspayer_835_Id,
                                             Erasetup_Required = insPayer.Erasetup_Required,
                                             Edisetup_Required = insPayer.Edisetup_Required,
                                             Insgroup_name = insGroup.Insgroup_Name,
                                             Insname_Description = insName.Insname_Description,
                                             Insname_Id = insPayer.Insname_Id,
                                             Inspayer_Description = insPayer.Inspayer_Description,
                                             Inspayer_Plan = insPayer.Inspayer_Plan,
                                             InsuranceGroup = new SelectListViewModel() { Id = insGroup.Insgroup_Id, IdStr = insGroup.Insgroup_Id.ToString(), Name = insGroup.Insgroup_Name },
                                             InsuranceName = new SelectListViewModel() { Id = insName.Insname_Id, IdStr = insName.Insname_Id.ToString(), Name = insName.Insname_Description },
                                             Insurance_Type_Code = insList.insurance_type_code

                                         }).FirstOrDefault();
                    }
                    if (insPayermodel != null)
                    {
                        objResponse.Status = "Sucess";
                        objResponse.Response = insPayermodel;
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
        public ResponseModel SaveInsurancePayer([FromBody] InsurancePayerViewModel insPayer, long v)
        {
            ResponseModel objResponse = new ResponseModel();
            Insurance_Payers _InsurancePayer = new Insurance_Payers();

            using (var ctx = new NPMDBEntities())
            {
                if (insPayer.Inspayer_Id != 0)
                {
                    _InsurancePayer = ctx.Insurance_Payers.SingleOrDefault(p => p.Inspayer_Id == insPayer.Inspayer_Id);
                    if (_InsurancePayer != null)
                    {
                        _InsurancePayer.Acknowledgement_Type = insPayer.Acknowledgement_Type;
                        _InsurancePayer.Edisetup_Required = insPayer.Edisetup_Required;
                        _InsurancePayer.EFTSETUPREQUIRED = insPayer.EFTSETUPREQUIRED;
                        _InsurancePayer.Electronic_Corrected_Claims = insPayer.Electronic_Corrected_Claims;
                        _InsurancePayer.Electronic_Late_Filing = insPayer.Electronic_Late_Filing;
                        _InsurancePayer.Erasetup_Required = insPayer.Erasetup_Required;
                        _InsurancePayer.Insname_Id = insPayer.Insname_Id;
                        _InsurancePayer.Inspayer_835_Id = insPayer.Inspayer_835_Id;
                        _InsurancePayer.Inspayer_837_Id = insPayer.Inspayer_837_Id;
                        _InsurancePayer.InsPayer_Claim_Status_Id = insPayer.InsPayer_Claim_Status_Id;
                        _InsurancePayer.Inspayer_Description = insPayer.Inspayer_Description;
                        _InsurancePayer.InsPayer_Description_old = insPayer.InsPayer_Description_old;
                        _InsurancePayer.InsPayer_Eligibility_Id = insPayer.InsPayer_Eligibility_Id;
                        _InsurancePayer.Inspayer_Id = insPayer.Inspayer_Id;
                        _InsurancePayer.Inspayer_Plan = insPayer.Inspayer_Plan;
                        _InsurancePayer.Inspayer_Referral_Id = insPayer.Inspayer_Referral_Id;
                        _InsurancePayer.Inspayer_State = insPayer.Inspayer_State;
                        _InsurancePayer.Is_Nonpar_CS = insPayer.Is_Nonpar_CS;
                        _InsurancePayer.Is_Nonpar_Era = insPayer.Is_Nonpar_Era;
                        _InsurancePayer.Is_Part_A = insPayer.Is_Part_A;
                        _InsurancePayer.Is_RTA_Payer = insPayer.Is_RTA_Payer;
                        _InsurancePayer.Is_Sec_Paper = insPayer.Is_Sec_Paper;
                        _InsurancePayer.Ivr_Server_Id = insPayer.Ivr_Server_Id;
                        _InsurancePayer.MU_Category = insPayer.MU_Category;
                        _InsurancePayer.Npi_Type = insPayer.Npi_Type;
                        _InsurancePayer.Restricted_Calls = insPayer.Restricted_Calls;
                        _InsurancePayer.SERVER_ID = insPayer.SERVER_ID;
                        _InsurancePayer.Submission_type = insPayer.Submission_type;
                        _InsurancePayer.Timely_Filing_Days = insPayer.Timely_Filing_Days;
                        _InsurancePayer.Modified_By = v;
                        _InsurancePayer.Modified_Date = DateTime.Now;
                        ctx.SaveChanges();
                    }
                }
                else
                {
                    _InsurancePayer = new Insurance_Payers();
                    long InsPayerId = Convert.ToInt64(ctx.SP_TableIdGenerator("Inspayer_Id").FirstOrDefault().ToString());
                    _InsurancePayer.Inspayer_Id = InsPayerId;
                    _InsurancePayer.Acknowledgement_Type = insPayer.Acknowledgement_Type;
                    _InsurancePayer.Edisetup_Required = insPayer.Edisetup_Required;
                    _InsurancePayer.EFTSETUPREQUIRED = insPayer.EFTSETUPREQUIRED;
                    _InsurancePayer.Electronic_Corrected_Claims = insPayer.Electronic_Corrected_Claims;
                    _InsurancePayer.Electronic_Late_Filing = insPayer.Electronic_Late_Filing;
                    _InsurancePayer.Erasetup_Required = insPayer.Erasetup_Required;
                    _InsurancePayer.Insname_Id = insPayer.Insname_Id;
                    _InsurancePayer.Inspayer_835_Id = insPayer.Inspayer_835_Id;
                    _InsurancePayer.Inspayer_837_Id = insPayer.Inspayer_837_Id;
                    _InsurancePayer.InsPayer_Claim_Status_Id = insPayer.InsPayer_Claim_Status_Id;
                    _InsurancePayer.Inspayer_Description = insPayer.Inspayer_Description;
                    _InsurancePayer.InsPayer_Description_old = insPayer.InsPayer_Description_old;
                    _InsurancePayer.InsPayer_Eligibility_Id = insPayer.InsPayer_Eligibility_Id;
                    _InsurancePayer.Inspayer_Plan = insPayer.Inspayer_Plan;
                    _InsurancePayer.Inspayer_Referral_Id = insPayer.Inspayer_Referral_Id;
                    _InsurancePayer.Inspayer_State = insPayer.Inspayer_State;
                    _InsurancePayer.Is_Nonpar_CS = insPayer.Is_Nonpar_CS;
                    _InsurancePayer.Is_Nonpar_Era = insPayer.Is_Nonpar_Era;
                    _InsurancePayer.Is_Part_A = insPayer.Is_Part_A;
                    _InsurancePayer.Is_RTA_Payer = insPayer.Is_RTA_Payer;
                    _InsurancePayer.Is_Sec_Paper = insPayer.Is_Sec_Paper;
                    _InsurancePayer.Ivr_Server_Id = insPayer.Ivr_Server_Id;
                    _InsurancePayer.MU_Category = insPayer.MU_Category;
                    _InsurancePayer.Npi_Type = insPayer.Npi_Type;
                    _InsurancePayer.Restricted_Calls = insPayer.Restricted_Calls;
                    _InsurancePayer.SERVER_ID = insPayer.SERVER_ID;
                    _InsurancePayer.Submission_type = insPayer.Submission_type;
                    _InsurancePayer.Timely_Filing_Days = insPayer.Timely_Filing_Days;
                    _InsurancePayer.Deleted = false;
                    _InsurancePayer.Created_By = v;
                    _InsurancePayer.Created_Date = DateTime.Now;

                    ctx.Insurance_Payers.Add(_InsurancePayer);
                    ctx.SaveChanges();
                }

                if (_InsurancePayer != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = _InsurancePayer.Inspayer_Id;
                }
                else
                {
                    objResponse.Status = "Error";
                }
            }

            return objResponse;
        }
        public ResponseModel DeleteInsurancePayer(long InsurancePayerId, long InsuranceNameId)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                List<GetInsurancePayersByName_Result> _InsuranceNamesList = new List<GetInsurancePayersByName_Result>();
                using (var ctx = new NPMDBEntities())
                {

                    Insurance_Payers objInsurancePayer = ctx.Insurance_Payers.SingleOrDefault(c => c.Inspayer_Id == InsurancePayerId);
                    if (objInsurancePayer != null)
                    {
                        objInsurancePayer.Deleted = true;
                        ctx.SaveChanges();
                    }

                    //_InsuranceNamesList = ctx.GetInsurancePayersByName(InsuranceNameId).ToList();
                }

                if (_InsuranceNamesList != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = _InsuranceNamesList;
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
        public ResponseModel GetSmartInsurancePayersList(string searchText)
        {
            ResponseModel responseModel = new ResponseModel();
            List<SelectListViewModel> list = new List<SelectListViewModel>();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    list = ctx.Insurance_Payers.Where(ig => (ig.Deleted ?? false) == false && (ig.Is_Sec_Paper ?? false) == false && ig.Inspayer_Description.Contains(searchText)).Select(svm => new SelectListViewModel { Id = svm.Inspayer_Id, Name = svm.Inspayer_Description }).OrderBy(t => t.Name).ToList();
                    if (list != null)
                    {
                        responseModel.Status = "Success";
                        responseModel.Response = list;
                    }
                    else
                    {
                        responseModel.Status = "error";
                    }
                }
            }
            catch (Exception ex)
            {
                responseModel.Status = ex.ToString();
            }
            return responseModel;
        }
        #endregion InsurancePayer

        #region Insurance
        public ResponseModel GetInsPayerList()
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var _InsurancePayer = ctx.Insurance_Payers.Where(ig => (ig.Deleted ?? false) == false && (ig.Is_Sec_Paper ?? false) == false).Select(svm => new SelectListViewModel { Id = svm.Inspayer_Id, Name = svm.Inspayer_Description }).OrderBy(t => t.Name).ToList();
                    if (_InsurancePayer != null)
                    {
                        objResponse.Status = "Sucess";
                        objResponse.Response = _InsurancePayer;
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
        public ResponseModel GetInsuranceList(long? GroupId, string insuranceName, string insurancePayerName)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                List<USP_Search_Insurances_Result> _InsuranceList = new List<USP_Search_Insurances_Result>();
                using (var ctx = new NPMDBEntities())
                {
                    _InsuranceList = ctx.USP_Search_Insurances(GroupId,  insuranceName,  insurancePayerName).ToList();
                    if (_InsuranceList != null)
                    {
                        objResponse.Status = "Sucess";
                        objResponse.Response = _InsuranceList;
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
        public ResponseModel GetInsurance(long InsuranceId)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                InsurancesByPayersViewModel _Insurances = new InsurancesByPayersViewModel();
                using (var ctx = new NPMDBEntities())
                {
                    var insurance = ctx.Insurances.SingleOrDefault(c => c.Insurance_Id == InsuranceId);
                    if (insurance != null)
                    {
                        if (insurance.Deleted != null)
                            _Insurances.Deleted = (bool)insurance.Deleted;

                        _Insurances.Insurance_Id = insurance.Insurance_Id;
                        _Insurances.AppealFilingLimit = insurance.AppealFilingLimit;
                        _Insurances.ClaimFilingLimit = insurance.ClaimFilingLimit;
                        _Insurances.EPSDT_WOKERINFO = insurance.EPSDT_WorkerInfo;
                        _Insurances.InActive = insurance.InActive;
                        _Insurances.InsPayer_Id = insurance.InsPayer_Id;
                        _Insurances.Insurance_Address = insurance.Insurance_Address;
                        _Insurances.Insurance_CardCategory = insurance.Insurance_CardCategory;
                        _Insurances.Insurance_City = insurance.Insurance_City;
                        _Insurances.Insurance_Department = insurance.Insurance_Department;
                        _Insurances.Insurance_Phone_Number1 = insurance.Insurance_Phone_Number1;
                        _Insurances.Insurance_Phone_Number2 = insurance.Insurance_Phone_Number2;
                        _Insurances.Insurance_Phone_Number3 = insurance.Insurance_Phone_Number3;
                        _Insurances.Insurance_Phone_Type1 = insurance.Insurance_Phone_Type1;
                        _Insurances.Insurance_Phone_Type2 = insurance.Insurance_Phone_Type2;
                        _Insurances.Insurance_Phone_Type3 = insurance.Insurance_Phone_Type3;
                        _Insurances.Insurance_State = insurance.Insurance_State;
                        _Insurances.Insurance_Zip = insurance.Insurance_Zip;
                        _Insurances.Is_Sec_Attach_Need = insurance.Is_Sec_Attach_Need;
                        _Insurances.IS_SEC_Paper = insurance.IS_SEC_Paper;
                        _Insurances.STOP_SUBMISSION = insurance.STOP_SUBMISSION;
                        _Insurances.Sub_Method = insurance.Sub_Method;
                        _Insurances.Time_From = insurance.Time_From;
                        _Insurances.Time_To = insurance.Time_To;
                        var insPayer = ctx.Insurance_Payers.FirstOrDefault(ig => ig.Inspayer_Id == insurance.InsPayer_Id);
                        if (insPayer != null && insPayer.Inspayer_Description != null)
                            _Insurances.InsurancePayerDescription = insPayer.Inspayer_Description;
                    }
                }

                if (_Insurances != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = _Insurances;
                }
                else
                {
                    objResponse.Status = "Error";
                }
            }
            catch (Exception ex)
            {
                objResponse.Status = "Error";
            }
            return objResponse;
        }
        public ResponseModel SaveInsurance([FromBody] Insurance insurance, long userId)
        {
            ResponseModel objResponse = new ResponseModel();
            Insurance _Insurances = new Insurance();

            using (var ctx = new NPMDBEntities())
            {
                if (insurance.Insurance_Id != 0)
                {
                    _Insurances = ctx.Insurances.SingleOrDefault(p => p.Insurance_Id == insurance.Insurance_Id);
                    if (_Insurances != null)
                    {
                        if (insurance.Deleted != null)
                            _Insurances.Deleted = (bool)insurance.Deleted;

                        _Insurances.AppealFilingLimit = insurance.AppealFilingLimit;
                        _Insurances.ClaimFilingLimit = insurance.ClaimFilingLimit;
                        _Insurances.EPSDT_WorkerInfo = insurance.EPSDT_WorkerInfo;
                        _Insurances.InActive = insurance.InActive;
                        _Insurances.InsPayer_Id = insurance.InsPayer_Id;
                        _Insurances.Insurance_Address = insurance.Insurance_Address;
                        _Insurances.Insurance_CardCategory = insurance.Insurance_CardCategory;
                        _Insurances.Insurance_City = insurance.Insurance_City;
                        _Insurances.Insurance_Department = insurance.Insurance_Department;
                        _Insurances.Insurance_Phone_Number1 = insurance.Insurance_Phone_Number1;
                        _Insurances.Insurance_Phone_Number2 = insurance.Insurance_Phone_Number2;
                        _Insurances.Insurance_Phone_Number3 = insurance.Insurance_Phone_Number3;
                        _Insurances.Insurance_Phone_Type1 = insurance.Insurance_Phone_Type1;
                        _Insurances.Insurance_Phone_Type2 = insurance.Insurance_Phone_Type2;
                        _Insurances.Insurance_Phone_Type3 = insurance.Insurance_Phone_Type3;
                        _Insurances.Insurance_State = insurance.Insurance_State;
                        _Insurances.Insurance_Zip = insurance.Insurance_Zip;
                        _Insurances.Is_Sec_Attach_Need = insurance.Is_Sec_Attach_Need;
                        _Insurances.IS_SEC_Paper = insurance.IS_SEC_Paper;
                        _Insurances.STOP_SUBMISSION = insurance.STOP_SUBMISSION;
                        _Insurances.Sub_Method = insurance.Sub_Method;
                        _Insurances.Time_From = insurance.Time_From;
                        _Insurances.Time_To = insurance.Time_To;
                        _Insurances.Modified_By = userId;
                        _Insurances.Modified_Date = DateTime.Now;
                        ctx.SaveChanges();
                    }
                }
                else
                {
                    _Insurances = new Insurance();
                    long InsuranceId = Convert.ToInt64(ctx.SP_TableIdGenerator("Insurance_Id").FirstOrDefault().ToString());
                    _Insurances.Insurance_Id = InsuranceId;
                    _Insurances.AppealFilingLimit = insurance.AppealFilingLimit;
                    _Insurances.ClaimFilingLimit = insurance.ClaimFilingLimit;
                    _Insurances.EPSDT_WorkerInfo = insurance.EPSDT_WorkerInfo;
                    _Insurances.InActive = insurance.InActive;
                    _Insurances.InsPayer_Id = insurance.InsPayer_Id;
                    _Insurances.Insurance_Address = insurance.Insurance_Address;
                    _Insurances.Insurance_CardCategory = insurance.Insurance_CardCategory;
                    _Insurances.Insurance_City = insurance.Insurance_City;
                    _Insurances.Insurance_Department = insurance.Insurance_Department;
                    _Insurances.Insurance_Phone_Number1 = insurance.Insurance_Phone_Number1;
                    _Insurances.Insurance_Phone_Number2 = insurance.Insurance_Phone_Number2;
                    _Insurances.Insurance_Phone_Number3 = insurance.Insurance_Phone_Number3;
                    _Insurances.Insurance_Phone_Type1 = insurance.Insurance_Phone_Type1;
                    _Insurances.Insurance_Phone_Type2 = insurance.Insurance_Phone_Type2;
                    _Insurances.Insurance_Phone_Type3 = insurance.Insurance_Phone_Type3;
                    _Insurances.Insurance_State = insurance.Insurance_State;
                    _Insurances.Insurance_Zip = insurance.Insurance_Zip;
                    _Insurances.Is_Sec_Attach_Need = insurance.Is_Sec_Attach_Need;
                    _Insurances.IS_SEC_Paper = insurance.IS_SEC_Paper;
                    _Insurances.STOP_SUBMISSION = insurance.STOP_SUBMISSION;
                    _Insurances.Sub_Method = insurance.Sub_Method;
                    _Insurances.Time_From = insurance.Time_From;
                    _Insurances.Time_To = insurance.Time_To;
                    _Insurances.Deleted = false;
                    _Insurances.Created_By = userId;
                    _Insurances.Created_Date = DateTime.Now;

                    ctx.Insurances.Add(_Insurances);
                    ctx.SaveChanges();
                }

                if (_Insurances != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = _Insurances.Insurance_Id;
                }
                else
                {
                    objResponse.Status = "Error";
                }
            }

            return objResponse;
        }
        public ResponseModel DeleteInsurance(long InsuranceId, long InsurancePayerId)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                List<GetInsurancesByPayers_Result> _InsuranceList = new List<GetInsurancesByPayers_Result>();
                using (var ctx = new NPMDBEntities())
                {
                    Insurance objInsurance = ctx.Insurances.SingleOrDefault(c => c.Insurance_Id == InsuranceId);
                    Insurance_Payers insurance_Payers = ctx.Insurance_Payers.SingleOrDefault(c => c.Inspayer_Id == objInsurance.InsPayer_Id);
                    Insurance_Names insurance_Names = ctx.Insurance_Names.SingleOrDefault(c => c.Insname_Id == insurance_Payers.Insname_Id);
                    if (objInsurance != null)
                    {
                        objInsurance.Deleted = true;
                        insurance_Payers.Deleted = true;
                        insurance_Names.Deleted = true;
                        ctx.SaveChanges();
                    }
                    _InsuranceList = ctx.GetInsurancesByPayers(InsurancePayerId).ToList();
                }

                if (_InsuranceList != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = _InsuranceList;
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
        public ResponseModel GetInsuranceModel(long? insuranceId)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                InsurancesByPayersViewModel _Insurances = new InsurancesByPayersViewModel();
                using (var ctx = new NPMDBEntities())
                {
                    if (insuranceId != null)
                    {
                        var insurance = ctx.Insurances.FirstOrDefault(c => c.Insurance_Id == insuranceId && (c.Deleted ?? false) == false);
                        if (insurance != null)
                        {
                            _Insurances.Insurance_Id = insurance.Insurance_Id;
                            _Insurances.AppealFilingLimit = insurance.AppealFilingLimit;
                            _Insurances.ClaimFilingLimit = insurance.ClaimFilingLimit;
                            _Insurances.EPSDT_WOKERINFO = insurance.EPSDT_WorkerInfo;
                            _Insurances.InActive = insurance.InActive;
                            _Insurances.InsPayer_Id = insurance.InsPayer_Id;
                            _Insurances.Insurance_Address = insurance.Insurance_Address;
                            _Insurances.Insurance_CardCategory = insurance.Insurance_CardCategory;
                            _Insurances.Insurance_City = insurance.Insurance_City;
                            _Insurances.Insurance_Department = insurance.Insurance_Department;
                            _Insurances.Insurance_Phone_Number1 = insurance.Insurance_Phone_Number1;
                            _Insurances.Insurance_Phone_Number2 = insurance.Insurance_Phone_Number2;
                            _Insurances.Insurance_Phone_Number3 = insurance.Insurance_Phone_Number3;
                            _Insurances.Insurance_Phone_Type1 = insurance.Insurance_Phone_Type1;
                            _Insurances.Insurance_Phone_Type2 = insurance.Insurance_Phone_Type2;
                            _Insurances.Insurance_Phone_Type3 = insurance.Insurance_Phone_Type3;
                            _Insurances.Insurance_State = insurance.Insurance_State;
                            _Insurances.Insurance_Zip = insurance.Insurance_Zip;
                            _Insurances.Is_Sec_Attach_Need = insurance.Is_Sec_Attach_Need;
                            _Insurances.IS_SEC_Paper = insurance.IS_SEC_Paper;
                            _Insurances.STOP_SUBMISSION = insurance.STOP_SUBMISSION;
                            _Insurances.Sub_Method = insurance.Sub_Method;
                            _Insurances.Time_From = insurance.Time_From;
                            _Insurances.Time_To = insurance.Time_To;
                            _Insurances.InsurancePayer = ctx.Insurance_Payers.Where(i => i.Inspayer_Id == insurance.InsPayer_Id).Select(i => new SelectListViewModel()
                            {
                                Id = i.Inspayer_Id,
                                IdStr = i.Inspayer_Id + "",
                                Name = i.Inspayer_Description
                            }).FirstOrDefault();
                        }
                    }
                    _Insurances.PhoneTypesList = ctx.Phone_Types.Where(ig => (ig.DELETED == null || ig.DELETED == false)).Select(svm => new SelectListViewModel { Id = svm.PHONE_CODE, Name = svm.PHONE_TYPE }).OrderBy(t => t.Name).ToList();
                    _Insurances.InsuraceDepartmentList = ctx.Insurance_Departments.Select(svm => new SelectListViewModel { Id = svm.PkInsDept_Id, Name = svm.InsDept_Description }).OrderBy(t => t.Name).ToList();
                    _Insurances.InsuraceCardCategoryList = ctx.InsCardCategories.Select(svm => new SelectListViewModel { Id = svm.InsCardCatID, Name = svm.Insurance_cardCategory }).OrderBy(t => t.Name).ToList();
                    _Insurances.SubmissionMethodList = ctx.SubmissionMethods.Select(svm => new SelectListViewModel { Id = svm.SubMthdID, Name = svm.SUBMETHOD }).OrderBy(t => t.Name).ToList();
                    _Insurances.EPSDTWorkerInfoList = ctx.EPSDTWORKERINFOes.Select(svm => new SelectListViewModel { Id = svm.EPSDTWRKRINFOID, Name = svm.DESCRIPTION }).OrderBy(t => t.Name).ToList();
                }

                if (_Insurances != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = _Insurances;
                }
                else
                {
                    objResponse.Status = "Error";
                }
            }
            catch (Exception ex)
            {
                objResponse.Status = "Error";
            }
            return objResponse;
        }
        #endregion Insurance
        public ResponseModel GetRelationsSelectList()
        {
            ResponseModel resp = new ResponseModel();
            List<SelectListViewModel> RelationshipList = new List<SelectListViewModel>();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    RelationshipList = ctx.Relationships.Where(r => r.Deleted == null || r.Deleted == false).Select(r => new SelectListViewModel()
                    {
                        Id = r.RelationshipId,
                        Name = r.Name,
                        IdStr = r.RelationshipShortCode

                    }).OrderBy(t => t.Name).ToList();
                    if (RelationshipList != null)
                    {
                        resp.Status = "Success";
                        resp.Response = RelationshipList;
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



        //public ResponseModel SaveInsuranceGroup([FromBody] InsuranceGroupViewModel Model, long userId)
        //{
        //    throw new NotImplementedException();
        //}
        #region InsuranceRewamp
        //public ResponseModel AddInsurance(AddInsuranceRequest addInsuranceRequest,long userId)
        //{
        //    ResponseModel objResponse = new ResponseModel();
        //    Insurance_Names objModel = null;
        //    Insurance_Payers _InsurancePayer = new Insurance_Payers();
        //    var namemodel = addInsuranceRequest.insuranceNameModelViewModel;
        //    var insPayer = addInsuranceRequest.insurancePayerViewModel;
        //    using (var ctx = new NPMDBEntities())
        //    {
        //        if (namemodel.Insname_Id != 0)
        //        {
        //            objModel = ctx.Insurance_Names.SingleOrDefault(p => p.Insname_Id == namemodel.Insname_Id);
        //            if (objModel != null)
        //            {
        //                objModel.Insgroup_Id = namemodel.Insgroup_Id;
        //                objModel.Insname_Description = namemodel.Insname_Description;
        //                objModel.Modified_By = userId;
        //                objModel.Modified_Date = DateTime.Now;
        //                ctx.SaveChanges();
        //            }
        //        }
        //        else
        //        {
        //            objModel = new Insurance_Names();
        //            long InsNameId = Convert.ToInt64(ctx.SP_TableIdGenerator("Insname_Id").FirstOrDefault().ToString());
        //            objModel.Insname_Id = InsNameId;
        //            objModel.Insgroup_Id = namemodel.Insgroup_Id;
        //            objModel.Insname_Description = namemodel.Insname_Description;
        //            objModel.Deleted = false;
        //            objModel.Created_By = userId;
        //            objModel.Created_Date = DateTime.Now;
        //            ctx.Insurance_Names.Add(objModel);
        //            ctx.SaveChanges();
        //        }
        //        insPayer.Insname_Id = namemodel.Insname_Id;
        //        if (insPayer.Inspayer_Id != 0)
        //        {
        //            _InsurancePayer = ctx.Insurance_Payers.SingleOrDefault(p => p.Inspayer_Id == insPayer.Inspayer_Id);
        //            if (_InsurancePayer != null)
        //            {
        //                _InsurancePayer.Acknowledgement_Type = insPayer.Acknowledgement_Type;
        //                _InsurancePayer.Edisetup_Required = insPayer.Edisetup_Required;
        //                _InsurancePayer.EFTSETUPREQUIRED = insPayer.EFTSETUPREQUIRED;
        //                _InsurancePayer.Electronic_Corrected_Claims = insPayer.Electronic_Corrected_Claims;
        //                _InsurancePayer.Electronic_Late_Filing = insPayer.Electronic_Late_Filing;
        //                _InsurancePayer.Erasetup_Required = insPayer.Erasetup_Required;
        //                _InsurancePayer.Insname_Id = insPayer.Insname_Id;
        //                _InsurancePayer.Inspayer_835_Id = insPayer.Inspayer_835_Id;
        //                _InsurancePayer.Inspayer_837_Id = insPayer.Inspayer_837_Id;
        //                _InsurancePayer.InsPayer_Claim_Status_Id = insPayer.InsPayer_Claim_Status_Id;
        //                _InsurancePayer.Inspayer_Description = insPayer.Inspayer_Description;
        //                _InsurancePayer.InsPayer_Description_old = insPayer.InsPayer_Description_old;
        //                _InsurancePayer.InsPayer_Eligibility_Id = insPayer.InsPayer_Eligibility_Id;
        //                _InsurancePayer.Inspayer_Id = insPayer.Inspayer_Id;
        //                _InsurancePayer.Inspayer_Plan = insPayer.Inspayer_Plan;
        //                _InsurancePayer.Inspayer_Referral_Id = insPayer.Inspayer_Referral_Id;
        //                _InsurancePayer.Inspayer_State = insPayer.Inspayer_State;
        //                _InsurancePayer.Is_Nonpar_CS = insPayer.Is_Nonpar_CS;
        //                _InsurancePayer.Is_Nonpar_Era = insPayer.Is_Nonpar_Era;
        //                _InsurancePayer.Is_Part_A = insPayer.Is_Part_A;
        //                _InsurancePayer.Is_RTA_Payer = insPayer.Is_RTA_Payer;
        //                _InsurancePayer.Is_Sec_Paper = insPayer.Is_Sec_Paper;
        //                _InsurancePayer.Ivr_Server_Id = insPayer.Ivr_Server_Id;
        //                _InsurancePayer.MU_Category = insPayer.MU_Category;
        //                _InsurancePayer.Npi_Type = insPayer.Npi_Type;
        //                _InsurancePayer.Restricted_Calls = insPayer.Restricted_Calls;
        //                _InsurancePayer.SERVER_ID = insPayer.SERVER_ID;
        //                _InsurancePayer.Submission_type = insPayer.Submission_type;
        //                _InsurancePayer.Timely_Filing_Days = insPayer.Timely_Filing_Days;
        //                _InsurancePayer.Modified_By = userId;
        //                _InsurancePayer.Modified_Date = DateTime.Now;
        //                ctx.SaveChanges();
        //            }
        //        }
        //        else
        //        {
        //            _InsurancePayer = new Insurance_Payers();
        //            long InsPayerId = Convert.ToInt64(ctx.SP_TableIdGenerator("Inspayer_Id").FirstOrDefault().ToString());
        //            _InsurancePayer.Inspayer_Id = InsPayerId;
        //            _InsurancePayer.Acknowledgement_Type = insPayer.Acknowledgement_Type;
        //            _InsurancePayer.Edisetup_Required = insPayer.Edisetup_Required;
        //            _InsurancePayer.EFTSETUPREQUIRED = insPayer.EFTSETUPREQUIRED;
        //            _InsurancePayer.Electronic_Corrected_Claims = insPayer.Electronic_Corrected_Claims;
        //            _InsurancePayer.Electronic_Late_Filing = insPayer.Electronic_Late_Filing;
        //            _InsurancePayer.Erasetup_Required = insPayer.Erasetup_Required;
        //            _InsurancePayer.Insname_Id = insPayer.Insname_Id;
        //            _InsurancePayer.Inspayer_835_Id = insPayer.Inspayer_835_Id;
        //            _InsurancePayer.Inspayer_837_Id = insPayer.Inspayer_837_Id;
        //            _InsurancePayer.InsPayer_Claim_Status_Id = insPayer.InsPayer_Claim_Status_Id;
        //            _InsurancePayer.Inspayer_Description = insPayer.Inspayer_Description;
        //            _InsurancePayer.InsPayer_Description_old = insPayer.InsPayer_Description_old;
        //            _InsurancePayer.InsPayer_Eligibility_Id = insPayer.InsPayer_Eligibility_Id;
        //            _InsurancePayer.Inspayer_Plan = insPayer.Inspayer_Plan;
        //            _InsurancePayer.Inspayer_Referral_Id = insPayer.Inspayer_Referral_Id;
        //            _InsurancePayer.Inspayer_State = insPayer.Inspayer_State;
        //            _InsurancePayer.Is_Nonpar_CS = insPayer.Is_Nonpar_CS;
        //            _InsurancePayer.Is_Nonpar_Era = insPayer.Is_Nonpar_Era;
        //            _InsurancePayer.Is_Part_A = insPayer.Is_Part_A;
        //            _InsurancePayer.Is_RTA_Payer = insPayer.Is_RTA_Payer;
        //            _InsurancePayer.Is_Sec_Paper = insPayer.Is_Sec_Paper;
        //            _InsurancePayer.Ivr_Server_Id = insPayer.Ivr_Server_Id;
        //            _InsurancePayer.MU_Category = insPayer.MU_Category;
        //            _InsurancePayer.Npi_Type = insPayer.Npi_Type;
        //            _InsurancePayer.Restricted_Calls = insPayer.Restricted_Calls;
        //            _InsurancePayer.SERVER_ID = insPayer.SERVER_ID;
        //            _InsurancePayer.Submission_type = insPayer.Submission_type;
        //            _InsurancePayer.Timely_Filing_Days = insPayer.Timely_Filing_Days;
        //            _InsurancePayer.Deleted = false;
        //            _InsurancePayer.Created_By = userId;
        //            _InsurancePayer.Created_Date = DateTime.Now;

        //            ctx.Insurance_Payers.Add(_InsurancePayer);
        //            ctx.SaveChanges();
        //        }
        //        if (namemodel != null)
        //        {
        //            objResponse.Status = "Sucess";
        //            objResponse.Response = namemodel.Insname_Id;
        //        }
        //        else
        //        {
        //            objResponse.Status = "Error";
        //        }
        //    }

        //    return objResponse;
        //}
        public ResponseModel AddInsurance(AddInsuranceRequest addInsuranceRequest, long userId)
        {
            ResponseModel objResponse = new ResponseModel();
            Insurance_Names objModel = new Insurance_Names();
            Insurance_Payers _InsurancePayer = new Insurance_Payers();
            Insurance _Insurances = new Insurance();

            var namemodel = addInsuranceRequest.insuranceNameModelViewModel;
            var insPayer = addInsuranceRequest.insurancePayerViewModel;
            var insurance = addInsuranceRequest.Insurance;
          //  var InsuranceTypeCode = addInsuranceRequest.InsuranceTypeCodeModel.InsuranceTypeCode;

            using (var ctx = new NPMDBEntities())
            {
                using (var transaction = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        // Handle Insurance_Names logic
                        if (namemodel.Insname_Id != 0)
                        {
                            objModel = ctx.Insurance_Names.SingleOrDefault(p => p.Insname_Id == namemodel.Insname_Id);
                            if (objModel != null)
                            {
                                objModel.Insgroup_Id = namemodel.Insgroup_Id;
                                objModel.Insname_Description = namemodel.Insname_Description;
                                objModel.Modified_By = userId;
                                objModel.Modified_Date = DateTime.Now;
                                ctx.SaveChanges();
                            }
                        }
                        else
                        {
                            long InsNameId = Convert.ToInt64(ctx.SP_TableIdGenerator("Insname_Id").FirstOrDefault().ToString());
                            objModel.Insname_Id = InsNameId;
                            objModel.Insgroup_Id = namemodel.Insgroup_Id;
                            objModel.Insname_Description = namemodel.Insname_Description;
                            objModel.Deleted = false;
                            objModel.Created_By = userId;
                            objModel.Created_Date = DateTime.Now;
                            ctx.Insurance_Names.Add(objModel);
                            ctx.SaveChanges();
                        }

                        // Handle Insurance_Payers logic
                        insPayer.Insname_Id = objModel.Insname_Id;
                        if (insPayer.Inspayer_Id != 0)
                        {
                            _InsurancePayer = ctx.Insurance_Payers.SingleOrDefault(p => p.Inspayer_Id == insPayer.Inspayer_Id);
                            if (_InsurancePayer != null)
                            {
                                UpdateInsurancePayer(_InsurancePayer, insPayer, userId);
                                ctx.SaveChanges();
                            }
                        }
                        else
                        {
                            _InsurancePayer = new Insurance_Payers();
                            long InsPayerId = Convert.ToInt64(ctx.SP_TableIdGenerator("Inspayer_Id").FirstOrDefault().ToString());
                            _InsurancePayer.Inspayer_Id = InsPayerId;
                            PopulateInsurancePayer(_InsurancePayer, insPayer, userId);
                            ctx.Insurance_Payers.Add(_InsurancePayer);
                            ctx.SaveChanges();
                        }

                        // Handle Insurance logic
                        insurance.InsPayer_Id = _InsurancePayer.Inspayer_Id;
                        if (insurance.Insurance_Id != 0)
                        {
                            _Insurances = ctx.Insurances.SingleOrDefault(p => p.Insurance_Id == insurance.Insurance_Id);
                            if (_Insurances != null)
                            {
                                if (insurance.Deleted != null)
                                    _Insurances.Deleted = (bool)insurance.Deleted;

                                _Insurances.AppealFilingLimit = insurance.AppealFilingLimit;
                                _Insurances.ClaimFilingLimit = insurance.ClaimFilingLimit;
                                _Insurances.EPSDT_WorkerInfo = insurance.EPSDT_WorkerInfo;
                                _Insurances.InActive = insurance.InActive;
                                _Insurances.InsPayer_Id = insurance.InsPayer_Id;
                                _Insurances.Insurance_Address = insurance.Insurance_Address;
                                _Insurances.Insurance_CardCategory = insurance.Insurance_CardCategory;
                                _Insurances.Insurance_City = insurance.Insurance_City;
                                _Insurances.Insurance_Department = insurance.Insurance_Department;
                                _Insurances.Insurance_Phone_Number1 = insurance.Insurance_Phone_Number1;
                                _Insurances.Insurance_Phone_Number2 = insurance.Insurance_Phone_Number2;
                                _Insurances.Insurance_Phone_Number3 = insurance.Insurance_Phone_Number3;
                                _Insurances.Insurance_Phone_Type1 = insurance.Insurance_Phone_Type1;
                                _Insurances.Insurance_Phone_Type2 = insurance.Insurance_Phone_Type2;
                                _Insurances.Insurance_Phone_Type3 = insurance.Insurance_Phone_Type3;
                                _Insurances.Insurance_State = insurance.Insurance_State;
                                _Insurances.Insurance_Zip = insurance.Insurance_Zip;
                                _Insurances.Is_Sec_Attach_Need = insurance.Is_Sec_Attach_Need;
                                _Insurances.IS_SEC_Paper = insurance.IS_SEC_Paper;
                                _Insurances.STOP_SUBMISSION = insurance.STOP_SUBMISSION;
                                _Insurances.Sub_Method = insurance.Sub_Method;
                                _Insurances.Time_From = insurance.Time_From;
                                _Insurances.Time_To = insurance.Time_To;
                                _Insurances.Modified_By = userId;
                                _Insurances.Modified_Date = DateTime.Now;
                                ctx.SaveChanges();

                            }
                        }
                        else
                        {

                            _Insurances = new Insurance();
                            long InsuranceId = Convert.ToInt64(ctx.SP_TableIdGenerator("Insurance_Id").FirstOrDefault().ToString());
                            _Insurances.Insurance_Id = InsuranceId;
                            _Insurances.AppealFilingLimit = insurance.AppealFilingLimit;
                            _Insurances.ClaimFilingLimit = insurance.ClaimFilingLimit;
                            _Insurances.EPSDT_WorkerInfo = insurance.EPSDT_WorkerInfo;
                            _Insurances.InActive = insurance.InActive;
                            _Insurances.InsPayer_Id = insurance.InsPayer_Id;
                            _Insurances.Insurance_Address = insurance.Insurance_Address;
                            _Insurances.Insurance_CardCategory = insurance.Insurance_CardCategory;
                            _Insurances.Insurance_City = insurance.Insurance_City;
                            _Insurances.Insurance_Department = insurance.Insurance_Department;
                            _Insurances.Insurance_Phone_Number1 = insurance.Insurance_Phone_Number1;
                            _Insurances.Insurance_Phone_Number2 = insurance.Insurance_Phone_Number2;
                            _Insurances.Insurance_Phone_Number3 = insurance.Insurance_Phone_Number3;
                            _Insurances.Insurance_Phone_Type1 = insurance.Insurance_Phone_Type1;
                            _Insurances.Insurance_Phone_Type2 = insurance.Insurance_Phone_Type2;
                            _Insurances.Insurance_Phone_Type3 = insurance.Insurance_Phone_Type3;
                            _Insurances.Insurance_State = insurance.Insurance_State;
                            _Insurances.Insurance_Zip = insurance.Insurance_Zip;
                            _Insurances.Is_Sec_Attach_Need = insurance.Is_Sec_Attach_Need;
                            _Insurances.IS_SEC_Paper = insurance.IS_SEC_Paper;
                            _Insurances.STOP_SUBMISSION = insurance.STOP_SUBMISSION;
                            _Insurances.Sub_Method = insurance.Sub_Method;
                            _Insurances.Time_From = insurance.Time_From;
                            _Insurances.Time_To = insurance.Time_To;
                            _Insurances.Deleted = false;
                            _Insurances.Created_By = userId;
                            _Insurances.Created_Date = DateTime.Now;

                            ctx.Insurances.Add(_Insurances);
                            ctx.SaveChanges();

                        }

                        //// Add data to insurancelist table
                        //var insuranceListEntry = new insurancelist();


                        //insuranceListEntry.Insgroup_Name = namemodel.Insname_Description.ToString(); // Adjust based on actual logic
                        //insuranceListEntry.inspayer_id = _InsurancePayer.Inspayer_Id;
                        //insuranceListEntry.inspayer_description = _InsurancePayer.Inspayer_Description;
                        //insuranceListEntry.inspayer_837_id = _InsurancePayer.Inspayer_837_Id;
                        //insuranceListEntry.inspayer_state = _InsurancePayer.Inspayer_State;
                        //insuranceListEntry.insurance_id = _Insurances.Insurance_Id;
                        //insuranceListEntry.insurance_address = _Insurances.Insurance_Address;
                        //insuranceListEntry.insurance_state = _Insurances.Insurance_State;
                        //insuranceListEntry.insurance_type_code =insPayer.Insurance_Type_Code;

                        //ctx.insurancelists.Add(insuranceListEntry);
                        //ctx.SaveChanges();
                        // Check if an existing entry in the insurancelist table matches the current insurance
                        var existingInsuranceListEntry = ctx.insurancelists.SingleOrDefault(il => il.insurance_id == _Insurances.Insurance_Id);
                        var groupName = ctx.Insurance_Groups.FirstOrDefault(g => g.Insgroup_Id == addInsuranceRequest.insuranceNameModelViewModel.Insgroup_Id).Insgroup_Name;

                        if (existingInsuranceListEntry != null)
                        {
                            // Update the existing entry
                            existingInsuranceListEntry.Insgroup_Name = groupName.ToString(); // Adjust based on actual logic
                            existingInsuranceListEntry.inspayer_id = _InsurancePayer.Inspayer_Id;
                            existingInsuranceListEntry.inspayer_description = _InsurancePayer.Inspayer_Description;
                            existingInsuranceListEntry.inspayer_837_id = _InsurancePayer.Inspayer_837_Id;
                            existingInsuranceListEntry.inspayer_state = _InsurancePayer.Inspayer_State;
                            existingInsuranceListEntry.insurance_address = _Insurances.Insurance_Address;
                            existingInsuranceListEntry.insurance_state = _Insurances.Insurance_State;
                            existingInsuranceListEntry.insurance_type_code = insPayer.Insurance_Type_Code;
                        }
                        else
                        {
                            // Add a new entry if no matching record exists
                            var insuranceListEntry = new insurancelist
                            {
                                Insgroup_Name = groupName.ToString(), // Adjust based on actual logic
                                inspayer_id = _InsurancePayer.Inspayer_Id,
                                inspayer_description = _InsurancePayer.Inspayer_Description,
                                inspayer_837_id = _InsurancePayer.Inspayer_837_Id,
                                inspayer_state = _InsurancePayer.Inspayer_State,
                                insurance_id = _Insurances.Insurance_Id,
                                insurance_address = _Insurances.Insurance_Address,
                                insurance_state = _Insurances.Insurance_State,
                                insurance_type_code = insPayer.Insurance_Type_Code
                            };

                            ctx.insurancelists.Add(insuranceListEntry);
                        }

                        // Save changes to the database
                        ctx.SaveChanges();

                        // Commit transaction if all operations succeed
                        transaction.Commit();

                        // Prepare response
                        if (namemodel != null)
                        {
                            objResponse.Status = "Success";
                            objResponse.Response = namemodel.Insname_Id;
                        }
                        else
                        {
                            objResponse.Status = "Error";
                        }
                    }
                    catch (Exception ex)
                    {
                        // Rollback transaction on error
                        transaction.Rollback();
                        objResponse.Status = "Error";
                        objResponse.Response = ex.Message; // Log exception message for debugging
                    }
                }
            }

            return objResponse;
        }


        // Helper method to update existing InsurancePayer
        private void UpdateInsurancePayer(Insurance_Payers target, InsurancePayerViewModel source, long userId)
        {
            target.Acknowledgement_Type = source.Acknowledgement_Type;
            target.Edisetup_Required = source.Edisetup_Required;
            target.EFTSETUPREQUIRED = source.EFTSETUPREQUIRED;
            target.Electronic_Corrected_Claims = source.Electronic_Corrected_Claims;
            target.Electronic_Late_Filing = source.Electronic_Late_Filing;
            target.Erasetup_Required = source.Erasetup_Required;
            target.Insname_Id = source.Insname_Id;
            target.Inspayer_835_Id = source.Inspayer_835_Id;
            target.Inspayer_837_Id = source.Inspayer_837_Id;
            target.InsPayer_Claim_Status_Id = source.InsPayer_Claim_Status_Id;
            target.Inspayer_Description = source.Inspayer_Description;
            target.InsPayer_Description_old = source.InsPayer_Description_old;
            target.InsPayer_Eligibility_Id = source.InsPayer_Eligibility_Id;
            target.Inspayer_Plan = source.Inspayer_Plan;
            target.Inspayer_Referral_Id = source.Inspayer_Referral_Id;
            target.Inspayer_State = source.Inspayer_State;
            target.Is_Nonpar_CS = source.Is_Nonpar_CS;
            target.Is_Nonpar_Era = source.Is_Nonpar_Era;
            target.Is_Part_A = source.Is_Part_A;
            target.Is_RTA_Payer = source.Is_RTA_Payer;
            target.Is_Sec_Paper = source.Is_Sec_Paper;
            target.Ivr_Server_Id = source.Ivr_Server_Id;
            target.MU_Category = source.MU_Category;
            target.Npi_Type = source.Npi_Type;
            target.Restricted_Calls = source.Restricted_Calls;
            target.SERVER_ID = source.SERVER_ID;
            target.Submission_type = source.Submission_type;
            target.Timely_Filing_Days = source.Timely_Filing_Days;
            target.Modified_By = userId;
            target.Modified_Date = DateTime.Now;
        }

        // Helper method to populate new InsurancePayer
        private void PopulateInsurancePayer(Insurance_Payers target, InsurancePayerViewModel source, long userId)
        {
            UpdateInsurancePayer(target, source, userId);
            target.Deleted = false;
            target.Created_By = userId;
            target.Created_Date = DateTime.Now;
        }

        #endregion InsuranceRewamp

    }
}