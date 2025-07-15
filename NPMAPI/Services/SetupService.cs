using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using EdiFabric.Core.Model.Edi.X12;
using EdiFabric.Templates.Hipaa5010;
using NPMAPI.Models;
using NPMAPI.Models.InboxHealth;
using NPMAPI.Models.ViewModels;
using NPMAPI.Repositories;
using NPOI.OpenXmlFormats.Wordprocessing;

namespace NPMAPI.Services
{
    // [RoutePrefix("api/SetupForms")]
    public class SetupService : ISetupRepository
    {
        public ResponseModel DeleteFacility(long FacilityId)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                List<Facility> FacilitiesList = null;
                using (var ctx = new NPMDBEntities())
                {

                    Facility objFacility = ctx.Facilities.SingleOrDefault(c => c.Facility_Code == FacilityId);
                    if (objFacility != null)
                    {
                        objFacility.Deleted = true;
                        ctx.SaveChanges();
                    }

                    FacilitiesList = ctx.Facilities.Where(c => c.Deleted == null || c.Deleted == false).ToList();
                    if (FacilitiesList != null)
                    {
                        objResponse.Status = "Sucess";
                        objResponse.Response = FacilitiesList;
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

        public ResponseModel GetFacility(long FacilityId)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                Facility _facility = null;
                using (var ctx = new NPMDBEntities())
                {
                    _facility = ctx.Facilities.SingleOrDefault(c => c.Facility_Code == FacilityId);
                }

                if (_facility != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = _facility;
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

        public ResponseModel GetFacilityList()
        {
            ResponseModel objResponse = new ResponseModel();
            List<Facility> objFacilitiesList = null;
            using (var ctx = new NPMDBEntities())
            {
                objFacilitiesList = ctx.Facilities.Where(f => f.Deleted == null || f.Deleted == false).ToList();
            }

            if (objFacilitiesList != null)
            {
                objResponse.Status = "Sucess";
                objResponse.Response = objFacilitiesList;
            }
            else
            {
                objResponse.Status = "Error";
            }
            return objResponse;
        }

        public ResponseModel SaveFacility([FromBody] Facility Model, long userId)
        {
            ResponseModel objResponse = new ResponseModel();
            Facility objModel = null;

            try
            {


                using (var ctx = new NPMDBEntities())
                {
                    if (Model.Facility_Code != 0)
                    {
                        objModel = ctx.Facilities.SingleOrDefault(p => p.Facility_Code == Model.Facility_Code);
                        if (objModel != null)
                        {
                            objModel.Stop_In_Submission = Model.Stop_In_Submission;
                            objModel.NPI = Model.NPI;
                            objModel.Facility_ZIP = Model.Facility_ZIP;
                            objModel.Facility_Type = Model.Facility_Type;
                            objModel.Facility_State = Model.Facility_State;
                            objModel.Facility_Phone = Model.Facility_Phone;
                            Model.Facility_Phone = Model.Facility_Phone.Replace("(", "").Replace(")", "").Replace(" ", "").Replace("-", "");
                            objModel.Facility_Name = Model.Facility_Name;
                            objModel.Facility_Id_Number = Model.Facility_Id_Number;
                            objModel.Facility_Contact_Name = Model.Facility_Contact_Name;
                            objModel.Facility_City = Model.Facility_City;
                            objModel.Facility_Address = Model.Facility_Address;
                            objModel.Modified_Date = DateTime.Now;
                            objModel.Modified_By = userId;
                            objModel.Practice_Code = Model.Practice_Code;
                            ctx.SaveChanges();
                        }
                    }
                    else
                    {
                        long facilityId = Convert.ToInt64(ctx.SP_TableIdGenerator("Facility_Code").FirstOrDefault().ToString());
                        Model.Facility_Code = facilityId;
                        Model.Facility_Phone = Model.Facility_Phone.Replace("(", "").Replace(")", "").Replace(" ", "").Replace("-", "");
                        Model.Deleted = false;
                        Model.Created_Date = DateTime.Now;
                        Model.Created_By = userId;

                        ctx.Facilities.Add(Model);
                        ctx.SaveChanges();
                    }

                    if (Model != null)
                    {
                        objResponse.Status = "Sucess";
                        objResponse.Response = Model.Facility_Code;
                    }
                    else
                    {
                        objResponse.Status = "Error";
                    }
                }

                return objResponse;
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
                throw;
            }
        }

        #region Gurantor
        public ResponseModel SaveGurantor([FromBody] Guarantor Model, long userId)
        {
            ResponseModel objResponse = new ResponseModel();
            Guarantor objModel = null;

            using (var ctx = new NPMDBEntities())
            {
                if (Model.Guarantor_Code != 0)
                {
                    objModel = ctx.Guarantors.SingleOrDefault(p => p.Guarantor_Code == Model.Guarantor_Code);
                    if (objModel != null)
                    {
                        objModel.Exported = Model.Exported;
                        objModel.Guarant_Address = Model.Guarant_Address;
                        objModel.Guarant_City = Model.Guarant_City;
                        objModel.Guarant_Dob = Model.Guarant_Dob;
                        objModel.Guarant_Fname = Model.Guarant_Fname;
                        objModel.Guarant_Gender = Model.Guarant_Gender;
                        objModel.Guarant_Home_Phone = Model.Guarant_Home_Phone;

                        objModel.Guarant_Lname = Model.Guarant_Lname;
                        objModel.Guarant_Mi = Model.Guarant_Mi;
                        objModel.Guarant_Ssn = Model.Guarant_Ssn;
                        objModel.Guarant_State = Model.Guarant_State;
                        objModel.Guarant_Type = Model.Guarant_Type;

                        objModel.GUARANT_Work_Phone = Model.GUARANT_Work_Phone;
                        objModel.GUARANT_Work_Phone_Ext = Model.GUARANT_Work_Phone_Ext;
                        objModel.Guarant_Zip = Model.Guarant_Zip;
                        objModel.modified_date = DateTime.Now;
                        objModel.modified_by = userId;
                        ctx.SaveChanges();
                    }
                }
                else
                {
                    objModel = new Guarantor();
                    long gurantorId = Convert.ToInt64(ctx.SP_TableIdGenerator("Guarantor_Code").FirstOrDefault().ToString());
                    Model.Guarantor_Code = gurantorId;
                    Model.Deleted = false;
                    Model.created_by = userId;
                    Model.created_date = DateTime.Now;
                    ctx.Guarantors.Add(Model);
                    ctx.SaveChanges();
                }

                if (Model != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = Model;
                }
                else
                {
                    objResponse.Status = "Error";
                }
            }

            return objResponse;
        }
        public ResponseModel SaveNDC([FromBody] NDCModel Model, long userId)
        {
            ResponseModel objResponse = new ResponseModel();

            try
            {
                //mychanges
                NDC_CrossWalk storeData = null;

                using (var ctx = new NPMDBEntities())
                {   

                    if (Model.NDC_ID != 0)
                    {
                        storeData = new NDC_CrossWalk();
                        storeData = ctx.NDC_CrossWalk.SingleOrDefault(p => p.NDC_ID == Model.NDC_ID);
                        if (storeData != null)
                        {
                            storeData.NDC2 = Model.ndc_code;
                            storeData.Practice_Code = Model.practice_code;
                            storeData.Qualifier = Model.qualifer;
                            storeData.HCPCS_Code = Model.HCPCS_code;
                            storeData.Drug_Name = Model.drug_name;
                            storeData.Labeler_Name = Model.labeler_name;
                            storeData.PKG_Qty = Model.PKG_Qty;
                            storeData.Effective_Date_From = Model.effectivefrom;
                            storeData.Effective_Date_To = Model.effectiveto;
                            storeData.Short_Description = Model.description;
                            storeData.Modified_Date = DateTime.Now;
                            storeData.Modified_By = userId.ToString();
                            ctx.SaveChanges();
                            objResponse.Status = "Sucess";
                            objResponse.Response = Model;
                        }

                    }


                    else
                    {
                        storeData = new NDC_CrossWalk();
                        long NDC_ID = Convert.ToInt64(ctx.SP_TableIdGenerator("NDC_ID").FirstOrDefault().ToString());

                        storeData.NDC_ID = NDC_ID;
                        storeData.NDC2 = Model.ndc_code;
                        storeData.Practice_Code = Model.practice_code;
                        storeData.Qualifier = Model.qualifer;
                        storeData.HCPCS_Code = Model.HCPCS_code;
                        storeData.Drug_Name = Model.drug_name;
                        storeData.Labeler_Name = Model.labeler_name;
                        storeData.PKG_Qty = Model.PKG_Qty;
                        storeData.Effective_Date_From = Model.effectivefrom;
                        storeData.Effective_Date_To = Model.effectiveto;
                        storeData.Short_Description = Model.description;
                        storeData.Created_Date = DateTime.Now;
                        storeData.Deleted = false;

                        storeData.Created_By = userId.ToString();


                        ctx.NDC_CrossWalk.Add(storeData);
                        ctx.SaveChanges();
                        objResponse.Status = "Sucess";
                        objResponse.Response = Model;
                    }


                }

                return objResponse;
            }
            catch (Exception ex)
            {

                objResponse.Status = "Error";
                throw;
            }
        }
        public ResponseModel SaveDX([FromBody] Diagnosi Model, long userId)
        {
            ResponseModel objResponse = new ResponseModel();

            try
            {

                Diagnosi storeData = null;

                using (var ctx = new NPMDBEntities())
                {
   
                 storeData = new Diagnosi();
                        storeData = ctx.Diagnosis.SingleOrDefault(p => p.Diag_Code == Model.Diag_Code);
                        if (storeData == null)
                        {
                            Model.Is_Active= true;
                            Model.Deleted= false;
                            Model.Created_Date = DateTime.Now;
                            Model.Created_By = userId; 
                            ctx.Diagnosis.Add(Model);
                            ctx.SaveChanges();
                            objResponse.Status = "Sucess";
                            objResponse.Response = Model;
                    }
                    else
                    {
                        objResponse.Status = "duplicate";
                       

                    }

                    


                  


                }

                return objResponse;
            }
            catch (Exception ex)
            {

                objResponse.Status = "Error";
                throw;
            }

        }
        public ResponseModel UpdateDX([FromBody] Diagnosi Model, long userId)
        {
            ResponseModel objResponse = new ResponseModel();

            try
            {

                Diagnosi storeData = null;

                using (var ctx = new NPMDBEntities())
                {

                    storeData = new Diagnosi();
                    storeData = ctx.Diagnosis.SingleOrDefault(p => p.Diag_Code == Model.Diag_Code);
                    if (storeData != null)
                    {
                        storeData.ICD_version = Model.ICD_version;
                        storeData.Diag_Description = Model.Diag_Description;
                        storeData.Diag_Effective_Date = Model.Diag_Effective_Date;
                        storeData.Diag_Expiry_Date = Model.Diag_Expiry_Date;
                        storeData.Gender_Applied_On= Model.Gender_Applied_On;
                        storeData.Is_Active = true;
                        storeData.Deleted = false;
                        storeData.Modified_Date = DateTime.Now;
                        storeData.Modified_By = userId;
                        ctx.SaveChanges();
                        objResponse.Status = "Sucess";
                        objResponse.Response = Model;
                    }
                    else
                    {
                        objResponse.Status = "Error";


                    }







                }

                return objResponse;
            }
            catch (Exception ex)
            {

                objResponse.Status = "Error";
                throw;
            }

        }




        public ResponseModel DeleteGurantor(long GurantorId)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                List<Guarantor> GuarantorList = null;
                using (var ctx = new NPMDBEntities())

                {
                    

                    Guarantor objGuarantor = ctx.Guarantors.SingleOrDefault(c => c.Guarantor_Code == GurantorId);
                    if (objGuarantor != null)
                    {
                        objGuarantor.Deleted = true;
                        ctx.SaveChanges();
                    }

                    GuarantorList = ctx.Guarantors.Where(c => (c.Deleted == null || c.Deleted == false)).ToList();
                    if (GuarantorList != null)
                    {
                        objResponse.Status = "Sucess";
                        objResponse.Response = GuarantorList;
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
        public ResponseModel DeleteNDC(long NDC_ID)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                List<NDC_CrossWalk> GetNDCList = null;
                using (var ctx = new NPMDBEntities())

                {


                    NDC_CrossWalk objNDC = ctx.NDC_CrossWalk.SingleOrDefault(c => c.NDC_ID == NDC_ID);
                    if (objNDC != null)
                    {
                        objNDC.Deleted = true;
                        ctx.SaveChanges();
                        objResponse.Status = "Sucess";
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
        public ResponseModel DeleteDX(string DX_code)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                List<Diagnosi> GetDXList = null;
                using (var ctx = new NPMDBEntities())

                {


                    Diagnosi objDX = ctx.Diagnosis.SingleOrDefault(c => c.Diag_Code == DX_code);
                    if (objDX != null)
                    {
                        objDX.Deleted = true;
                        ctx.SaveChanges();
                        objResponse.Status = "Sucess";
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
        public ResponseModel GetGurantor(long GurantorId)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                Guarantor _gurantor = null;
                using (var ctx = new NPMDBEntities())
                {
                    _gurantor = ctx.Guarantors.SingleOrDefault(c => c.Guarantor_Code == GurantorId);
                }

                if (_gurantor != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = _gurantor;
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
        public ResponseModel GetGurantorsList()
        {
            ResponseModel objResponse = new ResponseModel();
            List<Guarantor> objGurantorList = null;
            using (var ctx = new NPMDBEntities())
            {
                objGurantorList = ctx.Guarantors.Where(c => (c.Deleted == null || c.Deleted == false)).ToList();
            }

            if (objGurantorList != null)
            {
                objResponse.Status = "Sucess";
                objResponse.Response = objGurantorList;
            }
            else
            {
                objResponse.Status = "Error";
            }
            return objResponse;
        }
        public ResponseModel GetGurantorsList(Guarantor model)
        {
            ResponseModel objResponse = new ResponseModel();
            List<SP_GUARANTORSEARCH_Result> objGurantorList = null;
            using (var ctx = new NPMDBEntities())
            {
                objGurantorList = ctx.SP_GUARANTORSEARCH(model.Guarant_Lname, model.Guarant_Fname, model.Guarant_Home_Phone, model.Guarant_City,
                   model.Guarant_State, model.Guarant_Zip, model.Guarant_Address, model.Guarant_Dob).ToList();
            }

            if (objGurantorList != null)
            {
                objResponse.Status = "Sucess";
                objResponse.Response = objGurantorList;
            }
            else
            {
                objResponse.Status = "Error";
            }
            return objResponse;
        }
        public ResponseModel GetNDCList(NDCViewModel model)
        {
            //mychanges
            ResponseModel objResponse = new ResponseModel();
            List<NDCSearchCriteria_Result> objdncList = null;
            using (var ctx = new NPMDBEntities())
            {
                objdncList = ctx.NDCSearchCriteria(model.HCPCS_code, model.ndc_code, model.drug_name, model.practice_code
                 ).ToList();
            }

            if (objdncList != null)
            {
                objResponse.Status = "Sucess";
                objResponse.Response = objdncList;
            }
            else
            {
                objResponse.Status = "Error";
            }
            return objResponse;
        }

        public ResponseModel GetDXList(DXViewModel model)
        {
            ResponseModel objResponse = new ResponseModel();
            List<DXSearchCriteria_Result> objDXList = null;
            using (var ctx = new NPMDBEntities())
            {
                objDXList = ctx.DXSearchCriteria(model.Diag_Code, model.Diag_Description
                 ).ToList();
            }

            if (objDXList != null)
            {
                objResponse.Status = "Sucess";
                objResponse.Response = objDXList;
            }
            else
            {
                objResponse.Status = "Error";
            }
            return objResponse;
        }
        #endregion Gurantor

        #region FeeSchedule

        public ResponseModel GetPracticeList(long userId)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                List<SelectListViewModel> objProviderCPTFee = null;
                using (var ctx = new NPMDBEntities())
                {
                    objProviderCPTFee = (from u in ctx.Users
                                         join upp in ctx.Users_Practice_Provider on u.UserId equals upp.User_Id
                                         join p in ctx.Practices on upp.Practice_Code equals p.Practice_Code
                                         where u.UserId == userId && !(p.Deleted ?? false) && !(upp.Deleted ?? false)
                                         select new SelectListViewModel()
                                         {
                                             Id = p.Practice_Code,
                                             Name = p.Prac_Name
                                         }).OrderBy(p => p.Name).ToList();
                }

                if (objProviderCPTFee != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = objProviderCPTFee;
                }
                else
                {
                    objResponse.Status = "Error";
                }
                return objResponse;

            }
            catch (Exception)
            {
                objResponse.Status = "Error";
            }
            return objResponse;
        }


        public ResponseModel GetEshaPracticeList(long userId)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
            
                List<GETPRACTICES_WITHDETAILS_Result> objProviderCPTFee = null;
                using (var ctx = new NPMDBEntities())
                {
                    objProviderCPTFee = ctx.GETPRACTICES_WITHDETAILS(userId).ToList();
                }

                if (objProviderCPTFee != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = objProviderCPTFee;
                }
                else
                {
                    objResponse.Status = "Error";
                }
                return objResponse;

            }
            catch (Exception)
            {
                objResponse.Status = "Error";
            }
            return objResponse;
        }
        public ResponseModel GetuserList(long pracCode)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                List<SelectListViewModel> objuserlist = null;
                using (var ctx = new NPMDBEntities())
                {
                    objuserlist = (from u in ctx.Users
                                         join upp in ctx.Users_Practice_Provider on u.UserId equals upp.User_Id
                                         where upp.Practice_Code == pracCode &&  !(upp.Deleted ?? false)
                                         select new SelectListViewModel()
                                         {
                                             Id = u.UserId,
                                             Name = u.FirstName+" "+u.LastName
                                         }).OrderBy(p => p.Name).ToList();
                }

                if (objuserlist != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = objuserlist;
                }
                else
                {
                    objResponse.Status = "Error";
                }
                return objResponse;

            }
            catch (Exception)
            {
                objResponse.Status = "Error";
            }
            return objResponse;
        }

        public ResponseModel GetStatesList()
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                List<State> objStates = null;
                using (var ctx = new NPMDBEntities())
                {
                    objStates = ctx.States.Where(ci => ci.Deleted == null || ci.Deleted == false).OrderBy(s => s.State_Code).ToList();
                }

                if (objStates != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = objStates;
                }
                else
                {
                    objResponse.Status = "Error";
                }
                return objResponse;

            }
            catch (Exception)
            {
                objResponse.Status = "Error";
            }
            return objResponse;
        }

        public ResponseModel GetProviderFeeScheduleDD()
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                List<SelectListViewModel> states = null;
                List<SelectListViewModel> practices = null;

                using (var ctx = new NPMDBEntities())
                {
                    states = ctx.States.Where(s => (s.Deleted ?? false) == false).OrderBy(s => s.State_Code).Select(s => new SelectListViewModel()
                    {
                        IdStr = s.State_Code,
                        Name = s.State_Name
                    }).ToList();
                    practices = ctx.Practices.Where(p => (p.Deleted ?? false) == false).Select(p => new SelectListViewModel()
                    {
                        Id = p.Practice_Code,
                        Name = p.Prac_Name
                    }).OrderBy(ci => ci.Name).ToList();
                }
                if (states != null && practices != null)
                {
                    objResponse.Status = "Success";
                    objResponse.Response = new ExpandoObject();
                    objResponse.Response.States = states;
                    objResponse.Response.Practices = practices;
                }
                else
                {
                    objResponse.Status = "Error";
                }
                return objResponse;

            }
            catch (Exception)
            {
                objResponse.Status = "Error";
            }
            return objResponse;
        }

        //created for adding NEW Provider CPT Plan in fee schedule and Created BY Backend_Team 10/Jan/2023
        public ResponseModel PostproviderFeeSchedule(providercptplanModel model, long userId)
        {
            ResponseModel responseModel = new ResponseModel();
            using (var ctx = new NPMDBEntities())
            {
                try
                {
                    if (model.Facility_Code == null)
                    { model.Facility_Code = 0; }

                    if (model.Location_Code == null)
                    {
                        model.Location_Code = 0;
                    }


                    if (model.InsPayer_Id == null)
                    {
                        model.InsPayer_Id = 0;

                    }

                    string ProviderCode = model.Provider_Code == 0 ? "All" : model.Provider_Code.ToString();
                    string InsPayerId = model.InsPayer_Id == 0 ? "All" : model.InsPayer_Id.ToString();
                    string InsuranceState = model.Insurance_State == "" || model.Insurance_State == null ? "All" : model.Insurance_State;
                    string LocationCode = model.Location_Code == 0 || model.Location_Code == null ? "ALL" : model.Location_Code.ToString();
                    string FacilityCode = model.Facility_Code == 0 || model.Facility_Code == null ? "All" : model.Facility_Code.ToString();
                    string Self_pay = model.self_pay == true  ? "SELF" : "ALL";
                    string Provider_Cpt_Plan_Id = model.Practice_Code + ProviderCode + InsPayerId + InsuranceState + "ALL" + FacilityCode+ Self_pay;
                   
                    var val = ctx.Provider_Cpt_Plan.FirstOrDefault(x => x.Provider_Cpt_Plan_Id == Provider_Cpt_Plan_Id);
                    if (val == null)
                    {
                        ctx.Provider_Cpt_Plan.Add(new Provider_Cpt_Plan()
                        {
                            Provider_Cpt_Plan_Id = Provider_Cpt_Plan_Id,
                            Practice_Code = model.Practice_Code,
                            Provider_Code = model.Provider_Code,
                            InsPayer_Id = model.InsPayer_Id,
                            Insurance_State = model.Insurance_State,
                            Location_Code = model.Location_Code,
                            Facility_Code = model.Facility_Code,
                            Cpt_Plan = model.Cpt_Plan,
                            Percentage_Higher = model.Percentage_Higher,
                            self_pay = model.self_pay,
                            Deleted = false,
                            Created_By = userId,
                            Created_Date = DateTime.Now,
                            Modified_By = null,
                            modification_allowed = null,
                            Modified_Date = null

                        });

                        if (ctx.SaveChanges() > 0)
                        {
                            responseModel.Status = "Success";
                        }
                        else
                        {
                            responseModel.Status = "Error";
                        }
                    }
                    else
                    {
                        responseModel.Status = "dup";
                    }
                   

                  
                }catch (Exception e)
                {
              
                    throw;

                }
            }
            return responseModel;
        }

        //Created BY Backend_Team 10/Jan/2023
        public ResponseModel checkproviderFeeinformation(check_provider_cptplan_existence model)
        {
            ResponseModel responseModel = new ResponseModel();

            try
            {
                if (model.Practice_Code != 0 && model.Provider_Code != 0 && model.Location_Code != 0 && model.Location_State!=null )
                {
                    
                    using (var ctx = new NPMDBEntities())
                    {
                        var obj = ctx.check_CPtPlan(model.Practice_Code, model.Provider_Code, model.Location_Code, model.Location_State).ToList();
                        if (obj.Count > 0)
                        {
                             responseModel.Status = "Success";

                        }
                        else
                        {
                            responseModel.Status = "Error";
                        }
                        
                    }
                }
                else
                {
                    responseModel.Status = "Null";
                }
                
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    System.Diagnostics.Debug.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);

                    foreach (var ve in eve.ValidationErrors)
                    {
                        System.Diagnostics.Debug.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
                throw;

            }


            return responseModel;

            
            }

        //Created BY Backend_Team 10/Jan/2023
        public ResponseModel getpracticeinformationforcptplan(long Practicecode)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {

                    if (Practicecode != 0)
                    {
                        List<SP_getpraticedataforCPTPlan_Result> get_info = new List<SP_getpraticedataforCPTPlan_Result>();
                        get_info = ctx.SP_getpraticedataforCPTPlan(Practicecode).ToList();

                        if (get_info != null)
                        {
                            responseModel.Status = "Success";
                            responseModel.Response = get_info;
                        }
                        else
                        {
                            responseModel.Status = "Error";
                        }
                    }
                    else
                    {
                        responseModel.Status = "Practice code Null";
                    }
                }

            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    System.Diagnostics.Debug.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);

                    foreach (var ve in eve.ValidationErrors)
                    {
                        System.Diagnostics.Debug.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
                throw;

            }

            return responseModel;

        }

        //below code added by umer tariq

        public async Task<ResponseModel> GetRevenueCode(string Code)
        {

            ResponseModel objResponse = new ResponseModel();
            try
            {


               Revenue_Codes revenue_Code = new Revenue_Codes();
                using (var ctx = new NPMDBEntities())
                {
                    revenue_Code = await ctx.Revenue_Codes.Where(c=>c.revcode== Code).FirstOrDefaultAsync();
                }
                if (revenue_Code != null)
                {
                    objResponse.Status = "Success";
                    objResponse.Response = revenue_Code;
                }
            }

            catch (Exception)
            {
                objResponse.Status = "Error";
            }
            return objResponse;
        }


        //above code added by umer tariq

        public ResponseModel GetProviderFeeSchedule(ProviderFeeScheduleSearchVM model)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                if (model.PracticeCode != 0)
                {
                    List<SP_SearchProviderCPTFeePlan_Result> objProviderCPTFee = new List<SP_SearchProviderCPTFeePlan_Result>();
                    using (var ctx = new NPMDBEntities())
                    {
                        bool? selfPay = null;
                        if (model.FaciltiyOrLocation == "Facility")
                        {
                            model.LocationCode = null;
                            if (model.FacilityCode == null)
                                model.FacilityCode = 0;
                        }
                        if (model.FaciltiyOrLocation == "Location")
                        {
                            model.FacilityCode = null;
                            if (model.LocationCode == null)
                            {
                                model.LocationCode = 0;
                            }
                        }
                        if (model.InsuranceOrSelfPay == "SelfPay")
                        {
                            model.InsuranceId = null;
                            selfPay = true;
                        }
                        if (model.InsuranceOrSelfPay == "Insurance")
                        {
                            if (model.InsuranceId == null)
                                model.InsuranceId = 0;
                        }
                        objProviderCPTFee = ctx.SP_SearchProviderCPTFeePlan(model.PracticeCode, model.ProviderCode, model.InsuranceId, model.LocationCode, model.State, model.FacilityCode, selfPay).ToList();
                    }
                    if (objProviderCPTFee != null && objProviderCPTFee.Count() > 0)
                    {
                        objResponse.Status = "Success";
                        objResponse.Response = objProviderCPTFee;
                    }
                    else
                    {
                        objResponse.Status = "No fee plan found.";
                    }
                }
                return objResponse;

            }
            catch (Exception)
            {
                objResponse.Status = "Error";
            }
            return objResponse;
        }

        public ResponseModel GetProviderFeeSchedule(long PracticeCode)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                // List<provider_cpt_plan> objProviderCPTFee = null;
                List<SP_getcptplanlist_Result> objProviderCPTFee = null;
                using (var ctx = new NPMDBEntities())
                {
                  objProviderCPTFee = ctx.SP_getcptplanlist(PracticeCode).ToList();
                 
                }

                if (objProviderCPTFee != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = objProviderCPTFee;
                }
                else
                {
                    objResponse.Status = "Error";
                }
                return objResponse;

            }
            catch (Exception)
            {
                objResponse.Status = "Error";
            }
            return objResponse;
        }

        public ResponseModel PostProviderCptPlanDetails(Provider_Cpt_Plan_Details model, long userId)
        {
            //Added BY Hamza AKhlaq For Fee Schedule Enhancment
            ResponseModel res = new ResponseModel();
            using (var ctx = new NPMDBEntities())
            {
                try
                {
                    var id = ctx.SP_TableIdGenerator("Provider_Cpt_Plan_Detail_Id").FirstOrDefault().ToString();
                    ctx.Provider_Cpt_Plan_Details.Add(new Provider_Cpt_Plan_Details()
                    {       
                        Provider_Cpt_Plan_Detail_Id =Convert.ToInt64(id),
                        Provider_Cpt_Plan_Id =  model.Provider_Cpt_Plan_Id,
                        Cpt_Modifier = "",
                        Revenue_Code = model.Revenue_Code,
                        Revenue_Description = model.Revenue_Description,
                        Cpt_Code =model.Cpt_Code,
                        Cpt_Description =model.Cpt_Description,
                        Alternate_Code = model.Alternate_Code,
                        Charges= model.Charges != null ? model.Charges : "0",
                        Non_Facility_Non_Participating_Fee = model.Non_Facility_Non_Participating_Fee,
                        Non_Facility_Participating_Fee = model.Non_Facility_Participating_Fee,
                        Facility_Non_Participating_Fee = model.Facility_Non_Participating_Fee,
                        Facility_Participating_Fee = model.Facility_Participating_Fee,
                        Deleted = false,
                        Created_Date = DateTime.Now,
                        Created_By = userId,
                    });
                    if (ctx.SaveChanges() > 0)
                    {
                        res.Status = "Success";
                    }
                    else
                    {
                        res.Status = "Error";
                    }
                }
                catch (DbEntityValidationException e)
                {
                    foreach (var eve in e.EntityValidationErrors)
                    {
                        System.Diagnostics.Debug.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                            eve.Entry.Entity.GetType().Name, eve.Entry.State);
                       
                        foreach (var ve in eve.ValidationErrors)
                        {
                            System.Diagnostics.Debug.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                ve.PropertyName, ve.ErrorMessage);
                        }
                    }
                    throw;
                }
            }
            return res;
        }

        public ResponseModel CheckDuplicateCPT(string ProviderCPTCode, string ProviderCPTPlainId)
        {
            ResponseModel res = new ResponseModel();
            using (var ctx = new NPMDBEntities())
            {
                var search = ctx.Provider_Cpt_Plan_Details.Where(p => p.Cpt_Code == ProviderCPTCode && (p.Alternate_Code==""|| p.Alternate_Code == null) && p.Provider_Cpt_Plan_Id == ProviderCPTPlainId && p.Deleted == false).FirstOrDefault();
                if (search != null) 
                {
                    res.Status = "Duplicate";
                }
            }
            return res;
        }

        //.. Added by Pir Ubaid - CPT with multiple description
        public ResponseModel CheckDuplicateAlternateCode(string AlternateCode, string ProviderCPTPlainId)
        {
            ResponseModel res = new ResponseModel();
            using (var ctx = new NPMDBEntities())
            {
                var search = ctx.Provider_Cpt_Plan_Details.Where(p => p.Alternate_Code == AlternateCode && p.Provider_Cpt_Plan_Id == ProviderCPTPlainId && (p.Deleted == null || p.Deleted == false)).FirstOrDefault();
                if (search != null)
                {
                    res.Status = "Duplicate";
                }
            }
            return res;
        }


        //  check duplicate alternate code in claim charges
        public ResponseModel DuplicateAlternateCodeClaimCharges(string AlternateCode)
        {
            ResponseModel res = new ResponseModel();
            using (var ctx = new NPMDBEntities())
            {
                var search = ctx.Claim_Charges.Where(p => p.Alternate_Code == AlternateCode).FirstOrDefault();
                if (search != null)
                {
                    res.Status = "Duplicate";
                }
            }
            return res;
        }


        //..

        public ResponseModel GetProviderPlanDetails(string ProviderCPTPlanId)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                //Added By Hamza Akhlaq For Fee Schedule
                List<GetCPTPlanByProviderPlanId_Temp_Result> objProviderCPTFee = null;
                using (var ctx = new NPMDBEntities())
                {
                    objProviderCPTFee = ctx.GetCPTPlanByProviderPlanId_Temp(ProviderCPTPlanId).ToList();
                }

                if (objProviderCPTFee != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = objProviderCPTFee;
                }
                else
                {
                    objResponse.Status = "Error";
                }
                return objResponse;

            }
            catch (Exception)
            {
                throw;
            }
        }

        public ResponseModel GetDescriptionByCPT(string ProviderCPTPCode)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                string Cpt_Description = null;
                string Alternate_Code = null;
                using (var ctx = new NPMDBEntities())
                {
                    Cpt_Description = ctx.Procedures.Where(p => p.ProcedureCode == ProviderCPTPCode).FirstOrDefault()?.ProcedureDescription;
                    Alternate_Code = ctx.Procedures.Where(p => p.ProcedureCode == ProviderCPTPCode).FirstOrDefault()?.Alternate_Code;
                }

                if (Cpt_Description != null)
                {
                    objResponse.Status = "Sucess";
                    //objResponse.Response = Cpt_Description;
                    objResponse.Response = new
                    {
                        Cpt_Description = Cpt_Description,
                        Alternate_Code = Alternate_Code,
                    };
                }
                else
                {
                    objResponse.Status = "Incorrect CPT";
                }
                return objResponse;

            }
            catch (Exception)
            {
                throw;
            }
        }


        //.. Added by Pir Ubaid - CPT With Multiple Description

        //for check the code wethter it is CPT code or Alternate Code 
        public ResponseModel CheckCode(string Code)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                string Procedure_Code = null;
                string Alternate_Code = null;
              //  string Description = null;
//string Charges = null;

                using (var ctx = new NPMDBEntities())
                {
                    Console.WriteLine($"Received Code: {Code}");

                    //             var providerCptDetails = ctx.Provider_Cpt_Plan_Details
                    //.Where(p => p.Cpt_Code.Trim().Equals(Code.Trim(), StringComparison.OrdinalIgnoreCase)
                    //         || p.Alternate_Code.Trim().Equals(Code.Trim(), StringComparison.OrdinalIgnoreCase))
                    //.OrderByDescending(p => p.Alternate_Code.Trim().Equals(Code.Trim(), StringComparison.OrdinalIgnoreCase)) // Exact Alternate_Code match first
                    //.ThenByDescending(p => p.Cpt_Code.Trim().Equals(Code.Trim(), StringComparison.OrdinalIgnoreCase) && p.Alternate_Code == null) // Next, Cpt_Code match with null Alternate_Code
                    //.ThenByDescending(p => p.Cpt_Code.Trim().Equals(Code.Trim(), StringComparison.OrdinalIgnoreCase)) // Lastly, Cpt_Code match with non-null Alternate_Code
                    //.FirstOrDefault();
                    var providerCptDetails = ctx.Provider_Cpt_Plan_Details
                 .FirstOrDefault(p => p.Alternate_Code.Trim().Equals(Code.Trim(), StringComparison.OrdinalIgnoreCase));

                    if (providerCptDetails == null)
                    {
                        // 2. If no result, check for a match with Cpt_Code and null Alternate_Code
                        providerCptDetails = ctx.Provider_Cpt_Plan_Details
                            .FirstOrDefault(p => p.Cpt_Code.Trim().Equals(Code.Trim(), StringComparison.OrdinalIgnoreCase)
                                              && p.Alternate_Code == null);

                        if (providerCptDetails == null)
                        {
                            // 3. If no result, return the first match for Cpt_Code, regardless of Alternate_Code
                            providerCptDetails = ctx.Provider_Cpt_Plan_Details
                                .FirstOrDefault(p => p.Cpt_Code.Trim().Equals(Code.Trim(), StringComparison.OrdinalIgnoreCase));
                        }
                    }
                    if (providerCptDetails != null)
                    {
                        Console.WriteLine($"Found in Provider_Cpt_Plan_Details: {providerCptDetails.Cpt_Code}, {providerCptDetails.Alternate_Code}");

                        // If code matches Cpt_Code, return its Alternate_Code
                        if (providerCptDetails.Cpt_Code.Trim().Equals(Code.Trim(), StringComparison.OrdinalIgnoreCase))
                        {
                            Procedure_Code = providerCptDetails.Cpt_Code?.Trim();
                            Alternate_Code = providerCptDetails.Alternate_Code?.Trim();
                          //  Description = providerCptDetails.Cpt_Description?.Trim();
                           

                        }
                        // If code matches Alternate_Code, return its Cpt_Code
                        else if (providerCptDetails.Alternate_Code.Trim().Equals(Code.Trim(), StringComparison.OrdinalIgnoreCase))
                        {
                            Procedure_Code = providerCptDetails.Cpt_Code?.Trim();
                            Alternate_Code = providerCptDetails.Alternate_Code;
                            //Description = providerCptDetails.Cpt_Description?.Trim();
                        }
                    }
                    else
                    {
                        Console.WriteLine("Not found in Provider_Cpt_Plan_Details, checking Procedures table.");

                        //                   var procedure = ctx.Procedures
                        //.Where(p => p.ProcedureCode.Trim().Equals(Code.Trim(), StringComparison.OrdinalIgnoreCase)
                        //         || p.Alternate_Code.Trim().Equals(Code.Trim(), StringComparison.OrdinalIgnoreCase))
                        //.OrderByDescending(p => p.Alternate_Code == null && p.ProcedureCode.Trim().Equals(Code.Trim(), StringComparison.OrdinalIgnoreCase)) // Prioritize matching ProcedureCode with null Alternate_Code
                        //.ThenByDescending(p => p.Alternate_Code.Trim().Equals(Code.Trim(), StringComparison.OrdinalIgnoreCase)) // Then prioritize exact Alternate_Code match
                        //.ThenBy(p => p.Alternate_Code != null) // Lastly, prioritize non-null Alternate_Code
                        //.FirstOrDefault();

                        // 1. First, check for a match with ProcedureCode and null Alternate_Code
                        var procedure = ctx.Procedures
                            .FirstOrDefault(p => p.ProcedureCode.Trim().Equals(Code.Trim(), StringComparison.OrdinalIgnoreCase)
                                              && p.Alternate_Code == null);

                        if (procedure == null)
                        {
                            // 2. If no result, check for an exact match with Alternate_Code
                            procedure = ctx.Procedures
                                .FirstOrDefault(p => p.Alternate_Code.Trim().Equals(Code.Trim(), StringComparison.OrdinalIgnoreCase));

                            if (procedure == null)
                            {
                                // 3. If no result, return the first match for ProcedureCode, regardless of Alternate_Code
                                procedure = ctx.Procedures
                                    .FirstOrDefault(p => p.ProcedureCode.Trim().Equals(Code.Trim(), StringComparison.OrdinalIgnoreCase));
                            }
                        }

                        if (procedure != null)
                        {
                            Console.WriteLine($"Found in Procedures: {procedure.ProcedureCode}, {procedure.Alternate_Code}");

                            // If code matches ProcedureCode, return its Alternate_Code
                            if (procedure.ProcedureCode.Trim().Equals(Code.Trim(), StringComparison.OrdinalIgnoreCase))
                            {
                                Procedure_Code = procedure.ProcedureCode;
                                Alternate_Code = procedure.Alternate_Code;
                                //Description = procedure.ProcedureDescription;
                            }
                            // If code matches Alternate_Code, return its ProcedureCode
                            else if (procedure.Alternate_Code.Trim().Equals(Code.Trim(), StringComparison.OrdinalIgnoreCase))
                            {
                                Procedure_Code = procedure.ProcedureCode;
                                Alternate_Code = procedure.Alternate_Code;
                               // Description = procedure.ProcedureDescription;
                            }
                        }
                    }
                }

                // If Procedure_Code is not null, it means we found a match
                if (Procedure_Code != null)
                {
                    objResponse.Status = "Success";
                    objResponse.Response = new
                    {
                        Procedure_Code = Procedure_Code,
                        Alternate_Code = Alternate_Code,
                        //Description = Description
                    };
                }
                else
                {
                    // If no match was found in either table
                    objResponse.Status = "Invalid Code";
                }
                return objResponse;
            }
            catch (Exception ex)
            {
                // You may want to log the exception or handle it as per your error handling strategy
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }


        //


        public ResponseModel GetByAlternateCode(string AlternateCode)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                string Cpt_Code = null;
                string Cpt_Description = null;
                using (var ctx = new NPMDBEntities())
                {
                    Cpt_Code = ctx.Procedures.Where(p => p.Alternate_Code == AlternateCode).FirstOrDefault()?.ProcedureCode;
                    Cpt_Description = ctx.Procedures.Where(p => p.Alternate_Code == AlternateCode).FirstOrDefault()?.ProcedureDescription;
                }

                if (Cpt_Code != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = new
                    {
                        Cpt_Description = Cpt_Description,
                        Cpt_Code = Cpt_Code,
                    };
                }
                else
                {
                    objResponse.Status = "Incorrect Alternate Code";
                }
                return objResponse;

            }
            catch (Exception)
            {
                throw;
            }
        }
        //..
        public ResponseModel InitProviderFeePlan(ProviderFeeScheduleSearchVM model)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                if (model.PracticeCode != 0)
                {
                    List<SP_SearchProviderCPTFeePlan_Result> objProviderCPTFee = new List<SP_SearchProviderCPTFeePlan_Result>();
                    List<STANDARD_CPT_FEE> objStandardCPTFee = new List<STANDARD_CPT_FEE>();
                    using (var ctx = new NPMDBEntities())
                    {
                        bool? selfPay = null;
                        if (model.FaciltiyOrLocation == "Facility")
                        {
                            model.LocationCode = null;
                            if (model.FacilityCode == null)
                                model.FacilityCode = 0;
                        }
                        if (model.FaciltiyOrLocation == "Location")
                        {
                            model.FacilityCode = null;
                            if (model.LocationCode == null)
                            {
                                model.LocationCode = 0;
                            }
                        }
                        if (model.InsuranceOrSelfPay == "SelfPay")
                        {
                            model.InsuranceId = null; 
                            selfPay = true;
                        }
                        if (model.InsuranceOrSelfPay == "Insurance")
                        {
                            if (model.InsuranceId == null)
                                model.InsuranceId = 0;
                        }
                        objProviderCPTFee = ctx.SP_SearchProviderCPTFeePlan(model.PracticeCode, model.ProviderCode, model.InsuranceId, model.LocationCode, model.State, model.FacilityCode, selfPay).ToList();
                        if (objProviderCPTFee != null)
                        {
                            objResponse.Status = "Success";
                            objResponse.Response = objProviderCPTFee;
                        }
                        else
                        {
                            objResponse.Status = "Error";
                        }
                        //else
                        //{
                        //    if (model.State == null || model.State == "0")
                        //    {
                        //        objStandardCPTFee = ctx.STANDARD_CPT_FEE.Where(scf => (scf.Deleted ?? false) == false).ToList();
                        //    }
                        //    else
                        //    {
                        //        objStandardCPTFee = ctx.STANDARD_CPT_FEE.Where(scf => (scf.Deleted ?? false) == false && scf.State == model.State).Take(100).ToList();
                        //    }
                        //    if (objStandardCPTFee != null && objStandardCPTFee.Count() > 0)
                        //    {
                        //        objResponse.Status = "Success";
                        //        objResponse.Response = new ExpandoObject();
                        //        objResponse.Response.ProviderFeePlans = null;
                        //        objResponse.Response.StandardCPTFee = objStandardCPTFee;
                        //    }
                        //    else
                        //    {
                        //        objResponse.Status = "No standard CPT fees found.";
                        //        objResponse.Response = new ExpandoObject();
                        //        objResponse.Response.ProviderFeePlans = null;
                        //        objResponse.Response.StandardCPTFee = null;
                        //    }
                        //}
                    }
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

        public ResponseModel GetProviderPlanDetails(string ProviderCPTPlanId, Pager pager)
        {
            ResponseModel responseModel = new ResponseModel();
            PagingResponse pagingResponse = new PagingResponse();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var results = ctx.GetCPTPlanByProviderPlanId(ProviderCPTPlanId, pager.Page, pager.PageSize, pager.SearchString, pager.SortBy, pager.SortOrder).ToList();
                    if (results != null)
                    {
                        pagingResponse.data = results;
                        pagingResponse.TotalRecords = results.FirstOrDefault()?.TotalRecords == null ? 0 : results.FirstOrDefault().TotalRecords;
                        pagingResponse.FilteredRecords = results.FirstOrDefault()?.FilteredRecords == null ? 0 : results.FirstOrDefault()?.FilteredRecords;
                        responseModel.Status = "Success";
                        responseModel.Response = pagingResponse;
                    }
                    else
                    {
                        responseModel.Status = "Error";
                    }
                }
            }
            catch (Exception ex)
            {
                responseModel.Status = ex.ToString();
            }
            return responseModel;
        }

        public ResponseModel GetStandardCPTFeeSchedule(string StateCode)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                List<STANDARD_CPT_FEE> objStandardCPTFee = null;
                using (var ctx = new NPMDBEntities())
                {
                    objStandardCPTFee = ctx.STANDARD_CPT_FEE.Where(scf => scf.State == StateCode).ToList();
                }

                if (objStandardCPTFee != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = objStandardCPTFee;
                }
                else
                {
                    objResponse.Status = "Error";
                }
                return objResponse;

            }
            catch (Exception)
            {
                objResponse.Status = "Error";
            }
            return objResponse;
        }

        public ResponseModel GetStandardNobilityCPTFeeSchedule(string StateCode)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                List<Nobility_standardcptfee> objStandardCPTFee = null;
                using (var ctx = new NPMDBEntities())
                {
                    objStandardCPTFee = ctx.Nobility_standardcptfee.Where(scf => scf.State == StateCode).ToList();
                }

                if (objStandardCPTFee != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = objStandardCPTFee;
                }
                else
                {
                    objResponse.Status = "Error";
                }
                return objResponse;

            }
            catch (Exception)
            {
                objResponse.Status = "Error";
            }
            return objResponse;
        }

        public ResponseModel CreateProviderCPTPlan(CreateProviderCPTPlanVM model, long userId)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    bool? selfPay = null;
                    if (model.FaciltiyOrLocation == "Facility")
                    {
                        model.LocationCode = null;
                        if (model.FacilityCode == null)
                            model.FacilityCode = 0;
                    }
                    if (model.FaciltiyOrLocation == "Location")
                    {
                        model.FacilityCode = null;
                        if (model.LocationCode == null)
                        {
                            model.LocationCode = 0;
                        }
                    }
                    if (model.InsuranceOrSelfPay == "SelfPay")
                    {
                        model.InsuranceId = null;
                        selfPay = true;
                    }
                    if (model.InsuranceOrSelfPay == "Insurance")
                    {
                        if (model.InsuranceId == null)
                            model.InsuranceId = 0;
                    }
                    var results = ctx.SP_CreateProviderCPTPlanDetails(userId, model.PracticeCode, model.ProviderCode, model.State, model.LocationCode, model.FacilityCode, model.InsuranceId, selfPay, model.StandardOrPercentAge, model.PercentageHigher, model.Customize, model.ModificationAllowed, model.Computed);
                    responseModel.Status = "Success";
                }
            }
            catch (Exception ex)
            {
                responseModel.Status = ex.ToString();
            }
            #region obsolete
            //try
            //{
            //    bool? selfPay = null;
            //    if (model.FaciltiyOrLocation == "Facility")
            //    {
            //        model.LocationCode = null;
            //        if (model.FacilityCode == null)
            //            model.FacilityCode = 0;
            //    }
            //    if (model.FaciltiyOrLocation == "Location")
            //    {
            //        model.FacilityCode = null;
            //        if (model.LocationCode == null)
            //        {
            //            model.LocationCode = 0;
            //        }
            //    }
            //    if (model.InsuranceOrSelfPay == "SelfPay")
            //    {
            //        model.InsuranceId = null;
            //        selfPay = true;
            //    }
            //    if (model.InsuranceOrSelfPay == "Insurance")
            //    {
            //        if (model.InsuranceId == null)
            //            model.InsuranceId = 0;
            //    }
            //    string ProviderCode = model.ProviderCode == 0 ? "All" : model.ProviderCode.ToString();
            //    string InsPayerId = model.InsuranceId == 0 ? "All" : model.InsuranceId.ToString();
            //    string InsuranceState = model.State == "0" ? "All" : model.State;
            //    string LocationCode = model.LocationCode == 0 || model.LocationCode == null ? "All" : model.LocationCode.ToString();
            //    string FacilityCode = model.FacilityCode == 0 || model.FacilityCode == null ? "All" : model.FacilityCode.ToString();
            //    string Provider_Cpt_Plan_Id = model.PracticeCode + ProviderCode + InsPayerId + InsuranceState + LocationCode + FacilityCode;
            //    using (var ctx = new NPMDBEntities())
            //    {
            //        using (var transaction = ctx.Database.BeginTransaction())
            //        {
            //            try
            //            {
            //                var cptsToCopy = ctx.STANDARD_CPT_FEE.Where(s => (s.Deleted ?? false) == false).ToList();
            //                if (cptsToCopy != null && cptsToCopy.Count() > 0)
            //                {
            //                    Provider_Cpt_Plan provider_Cpt_Plan = new Provider_Cpt_Plan()
            //                    {
            //                        Cpt_Plan = "CUSTM",
            //                        Created_By = userId,
            //                        Created_Date = DateTime.Now,
            //                        Deleted = false,
            //                        Facility_Code = model.FacilityCode,
            //                        InsPayer_Id = model.InsuranceId,
            //                        Insurance_State = model.State,
            //                        Location_Code = model.LocationCode,
            //                        Modified_By = null,
            //                        Modified_Date = null,
            //                        Percentage_Higher = model.PercentageHigher,
            //                        Practice_Code = model.PracticeCode,
            //                        Provider_Code = model.ProviderCode,
            //                        Provider_Cpt_Plan_Id = Provider_Cpt_Plan_Id,
            //                        self_pay = selfPay
            //                    };
            //                    ctx.Provider_Cpt_Plan.Add(provider_Cpt_Plan);
            //                    ctx.SaveChanges();
            //                    List<Provider_Cpt_Plan_Details> provider_Cpt_Plan_Details = new List<Provider_Cpt_Plan_Details>();
            //                    foreach (var item in cptsToCopy)
            //                    {
            //                        provider_Cpt_Plan_Details.Add(new Provider_Cpt_Plan_Details()
            //                        {
            //                            Provider_Cpt_Plan_Detail_Id = Convert.ToInt64(ctx.SP_TableIdGenerator("Provider_Cpt_Plan_Detail_Id").FirstOrDefault().ToString()),
            //                            Provider_Cpt_Plan_Id = Provider_Cpt_Plan_Id,
            //                            Cpt_Code = item.Cpt_Code,
            //                            Cpt_Description = item.Cpt_Description,
            //                            Cpt_Modifier = item.Cpt_Modifier == null ? "" : item.Cpt_Modifier,
            //                            Created_By = userId,
            //                            Created_Date = DateTime.Now,
            //                            Deleted = false,
            //                            Facility_Non_Participating_Fee = model.StandardOrPercentAge == "Percentage" && item.Facility_Non_Participating_Fee > 0 ? item.Facility_Non_Participating_Fee + ((decimal)(item.Facility_Non_Participating_Fee * model.PercentageHigher) / 100) : item.Facility_Non_Participating_Fee,
            //                            Facility_Non_Participating_Fee_ctrl_Fee = model.StandardOrPercentAge == "Percentage" && item.Facility_Non_Participating_Fee_ctrl_Fee > 0 ? item.Facility_Non_Participating_Fee_ctrl_Fee + ((decimal)(item.Facility_Non_Participating_Fee_ctrl_Fee * model.PercentageHigher) / 100) : item.Facility_Non_Participating_Fee_ctrl_Fee,
            //                            Facility_Participating_Fee = model.StandardOrPercentAge == "Percentage" && item.Facility_Participating_Fee > 0 ? item.Facility_Participating_Fee + ((decimal)(item.Facility_Participating_Fee * model.PercentageHigher) / 100) : item.Facility_Participating_Fee,
            //                            Facility_Participating_Fee_ctrl_Fee = model.StandardOrPercentAge == "Percentage" && item.Facility_Participating_Fee_ctrl_Fee > 0 ? item.Facility_Participating_Fee_ctrl_Fee + ((decimal)(item.Facility_Participating_Fee_ctrl_Fee * model.PercentageHigher) / 100) : item.Facility_Participating_Fee_ctrl_Fee,
            //                            Non_Facility_Non_Participating_Fee = model.StandardOrPercentAge == "Percentage" && item.Non_Facility_Non_Participating_Fee > 0 ? item.Non_Facility_Non_Participating_Fee + ((decimal)(item.Non_Facility_Non_Participating_Fee * model.PercentageHigher) / 100) : item.Non_Facility_Non_Participating_Fee,
            //                            Non_Facility_Non_Participating_Fee_ctrl_Fee = model.StandardOrPercentAge == "Percentage" && item.Non_Facility_Non_Participating_Fee_ctrl_Fee > 0 ? item.Non_Facility_Non_Participating_Fee_ctrl_Fee + ((decimal)(item.Non_Facility_Non_Participating_Fee_ctrl_Fee * model.PercentageHigher) / 100) : item.Non_Facility_Non_Participating_Fee_ctrl_Fee,
            //                            Non_Facility_Participating_Fee = model.StandardOrPercentAge == "Percentage" && item.Non_Facility_Participating_Fee > 0 ? item.Non_Facility_Participating_Fee + ((decimal)(item.Non_Facility_Participating_Fee * model.PercentageHigher) / 100) : item.Non_Facility_Participating_Fee,
            //                            Non_Facility_Participating_Fee_ctrl_Fee = model.StandardOrPercentAge == "Percentage" && item.Non_Facility_Participating_Fee_ctrl_Fee > 0 ? item.Non_Facility_Participating_Fee_ctrl_Fee + ((decimal)(item.Non_Facility_Participating_Fee_ctrl_Fee * model.PercentageHigher) / 100) : item.Non_Facility_Participating_Fee_ctrl_Fee
            //                        });
            //                    }
            //                    ctx.Provider_Cpt_Plan_Details.AddRange(provider_Cpt_Plan_Details);
            //                    ctx.SaveChanges();
            //                    transaction.Commit();
            //                    responseModel.Status = "Success";
            //                }
            //            }
            //            catch (Exception ex)
            //            {
            //                transaction.Rollback();
            //                responseModel.Status = ex.ToString();
            //            }
            //        }
            //    }

            //}
            //catch (Exception ex)
            //{
            //    responseModel.Status = ex.ToString();
            //} 
            #endregion
            return responseModel;
        }

        public ResponseModel UpdateProviderCPTDetails(string id, List<Provider_Cpt_Plan_Details> model, long v)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    model.ForEach(item =>
                    {
                        item.Modified_By = v;
                        item.Modified_Date = DateTime.Now;
                        ctx.Provider_Cpt_Plan_Details.Attach(item);
                        ctx.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    });
                    if (ctx.SaveChanges() > 0)
                    {
                        responseModel.Status = "Success";
                    }
                    else
                    {
                        responseModel.Status = "Failure to update Provider CPT Details.";
                    }
                }
            }
            catch (Exception ex)
            {
                responseModel.Status = ex.ToString();
            }
            return responseModel;
        }

        public ResponseModel DeleteProviderPlanAndCPT(string planId, long v)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    if (ctx.SP_DeleteProviderPlainWithCPTS(planId, v) > 0)
                        responseModel.Status = "Success";
                    else
                        responseModel.Status = "Error";
                }
            }
            catch (Exception ex)
            {
                responseModel.Status = ex.ToString();
            }
            return responseModel;
        }

        public ResponseModel GetStandardCPTFeeSchedule(long practiceCode, Pager pager)
        {
            ResponseModel responseModel = new ResponseModel();
            PagingResponse pagingResponse = new PagingResponse();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var results = ctx.SP_GetStandardCPTSByPracticeState(practiceCode, pager.Page, pager.PageSize, pager.SearchString, pager.SortBy, pager.SortOrder).ToList();
                    if (results != null)
                    {
                        pagingResponse.data = results;
                        pagingResponse.TotalRecords = results.FirstOrDefault()?.TotalRecords == null ? 0 : results.FirstOrDefault().TotalRecords;
                        pagingResponse.FilteredRecords = results.FirstOrDefault()?.FilteredRecords == null ? 0 : results.FirstOrDefault().FilteredRecords;
                        responseModel.Status = "Success";
                        responseModel.Response = pagingResponse;
                    }
                    else
                    {
                        responseModel.Status = "Error";
                    }
                }
            }
            catch (Exception ex)
            {
                responseModel.Status = ex.ToString();
            }
            return responseModel;
        }

        #region Provider CPT Plan Notes

        public ResponseModel GetProviderCPTPlanNotes(string planId)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var notes = (from n in ctx.Provider_Cpt_Plan_Notes
                                 join uc in ctx.Users on n.CREATED_BY equals uc.UserId
                                 join um in ctx.Users on n.MODIFIED_BY equals um.UserId into num
                                 from um in num.DefaultIfEmpty()
                                 where (n.Deleted ?? false) == false && n.Provider_Cpt_Plan_Id == planId
                                 select new ProviderCptPlanNotesVM()
                                 {
                                     Provider_Cpt_Plan_Id = n.Provider_Cpt_Plan_Id,
                                     Deleted = n.Deleted,
                                     CREATED_BY = n.CREATED_BY,
                                     CREATED_BY_FULL_NAME = uc.LastName + ", " + uc.FirstName,
                                     CREATED_DATE = n.CREATED_DATE,
                                     MODIFIED_BY = n.MODIFIED_BY,
                                     MODIFIED_BY_FULL_NAME = !string.IsNullOrEmpty(um.LastName) && !string.IsNullOrEmpty(um.FirstName) ? um.LastName + ", " + um.FirstName : "-",
                                     MODIFIED_DATE = n.MODIFIED_DATE,
                                     NOTE_CONTENT = n.NOTE_CONTENT,
                                     NOTE_DATE = n.NOTE_DATE,
                                     Note_Id = n.Note_Id,
                                     NOTE_USER = n.NOTE_USER
                                 }).ToList();
                    if (notes != null)
                    {
                        responseModel.Response = notes;
                        responseModel.Status = "Success";
                    }
                    else
                    {
                        responseModel.Status = "Error";
                    }
                }
            }
            catch (Exception ex)
            {
                responseModel.Status = ex.ToString();
            }
            return responseModel;
        }

        public ResponseModel SaveProviderCPTNote(ProviderCptPlanNoteCreateVM note, long v)
        {

            ResponseModel responseModel = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    if (note.Note_Id == 0)
                    {
                        ctx.Provider_Cpt_Plan_Notes.Add(new Provider_Cpt_Plan_Notes()
                        {
                            CREATED_BY = v,
                            CREATED_DATE = DateTime.Now,
                            Deleted = false,
                            MODIFIED_BY = null,
                            MODIFIED_DATE = null,
                            NOTE_CONTENT = note.NOTE_CONTENT,
                            NOTE_DATE = DateTime.Now,
                            Note_Id = Convert.ToInt64(ctx.SP_TableIdGenerator("Note_Id").FirstOrDefault()),
                            NOTE_USER = v.ToString(),
                            Provider_Cpt_Plan_Id = note.Provider_Cpt_Plan_Id
                        });
                    }
                    else
                    {
                        var updateNote = ctx.Provider_Cpt_Plan_Notes.FirstOrDefault(n => n.Note_Id == note.Note_Id);
                        if (updateNote != null)
                        {
                            updateNote.Deleted = false;
                            updateNote.MODIFIED_BY = v;
                            updateNote.MODIFIED_DATE = DateTime.Now;
                            updateNote.NOTE_CONTENT = note.NOTE_CONTENT;
                            ctx.Entry(updateNote).State = System.Data.Entity.EntityState.Modified;
                        }
                        else
                        {
                            responseModel.Status = "No note found.";
                        }
                    }
                    if (ctx.SaveChanges() > 0)
                    {
                        responseModel.Status = "Success";
                    }
                    else
                    {
                        responseModel.Status = "Error";
                    }
                }
            }
            catch (Exception ex)
            {
                responseModel.Status = ex.ToString();
            }
            return responseModel;
        }

        public ResponseModel DeleteProviderCPTNote(long noteId, long v)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var note = ctx.Provider_Cpt_Plan_Notes.FirstOrDefault(p => p.Note_Id == noteId && (p.Deleted ?? false) == false);
                    if (note != null)
                    {
                        note.Deleted = true;
                        note.MODIFIED_BY = v;
                        note.MODIFIED_DATE = DateTime.Now;
                        ctx.Entry(note).State = System.Data.Entity.EntityState.Modified;
                        if (ctx.SaveChanges() > 0)
                        {
                            responseModel.Status = "Success";
                        }
                        else
                        {
                            responseModel.Status = "Error";
                        }
                    }
                    else
                    {
                        responseModel.Status = "Error";
                    }
                }
            }
            catch (Exception ex)
            {
                responseModel.Status = ex.ToString();
            }
            return responseModel;
        }

        #endregion

        #endregion FeeSchedule

        #region Procedure
        public ResponseModel GetProcedure(string procedureCode, string alternateCode)
        {
            ResponseModel objResponse = new ResponseModel();
            Procedure proc = null;
            if(alternateCode == null)
            {
                alternateCode = "";
            }
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    proc = ctx.Procedures.FirstOrDefault(p => p.ProcedureCode == procedureCode && p.Alternate_Code == alternateCode && (p.Deleted ?? false) == false);
                    if (proc != null)
                    {
                        objResponse.Status = "Success";
                        objResponse.Response = proc;
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
        public ResponseModel GetDropdownsListForProcedures()
        {
            ResponseModel responseModel = new ResponseModel();
            ProcedureDropdownListViewModel model = new ProcedureDropdownListViewModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    model.POS = ctx.Place_Of_Services.Where(p => (p.Deleted ?? false) == false).Select(p => new SelectListViewModel()
                    {
                        IdStr = p.posshortcode,
                        Name = p.POS_Code +" "+ p.POS_Name
                    }).ToList();
                    model.Modifiers = ctx.Modifiers.Where(m => (m.Deleted ?? false) == false).Select(m => new SelectListViewModel()
                    {
                        IdStr = m.Modifier_Code,
                        Name = m.Modifier_Description
                    }).ToList();
                    model.Category = ctx.NEW_CATEGORY.Where(c => (c.Deleted ?? false) == false).Select(c => new SelectListViewModel()
                    {
                        Id = c.Category_Id,
                        Name = c.Category_Desc
                    }).ToList();
                }
                if (model != null)
                {
                    responseModel.Status = "Success";
                    responseModel.Response = model;
                }
                else
                {
                    responseModel.Status = "Error";
                }
            }
            catch (Exception ex)
            {
                responseModel.Status = ex.ToString();
            }
            return responseModel;
        }

        //  check duplicate proc code and  alternate code in Procedures
        public ResponseModel DuplicateProcAndAlternateCode(string AlternateCode, string ProcCode)
        {
            ResponseModel res = new ResponseModel();
            if(AlternateCode == null)
            {
                AlternateCode = "";
            }
            using (var ctx = new NPMDBEntities())
            {
                //if (AlternateCode != null)
                //{

                var search = ctx.Procedures.Where(p => p.Alternate_Code == AlternateCode && p.ProcedureCode == ProcCode && (p.Deleted == true)).FirstOrDefault();
                if (search != null)
                {
                    res.Status = "Duplicate Procedure Code and Alternate Code combination with status deleted already exist.";
                }
                else
                {
                    search = ctx.Procedures.Where(p => p.Alternate_Code == AlternateCode && p.ProcedureCode == ProcCode && (p.Deleted == null || p.Deleted == false)).FirstOrDefault();
                    if (search != null)
                    {
                        res.Status = "Duplicate Procedure Code and Alternate Code combination. Please enter unique values.";
                    }
                }
                //}
                //else
                //{
                //    var search = ctx.Procedures.Where(p => p.ProcedureCode == ProcCode).FirstOrDefault();
                //    if (search != null)
                //    {
                //        res.Status = "Duplicate";
                //    }
                //}
            }
            return res;
        }

        public ResponseModel SaveProcedure(ProcedureViewModel model,long userId)
        {
            ResponseModel objResponse = new ResponseModel();
            Procedure procedure = new Procedure();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    if (!string.IsNullOrEmpty(model.ProcedureCode))
                    {
                         //var _procedure = ctx.Procedures.ToList();

                        procedure = ctx.Procedures.FirstOrDefault(t => t.ProcedureCode == model.oldProcCode && t.Alternate_Code == model.oldAlternateCode && (t.Deleted == null || t.Deleted == false));
                        if (procedure != null)
                        {
                            string sql = @"
    UPDATE Procedures
    SET 
        AgeCategory = @AgeCategory,
        AgeFrom = @AgeFrom,
        AgeRangeCriteria = @AgeRangeCriteria,
        AgeTo = CASE WHEN @AgeRangeCriteria != 'Between' THEN NULL ELSE @AgeTo END,
        CategoryId = @CategoryId,
        clia_number = @clia_number,
        Comments = @Comments,
        ComponentCode = @ComponentCode,
        Alternate_Code = @Alternate_Code,
        CPTDosage = @CPTDosage,
        ModifiedBy = @ModifiedBy,
        ModifiedDate = GETDATE(),
        Deleted = 0,
        EffectiveDate = @EffectiveDate,
        GenderAppliedOn = @GenderAppliedOn,
        IncludeInEDI = @IncludeInEDI,
        LongDescription = @LongDescription,
        MxUnits = @MxUnits,
        NOC = @NOC,
        ProcedureCode = @ProcedureCode, -- Uncomment if updating ProcedureCode
        ProcedureDefaultCharge = @ProcedureDefaultCharge,
        ProcedureDefaultModifier = @ProcedureDefaultModifier,
        ProcedureDescription = @ProcedureDescription,
        ProcedureEffectiveDate = @ProcedureEffectiveDate,
        ProcedurePosCode = @ProcedurePosCode
    WHERE 
        ProcedureCode = @OldProcedureCode 
        AND Alternate_Code = @OldAlternateCode 
        AND (Deleted IS NULL OR Deleted = 0);
";

                            int rowsAffected = ctx.Database.ExecuteSqlCommand(
                                sql,
                                new SqlParameter("@AgeCategory", model.AgeCategory ?? (object)DBNull.Value),
                                new SqlParameter("@AgeFrom", model.AgeFrom ?? (object)DBNull.Value),
                                new SqlParameter("@AgeRangeCriteria", model.AgeRangeCriteria ?? (object)DBNull.Value),
                                new SqlParameter("@AgeTo", model.AgeRangeCriteria != "Between" ? (object)DBNull.Value : model.AgeTo ?? (object)DBNull.Value),
                                new SqlParameter("@CategoryId", model.CategoryId ?? (object)DBNull.Value),
                                new SqlParameter("@clia_number", model.clia_number ?? (object)DBNull.Value),
                                new SqlParameter("@Comments", model.Comments ?? (object)DBNull.Value),
                                new SqlParameter("@ComponentCode", model.ComponentCode ?? (object)DBNull.Value),
                                new SqlParameter("@Alternate_Code", model.Alternate_Code ?? (object)DBNull.Value),
                                new SqlParameter("@CPTDosage", model.CPTDosage ?? (object)DBNull.Value),
                                new SqlParameter("@ModifiedBy", userId),
                                new SqlParameter("@EffectiveDate", model.EffectiveDate ?? (object)DBNull.Value),
                                new SqlParameter("@GenderAppliedOn", model.GenderAppliedOn ?? (object)DBNull.Value),
                                new SqlParameter("@IncludeInEDI", model.IncludeInEDI ?? (object)DBNull.Value),
                                new SqlParameter("@LongDescription", model.LongDescription ?? (object)DBNull.Value),
                                new SqlParameter("@MxUnits", model.MxUnits ?? (object)DBNull.Value),
                                new SqlParameter("@NOC", model.NOC ?? (object)DBNull.Value),
                                new SqlParameter("@ProcedureCode", model.ProcedureCode ?? (object)DBNull.Value), // Uncomment if needed
                                new SqlParameter("@ProcedureDefaultCharge", model.ProcedureDefaultCharge ?? (object)DBNull.Value),
                                new SqlParameter("@ProcedureDefaultModifier", model.ProcedureDefaultModifier ?? (object)DBNull.Value),
                                new SqlParameter("@ProcedureDescription", model.ProcedureDescription ?? (object)DBNull.Value),
                                new SqlParameter("@ProcedureEffectiveDate", model.ProcedureEffectiveDate ?? (object)DBNull.Value),
                                new SqlParameter("@ProcedurePosCode", model.ProcedurePosCode ?? (object)DBNull.Value),
                                new SqlParameter("@OldProcedureCode", model.oldProcCode),
                                new SqlParameter("@OldAlternateCode", model.oldAlternateCode)
                            );

                            if (rowsAffected > 0)
                            {
                                Console.WriteLine("Update successful!");
                                objResponse.Status = "Success";
                            }
                            else
                            {
                                Console.WriteLine("No records updated. Check your filter conditions.");
                            }

                            //procedure.AgeCategory = model.AgeCategory;
                            //procedure.AgeFrom = model.AgeFrom;
                            //procedure.AgeRangeCriteria = model.AgeRangeCriteria;
                            //procedure.AgeTo = procedure.AgeRangeCriteria != "Between" ? null : model.AgeTo;
                            //procedure.CategoryId = model.CategoryId;
                            //procedure.clia_number = model.clia_number;
                            //procedure.Comments = model.Comments;
                            //procedure.ComponentCode = model.ComponentCode;
                            //procedure.Alternate_Code = model.Alternate_Code;
                            //procedure.CPTDosage = model.CPTDosage;
                            //procedure.ModifiedBy = userId;
                            //procedure.ModifiedDate = DateTime.Now;
                            //procedure.Deleted = false;
                            //procedure.EffectiveDate = model.EffectiveDate;
                            //procedure.GenderAppliedOn = model.GenderAppliedOn;
                            //procedure.IncludeInEDI = model.IncludeInEDI;
                            //procedure.LongDescription = model.LongDescription;
                            //procedure.MxUnits = model.MxUnits;
                            //procedure.NOC = model.NOC;
                            ////procedure.ProcedureCode = model.ProcedureCode;
                            //procedure.ProcedureDefaultCharge = model.ProcedureDefaultCharge;
                            //procedure.ProcedureDefaultModifier = model.ProcedureDefaultModifier;
                            //procedure.ProcedureDescription = model.ProcedureDescription;
                            //procedure.ProcedureEffectiveDate = model.ProcedureEffectiveDate;
                            //procedure.ProcedurePosCode = model.ProcedurePosCode;

                            //ctx.Entry(procedure).State = System.Data.Entity.EntityState.Modified;
                        }
                        else
                        {
                            procedure = new Procedure()
                            {
                                AgeCategory = model.AgeCategory,
                                AgeFrom = model.AgeFrom,
                                AgeRangeCriteria = model.AgeRangeCriteria,
                                AgeTo = model.AgeTo,
                                CategoryId = model.CategoryId,
                                clia_number = model.clia_number,
                                Comments = model.Comments,
                                ComponentCode = model.ComponentCode,
                                Alternate_Code = model.Alternate_Code,
                                CPTDosage = model.CPTDosage,
                                CreatedBy = userId,
                                CreatedDate = DateTime.Now,
                                Deleted = false,
                                EffectiveDate = model.EffectiveDate,
                                GenderAppliedOn = model.GenderAppliedOn,
                                IncludeInEDI = model.IncludeInEDI,
                                LongDescription = model.LongDescription,
                                MxUnits = model.MxUnits,
                                NOC = model.NOC,
                                ProcedureCode = model.ProcedureCode,
                                ProcedureDefaultCharge = model.ProcedureDefaultCharge,
                                ProcedureDefaultModifier = model.ProcedureDefaultModifier,
                                ProcedureDescription = model.ProcedureDescription,
                                ProcedureEffectiveDate = model.ProcedureEffectiveDate,
                                ProcedurePosCode = model.ProcedurePosCode,
                            Qualifier = model.Qualifier,
                                TimeMin = model.TimeMin
                            };
                            ctx.Procedures.Add(procedure);
                        }
                    }
                    else
                    {
                        procedure = new Procedure()
                        {
                            AgeCategory = model.AgeCategory,
                            AgeFrom = model.AgeFrom,
                            AgeRangeCriteria = model.AgeRangeCriteria,
                            AgeTo = model.AgeTo,
                            CategoryId = model.CategoryId,
                            clia_number = model.clia_number,
                            Comments = model.Comments,
                            ComponentCode = model.ComponentCode,
                            Alternate_Code = model.Alternate_Code,
                            CPTDosage = model.CPTDosage,
                            CreatedBy = userId,
                            CreatedDate = DateTime.Now,
                            Deleted = false,
                            EffectiveDate = model.EffectiveDate,
                            GenderAppliedOn = model.GenderAppliedOn,
                            IncludeInEDI = model.IncludeInEDI,
                            LongDescription = model.LongDescription,
                            MxUnits = model.MxUnits,
                            NOC = model.NOC,
                            ProcedureCode = model.ProcedureCode,
                            ProcedureDefaultCharge = model.ProcedureDefaultCharge,
                            ProcedureDefaultModifier = model.ProcedureDefaultModifier,
                            ProcedureDescription = model.ProcedureDescription,
                            ProcedureEffectiveDate = model.ProcedureEffectiveDate,
                            ProcedurePosCode = model.ProcedurePosCode,
                            Qualifier = model.Qualifier,
                            TimeMin = model.TimeMin
                        };
                        ctx.Procedures.Add(procedure);
                    }
                    if (ctx.SaveChanges() > 0)
                    {
                        objResponse.Status = "Success";
                    }
                }

            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                // Capture entity validation errors
                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        objResponse.Status += $"Property: {validationError.PropertyName}, Error: {validationError.ErrorMessage} \n";
                    }
                }
            }

            catch (Exception ex)
            {
                objResponse.Status = ex.ToString();
            }
            return objResponse;
        }
        public ResponseModel DeleteProcedure(string procedureCode, string alternateCode, long userId)
        {
            ResponseModel objResponse = new ResponseModel();
            Procedure proc = null;
            if(alternateCode == null)
            {
                alternateCode = "";
            }
            try
            {
                if (!string.IsNullOrEmpty(procedureCode))
                {
                    using (var ctx = new NPMDBEntities())
                    {
                        proc = ctx.Procedures.FirstOrDefault(p => p.ProcedureCode == procedureCode && p.Alternate_Code == alternateCode); ;
                        if (proc != null)
                        {
                            proc.Deleted = true;
                            proc.ModifiedBy = userId;
                            proc.ModifiedDate = DateTime.Now;
                            ctx.Entry(proc).State = System.Data.Entity.EntityState.Modified;
                            if (ctx.SaveChanges() > 0)
                            {
                                objResponse.Status = "Success";
                            }
                        }
                        else
                        {
                            objResponse.Status = "No Procedures found.";
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
        public ResponseModel SearchProcedures(ProceduresSearchViewModel model)
        {
            ResponseModel objResponse = new ResponseModel();
            List<SP_ProcedureSearch_Result> procedureList = null;
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    procedureList = ctx.SP_ProcedureSearch(model.ProcedureCode, model.ProcedureDescription).ToList();
                }

                if (procedureList != null)
                {
                    objResponse.Status = "Success";
                    objResponse.Response = procedureList;
                }
                else
                {
                    objResponse.Status = "Error";
                }
            }
            catch (Exception ex)
            {
                objResponse.Status = ex.ToString();
            }
            return objResponse;
        }

      



        #endregion Procedure
    }
}