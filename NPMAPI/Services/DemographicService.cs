using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Web.Hosting;
using System.Web.Http;
using System.Xml;
using Newtonsoft.Json;
using NPMAPI.App_Start;
using NPMAPI.com.gatewayedi.services;
using NPMAPI.Enums;
using NPMAPI.Models;
using NPMAPI.Models.InboxHealth;
using NPMAPI.Models.ViewModels;
using System.Diagnostics;
using NPMAPI.com.gatewayedi.services;
using NPMAPI.Models.InboxHealth;
using Newtonsoft.Json;
using NPMAPI.Repositories;
using System.Web.Optimization;
using static iTextSharp.text.pdf.AcroFields;
using EdiFabric.Core.Model.Edi.X12;
using EdiFabric.Templates.Hipaa5010;
using System.Web.UI.WebControls;
using System.Globalization;
using Microsoft.AspNet.SignalR.Messaging;
using NPOI.SS.Formula.Functions;
using NPOI.Util;
using System.Web.Http.Results;
using System.Threading.Tasks;
using System.Reflection.Emit;

namespace NPMAPI.Services
{
    public partial class DemographicService : IDemographicRepository 
    {
        private readonly IFTP ftp;
        private readonly IPracticeRepository practiceRepository;
        private IEligibility _iEligibility;
        private IDeltaSyncRepository _deltaSyncRepository;
        private readonly IScrubberRepository _scrubberService;


        public DemographicService(IFTP _ftp, IPracticeRepository _practiceRepository, IEligibility iEligibility, IDeltaSyncRepository deltaSyncRepository,IScrubberRepository scrubberRepository , IScrubberRepository scrubberService)
        {
            ftp = _ftp;
            practiceRepository = _practiceRepository;
            _iEligibility = iEligibility;
            _deltaSyncRepository = deltaSyncRepository;
            _scrubberService = scrubberService;


        }

    
        public ResponseModel SearchPatient([FromBody] PatientSearchModel SearchModel)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                List<SP_PatientSearch_Result> objPatientList = null;
                using (var ctx = new NPMDBEntities())
                {
                    if (SearchModel.inActive)  // implement in sp this deleted feature
                        objPatientList = ctx.SP_PatientSearch(SearchModel.PracticeCode.ToString(), SearchModel.PatientAccount.ToString(), (SearchModel.FirstName == null ? "" : SearchModel.FirstName), (SearchModel.LastName == null ? "" : SearchModel.LastName), (SearchModel.SSN == null ? "" : SearchModel.SSN), (SearchModel.HomePhone == null ? "" : SearchModel.HomePhone), (SearchModel.ZIP == null ? "" : SearchModel.ZIP), SearchModel.ClaimNo.ToString(), (SearchModel.PolicyNo == null ? "" : SearchModel.PolicyNo), SearchModel.IncludePTLPatients, SearchModel.inActive, SearchModel.dob, SearchModel.dateFrom, SearchModel.dateTo, SearchModel.dateType).ToList();
                    else
                        objPatientList = ctx.SP_PatientSearch(SearchModel.PracticeCode.ToString(), SearchModel.PatientAccount.ToString(), (SearchModel.FirstName == null ? "" : SearchModel.FirstName), (SearchModel.LastName == null ? "" : SearchModel.LastName), (SearchModel.SSN == null ? "" : SearchModel.SSN), (SearchModel.HomePhone == null ? "" : SearchModel.HomePhone), (SearchModel.ZIP == null ? "" : SearchModel.ZIP), SearchModel.ClaimNo.ToString(), (SearchModel.PolicyNo == null ? "" : SearchModel.PolicyNo), SearchModel.IncludePTLPatients, SearchModel.inActive, SearchModel.dob, SearchModel.dateFrom, SearchModel.dateTo, SearchModel.dateType).ToList();
                }

                if (objPatientList != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = objPatientList;
                }
                else
                {
                    objResponse.Status = "No Data Found";
                }
              //  return objResponse;
            }
            catch (Exception)
            {
            
                objResponse.Status = "Invalid";
               // throw;
            }
            return objResponse;
        }

        public ResponseModel GetPatientList()
        {
            ResponseModel objResponse = new ResponseModel();
            List<Models.Patient> objPatientList = null;
            using (var ctx = new NPMDBEntities())
            {
                objPatientList = ctx.Patients.ToList();
            }

            if (objPatientList != null)
            {
                objResponse.Status = "Sucess";
                objResponse.Response = objPatientList;
            }
            else
            {
                objResponse.Status = "Error";
            }
            return objResponse;
        }

        public ResponseModel IsPatientAlreadyExist(DublicatePatientCheckModel model)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var patients = (from p in ctx.Patients
                                    where p.First_Name == model.FirstName.Trim() &&
                                    p.Last_Name == model.LastName.Trim() &&
                                    p.ZIP == model.Zip.Trim()
                                    && p.Date_Of_Birth == model.DOB
                                    && p.Practice_Code == model.PracticeCode
                                    && p.Deleted == false
                                    select p).ToList();
                    long[] pp = patients.Select(p => p.Patient_Account).ToArray();
                    // new user
                    if (model.PatientAccount == 0 && patients.Count() > 0)
                        objResponse.Response = true;
                    // edit user
                    else if (model.PatientAccount > 0 && !pp.Contains(model.PatientAccount) && patients.Count() > 0)
                        objResponse.Response = true;
                    else
                        objResponse.Response = false;
                    objResponse.Status = "Success";
                }
            }
            catch (Exception ex)
            {
                objResponse.Status = ex.Message.ToString() + "\r\n" + ex.InnerException.Message.ToString();
            }
            return objResponse;
        }
        public ResponseModel checkDuplicateDOS(long patientAccount,DateTime dos )
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var dosList = (from c in ctx.Claims
                                    where c.Patient_Account == patientAccount &&
                                    c.DOS == dos
                                    select c).ToList();
                    // if any duplicate dos is found
                    if (dosList.Count > 0 && dosList != null)
                        objResponse.Response = true;
                    else
                        objResponse.Response = false;
                    objResponse.Status = "Success";
                }
            }
            catch (Exception ex)
            {
                objResponse.Status = ex.Message.ToString() + "\r\n" + ex.InnerException.Message.ToString();
            }
            return objResponse;
        }

        public ResponseModel GetPatientModel(long practiceCode)
        {
            ResponseModel objResponse = new ResponseModel();
            PatientDemographicViewModel patientDemoModel = new PatientDemographicViewModel();
            using (var ctx = new NPMDBEntities())
            {
                patientDemoModel.GenderList = ctx.Genders.Where(r => !(r.Deleted ?? false)).ToList();
                patientDemoModel.MaritalStatusList = ctx.MaritalStatus.Where(r => !(r.Deleted ?? false)).ToList();
                patientDemoModel.RaceList = ctx.Races.Where(r => !(r.Deleted ?? false)).ToList();
                patientDemoModel.EthiniciesList = ctx.ethnicities.Where(r => !(r.deleted ?? false)).ToList();

                patientDemoModel.LanguageList = ctx.Languages.Where(r => !(r.Deleted ?? false)).ToList();
                patientDemoModel.AddressTypeList = ctx.AddressTypes.Where(r => !(r.Deleted ?? false)).ToList();
                patientDemoModel.RelationshipList = ctx.Relationships.Where(r => !(r.Deleted ?? false)).ToList();

                patientDemoModel.ProviderList = ctx.Providers.Where(r => (r.Deleted ?? false) == false && r.Practice_Code == practiceCode).ToList();
                patientDemoModel.PracticeLocationsList = ctx.Practice_Locations.Where(r => (r.Deleted ?? false) == false && r.Practice_Code == practiceCode).ToList();
                patientDemoModel.ReferringPhysicianList = ctx.Referral_Physicians.Where(r => !(r.Deleted ?? false)).ToList();
            }

            if (patientDemoModel != null)
            {
                objResponse.Status = "Sucess";
                objResponse.Response = patientDemoModel;
            }
            else
            {
                objResponse.Status = "Error";
            }
            return objResponse;

        }

        public ResponseModel GetPatient(long PatientAccount)
        {
            ResponseModel objResponse = new ResponseModel();
            PatientDemographicViewModel patientDemoModel = new PatientDemographicViewModel();
            using (var ctx = new NPMDBEntities())
            {
                patientDemoModel = ctx.Patients.Where(p => p.Patient_Account == PatientAccount)
                            .Select(s => new PatientDemographicViewModel()
                            {
                                Patient_Account = s.Patient_Account,
                                alternate_id = s.alternate_id,
                                First_Name = s.First_Name,
                                Last_Name = s.Last_Name,
                                MI = s.MI,
                                Date_Of_Birth = s.Date_Of_Birth,
                                SSN = s.SSN,
                                Gender = s.Gender,
                                Marital_Status = s.Marital_Status,
                                Race = s.Race,
                                Address = s.Address,
                                Address_Type = s.Address_Type,
                                ZIP = s.ZIP,
                                City = s.City,
                                State = s.State,
                                Home_Phone = s.Home_Phone,
                                Cell_Phone = s.Cell_Phone,
                                Business_Phone = s.Business_Phone,
                                Email_Address = s.Email_Address,
                                Family_Name = s.Family_Name,
                                Fam_Relation = s.Fam_Relation,
                                Father_Cell = s.Father_Cell,
                                Financial_Guarantor = s.Financial_Guarantor,
                                Gurantor_Relation = s.Gurantor_Relation,
                                IsDeceased = s.IsDeceased,
                                Expiry_Date = s.Expiry_Date,
                                Provider_Code = s.Provider_Code,
                                Location_Code = s.Location_Code,
                                Referring_Physician = s.Referring_Physician,
                                Family_Id = s.Family_Id + "",
                                DeathDate = s.DeathDate,
                                Financial_Guarantor_Name = s.Financial_Guarantor_Name,
                                Ethnicities = s.Ethnicities,
                                Languages = s.Languages,
                                Practice_Code = s.Practice_Code,
                                emailnotonfile = (s.emailnotonfile ?? false),
                                PTL_STATUS = (s.PTL_STATUS ?? false),
                                PicturePath = s.PicturePath,
                                Hold_pt_until=s.Hold_pt_until,
                                Hold_Pt_Billing= (s.Hold_Pt_Billing ?? false)


                            }).SingleOrDefault<PatientDemographicViewModel>();

                if (patientDemoModel.MI == "" || patientDemoModel.MI == " ")
                {

                    patientDemoModel.MI = null;
                }

                patientDemoModel.GenderList = ctx.Genders.Where(r => !(r.Deleted ?? false)).ToList();
                patientDemoModel.MaritalStatusList = ctx.MaritalStatus.Where(r => !(r.Deleted ?? false)).ToList();
                patientDemoModel.RaceList = ctx.Races.Where(r => !(r.Deleted ?? false)).ToList();
                patientDemoModel.EthiniciesList = ctx.ethnicities.Where(r => !(r.deleted ?? false)).ToList();

                patientDemoModel.LanguageList = ctx.Languages.Where(r => !(r.Deleted ?? false)).ToList();
                patientDemoModel.AddressTypeList = ctx.AddressTypes.Where(r => !(r.Deleted ?? false)).ToList();
                patientDemoModel.RelationshipList = ctx.Relationships.Where(r => !(r.Deleted ?? false)).ToList();

                patientDemoModel.ProviderList = ctx.Providers.Where(r => (r.Deleted ?? false) == false && r.Practice_Code == patientDemoModel.Practice_Code).ToList();
                patientDemoModel.PracticeLocationsList = ctx.Practice_Locations.Where(r => (r.Deleted ?? false) == false && r.Practice_Code == patientDemoModel.Practice_Code).ToList();
                patientDemoModel.ReferringPhysicianList = ctx.Referral_Physicians.Where(r => !(r.Deleted ?? false)).ToList();
                if (!string.IsNullOrEmpty(patientDemoModel.ZIP))
                    patientDemoModel.ZipCodeCities = GetCitiesByZipCode(patientDemoModel.ZIP).Response;
                else
                    patientDemoModel.ZipCodeCities = new List<CityStateModel>();
                patientDemoModel.PatientInsuranceList = new List<PatientInsuranceViewModel>(); // 

                var patInsurances = ctx.SP_PATIENTINSSERACH(patientDemoModel.Patient_Account).ToList();
                if (patInsurances != null)
                {
                    foreach (var item in patInsurances)
                    {
                        PatientInsuranceViewModel objpatinsvmModel = new PatientInsuranceViewModel();
                        objpatinsvmModel.Access_Carolina_Number = item.Access_Carolina_Number;
                        objpatinsvmModel.Allowed_Visits = item.Allowed_Visits;
                        objpatinsvmModel.CCN = item.CCN;
                        objpatinsvmModel.coverage_description = item.Access_Carolina_Number;
                        objpatinsvmModel.Co_Payment = item.Co_Payment;
                        objpatinsvmModel.Co_Payment_Per = item.Co_Payment_Per;
                        objpatinsvmModel.Created_By = item.Created_By;
                        objpatinsvmModel.Created_Date = item.Created_Date;
                        objpatinsvmModel.Created_From = item.Created_From;
                        objpatinsvmModel.Deductions = item.Deductions;
                        objpatinsvmModel.Deleted = item.Deleted;
                        objpatinsvmModel.Effective_Date = item.Effective_Date;
                        objpatinsvmModel.Eligibility_Difference = item.Eligibility_Difference;
                        objpatinsvmModel.Eligibility_Enquiry_Date = item.Eligibility_Enquiry_Date;
                        objpatinsvmModel.Eligibility_Status = item.Eligibility_Status;

                        objpatinsvmModel.Eligibility_S_No = item.Eligibility_S_No;
                        objpatinsvmModel.Filing_Indicator = item.Filing_Indicator;
                        objpatinsvmModel.Filing_Indicator_Code = item.Filing_Indicator_Code;
                        objpatinsvmModel.Group_Name = item.Group_Name;
                        objpatinsvmModel.Group_Number = item.Group_Number;
                        objpatinsvmModel.Insurance_Id = item.Insurance_Id;
                        objpatinsvmModel.Is_Capitated_Patient = item.Is_Capitated_Patient;
                        objpatinsvmModel.MCR_Sec_Payer = item.MCR_Sec_Payer;
                        objpatinsvmModel.MCR_Sec_Payer_Code = item.MCR_Sec_Payer_Code;
                        objpatinsvmModel.Modified_By = item.Modified_By;
                        objpatinsvmModel.Modified_Date = item.Modified_Date;
                        objpatinsvmModel.Patient_Account = item.Patient_Account;
                        objpatinsvmModel.Patient_Insurance_Id = item.Patient_Insurance_Id;
                        objpatinsvmModel.PayerDescription = item.PayerDescription;
                        objpatinsvmModel.Plan_Name = item.Plan_Name;
                        objpatinsvmModel.Plan_Name_Type = item.Plan_Name_Type;

                        objpatinsvmModel.Plan_type = item.Plan_type;
                        objpatinsvmModel.Policy_Number = item.Policy_Number;
                        objpatinsvmModel.Pri_Sec_Oth_Type = item.Pri_Sec_Oth_Type;
                        objpatinsvmModel.Relationship = item.Relationship;
                        objpatinsvmModel.Remaining_Visits = item.Remaining_Visits;
                        objpatinsvmModel.Subscriber = item.Subscriber;
                        objpatinsvmModel.Termination_Date = item.Termination_Date;
                        objpatinsvmModel.Visits_End_Date = item.Visits_End_Date;
                        objpatinsvmModel.Visits_Start_Date = item.Visits_Start_Date;
                        objpatinsvmModel.IS_Active = item.IS_Active;


                        objpatinsvmModel.SubscriberName = item.SubscriberName;

                        patientDemoModel.PatientInsuranceList.Add(objpatinsvmModel);
                    }

                }

               

                foreach (var item in patientDemoModel.PatientInsuranceList)
                {
                    if (item.Pri_Sec_Oth_Type == "P" && (item.Insurance_Id != 0))
                    {
                        var primaryResult = ctx.FindInsuranceName(item.Insurance_Id);
                        patientDemoModel.PrimaryInsuranceName = (primaryResult == null ? "" : primaryResult.FirstOrDefault().ToString());
                    }
                    else if (item.Pri_Sec_Oth_Type == "S" && (item.Insurance_Id != 0))
                    {
                        var secondaryResult = ctx.FindInsuranceName(item.Insurance_Id);
                        patientDemoModel.SecondaryInsuranceName = (secondaryResult == null ? "" : secondaryResult.FirstOrDefault().ToString());
                    }
                    else if (item.Pri_Sec_Oth_Type == "O" && (item.Insurance_Id != 0))
                    {
                        var otherResult = ctx.FindInsuranceName(item.Insurance_Id);
                        patientDemoModel.OtherInsuranceName = (otherResult == null ? "" : otherResult.FirstOrDefault().ToString());
                    }
                }
            }
            //if (patientDemoModel.MI == "")
            //{

            //    patientDemoModel.MI = null;
            //}

            var a = patientDemoModel.MI;

            if (patientDemoModel != null)
            {
                objResponse.Status = "Sucess";
                objResponse.Response = patientDemoModel;
            }
            else
            {
                objResponse.Status = "Error";
            }
            return objResponse;

        }
        public ResponseModel DeletePatient(long PatientAccount)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {

                using (var ctx = new NPMDBEntities())
                {

                    Models.Patient objPatient = ctx.Patients.SingleOrDefault(c => c.Patient_Account == PatientAccount);
                    if (objPatient != null)
                    {
                        objPatient.Deleted = true;
                        ctx.SaveChanges();
                    }

                    if (objPatient != null)
                    {
                        objResponse.Status = "Sucess";
                        //objResponse.Response = FacilitiesList;
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

        public ResponseModel GetFinancialGurantor(string Name)
        {
            ResponseModel objResponse = new ResponseModel();
            List<Guarantor> objPatientGurantorsList = null;
            using (var ctx = new NPMDBEntities())
            {
                objPatientGurantorsList = ctx.Guarantors.Where(g => g.Guarant_Fname.StartsWith(Name) || g.Guarant_Lname.StartsWith(Name) || g.Guarant_Ssn.StartsWith(Name) || g.Guarant_Home_Phone.StartsWith(Name)).Take(5).ToList();
            }

            if (objPatientGurantorsList != null)
            {
                objResponse.Status = "Sucess";
                objResponse.Response = objPatientGurantorsList;
            }
            else
            {
                objResponse.Status = "Error";
            }
            return objResponse;
        }


        public ResponseModel GetFinancialGurantorSubscriber(string Name)
        {
            ResponseModel objResponse = new ResponseModel();
            List<Guarantor> objPatientGurantorsList = null;
            using (var ctx = new NPMDBEntities())
            {
                objPatientGurantorsList = ctx.Guarantors.Where(g => g.Guarant_Type == "S" && (g.Deleted == null || g.Deleted == false) || g.Guarant_Fname.StartsWith(Name) || g.Guarant_Lname.StartsWith(Name) || g.Guarant_Ssn.StartsWith(Name) || g.Guarant_Home_Phone.StartsWith(Name)).Take(5).ToList();
            }

            if (objPatientGurantorsList != null)
            {
                objResponse.Status = "Sucess";
                objResponse.Response = objPatientGurantorsList;
            }
            else
            {
                objResponse.Status = "Error";
            }
            return objResponse;
        }

        public ResponseModel AddEditPatient([FromBody] PatientCreateViewModel PatientModel, long UserId)
        {
            long result = 0;
            ResponseModel objResponse = new ResponseModel();
            PatientDemographicViewModel patientDemoModel = new PatientDemographicViewModel();
            Models.Patient objNewPatient = null;
            var syncedPractice = _deltaSyncRepository.GetSyncedPractice(PatientModel.Practice_Code);
            using (var ctx = new NPMDBEntities())
            {
                objNewPatient = ctx.Patients.SingleOrDefault(p => p.Patient_Account == PatientModel.Patient_Account);
                if (objNewPatient == null || PatientModel.Patient_Account == 0)
                {
                    long patAccount = Convert.ToInt64(ctx.SP_TableIdGenerator("Patient_Account").FirstOrDefault().ToString());
                    objNewPatient = new Models.Patient();
                    objNewPatient.Patient_Account = Convert.ToInt64(PatientModel.Practice_Code.ToString() + patAccount.ToString());
                    objNewPatient.First_Name = PatientModel.First_Name.Trim();
                    objNewPatient.Last_Name = PatientModel.Last_Name.Trim();
                    objNewPatient.MI = PatientModel.MI?.Trim();
                    objNewPatient.Date_Of_Birth = PatientModel.Date_Of_Birth;
                    objNewPatient.SSN = PatientModel.SSN?.Trim();
                    objNewPatient.Gender = PatientModel.Gender;
                    objNewPatient.Marital_Status = PatientModel.Marital_Status;
                    objNewPatient.Race = PatientModel.Race;
                    objNewPatient.Address = PatientModel.Address?.Trim();
                    objNewPatient.Address_Type = (PatientModel.Address_Type != null ? PatientModel.Address_Type : 0);
                    objNewPatient.ZIP = PatientModel.ZIP?.Trim();
                    objNewPatient.City = PatientModel.City?.Trim();
                    objNewPatient.State = PatientModel.State?.Trim();
                    objNewPatient.Home_Phone = PatientModel.Home_Phone?.Trim();
                    objNewPatient.Cell_Phone = PatientModel.Cell_Phone?.Trim();
                    objNewPatient.Business_Phone = PatientModel.Business_Phone?.Trim();
                    objNewPatient.Email_Address = PatientModel.Email_Address?.Trim();
                    objNewPatient.Family_Name = PatientModel.Family_Name?.Trim();
                    objNewPatient.Family_Id = PatientModel.Family_Id;
                    objNewPatient.Father_Cell = PatientModel.Father_Cell?.Trim();
                    objNewPatient.Financial_Guarantor = PatientModel.Financial_Guarantor;
                    objNewPatient.Gurantor_Relation = PatientModel.Gurantor_Relation;
                    objNewPatient.IsDeceased = PatientModel.IsDeceased;
                    objNewPatient.DeathDate = PatientModel.DeathDate;
                    //Added BY Hamza Akhlaq For Hold Patient Billing
                    objNewPatient.Hold_Pt_Billing = PatientModel.Hold_Pt_Billing;
                    objNewPatient.Hold_pt_until = PatientModel.Hold_pt_until;
                    objNewPatient.Expiry_Date = PatientModel.Expiry_Date;
                    objNewPatient.Provider_Code = PatientModel.Provider_Code;
                    objNewPatient.Location_Code = PatientModel.Location_Code;
                    objNewPatient.Referring_Physician = PatientModel.Referring_Physician;
                    objNewPatient.Languages = PatientModel.Languages;
                    objNewPatient.Practice_Code = PatientModel.Practice_Code;
                    objNewPatient.Financial_Guarantor_Name = PatientModel.Financial_Guarantor_Name?.Trim();
                    objNewPatient.Ethnicities = PatientModel.Ethnicities;
                    objNewPatient.PTL_STATUS = PatientModel.PTL_STATUS;
                    objNewPatient.Deleted = false;
                    objNewPatient.Created_Date = DateTime.Now;
                    objNewPatient.Created_By = UserId;
                    objNewPatient.emailnotonfile = string.IsNullOrEmpty(PatientModel.Email_Address) ? PatientModel.emailnotonfile : false;
                    objNewPatient.PicturePath = !string.IsNullOrEmpty(PatientModel.PicturePath) ? PatientModel.PicturePath : null;
                    objNewPatient.alternate_id = PatientModel.alternate_id;

                    if (PatientModel.PatientInsuranceList != null && PatientModel.PatientInsuranceList.Count > 0)
                    {
                        foreach (var item in PatientModel.PatientInsuranceList)
                        {
                            if (item != null)
                            {
                                Patient_Insurance objNewPatientInsurance = new Patient_Insurance();
                                long patInsuranceId = Convert.ToInt64(ctx.SP_TableIdGenerator("Patient_Insurance_Id").FirstOrDefault().ToString());//
                                objNewPatientInsurance.Patient_Insurance_Id = patInsuranceId;
                                objNewPatientInsurance.Insurance_Id = item.Insurance_Id;
                                objNewPatientInsurance.Pri_Sec_Oth_Type = item.Pri_Sec_Oth_Type;
                                objNewPatientInsurance.Policy_Number = item.Policy_Number;
                                objNewPatientInsurance.Subscriber = item.Subscriber;
                                objNewPatientInsurance.Relationship = item.Relationship;
                                objNewPatientInsurance.Co_Payment = item.Co_Payment;
                                objNewPatientInsurance.Group_Number = item.Group_Number;
                                objNewPatientInsurance.Group_Name = item.Group_Name;
                                objNewPatientInsurance.Deductions = item.Deductions;
                                objNewPatientInsurance.Co_Payment_Per = item.Co_Payment_Per;
                                objNewPatientInsurance.CCN = item.CCN;
                                objNewPatientInsurance.Visits_Start_Date = item.Visits_Start_Date;
                                objNewPatientInsurance.Visits_End_Date = item.Visits_End_Date;
                                objNewPatientInsurance.Access_Carolina_Number = item.Access_Carolina_Number;
                                objNewPatientInsurance.Is_Capitated_Patient = item.Is_Capitated_Patient;
                                objNewPatientInsurance.IS_Active = item.IS_Active;
                                ctx.Patient_Insurance.Add(objNewPatientInsurance);
                                ctx.SaveChanges();
                            }
                        }
                    }
                    var entity = ctx.Patients.Add(objNewPatient);
                    ctx.SaveChanges();
                    result = entity.Patient_Account;
                    //Check if the patient practice is already synced with inbox then set the completedAt column of PracticeSynchronization table to null
                    //so that the next time the sync worker runs, it add this new patient, if it's due amount is > 0
                    if (syncedPractice != null)
                    {
                        PracticeSynchronization practiceSynchronization = ctx.PracticeSynchronizations.Where(x => x.PracticeId == syncedPractice.Practice_Code).FirstOrDefault();
                        practiceSynchronization.UpdatedDate = DateTime.Now;
                        ctx.SaveChanges();
                    }
                }
                else
                {
                    objNewPatient.First_Name = PatientModel.First_Name.Trim();
                    objNewPatient.Last_Name = PatientModel.Last_Name.Trim();
                    objNewPatient.MI = PatientModel.MI?.Trim();
                    objNewPatient.Date_Of_Birth = PatientModel.Date_Of_Birth;
                    objNewPatient.SSN = PatientModel.SSN?.Trim();
                    objNewPatient.Gender = PatientModel.Gender;
                    objNewPatient.Marital_Status = PatientModel.Marital_Status;
                    objNewPatient.Race = PatientModel.Race;
                    objNewPatient.Address = PatientModel.Address?.Trim();
                    objNewPatient.Address_Type = PatientModel.Address_Type;
                    objNewPatient.ZIP = PatientModel.ZIP?.Trim();
                    objNewPatient.City = PatientModel.City?.Trim();
                    objNewPatient.State = PatientModel.State?.Trim();
                    objNewPatient.Home_Phone = PatientModel.Home_Phone?.Trim();
                    objNewPatient.Cell_Phone = PatientModel.Cell_Phone?.Trim();
                    objNewPatient.Business_Phone = PatientModel.Business_Phone?.Trim();
                    objNewPatient.Email_Address = PatientModel.Email_Address?.Trim();
                    objNewPatient.Family_Name = PatientModel.Family_Name?.Trim();
                    objNewPatient.Family_Id = PatientModel.Family_Id;
                    objNewPatient.Father_Cell = PatientModel.Father_Cell?.Trim();
                    objNewPatient.Financial_Guarantor = PatientModel.Financial_Guarantor;
                    objNewPatient.Gurantor_Relation = PatientModel.Gurantor_Relation;
                    objNewPatient.IsDeceased = PatientModel.IsDeceased;
                    objNewPatient.DeathDate = PatientModel.DeathDate;
                    //Added By Hamza Akhlaq for Hold Patient Billing
                    objNewPatient.Hold_Pt_Billing = PatientModel.Hold_Pt_Billing;
                    objNewPatient.Hold_pt_until = PatientModel.Hold_pt_until;
                    objNewPatient.Expiry_Date = PatientModel.Expiry_Date;
                    objNewPatient.Provider_Code = PatientModel.Provider_Code;
                    objNewPatient.Location_Code = PatientModel.Location_Code;
                    objNewPatient.Referring_Physician = PatientModel.Referring_Physician;
                    objNewPatient.Languages = PatientModel.Languages;
                    objNewPatient.PTL_STATUS = PatientModel.PTL_STATUS;
                    objNewPatient.Financial_Guarantor_Name = PatientModel.Financial_Guarantor_Name?.Trim();
                    objNewPatient.Ethnicities = PatientModel.Ethnicities;
                    objNewPatient.emailnotonfile = string.IsNullOrEmpty(PatientModel.Email_Address) ? PatientModel.emailnotonfile : false;
                    objNewPatient.Modified_By = UserId;
                    objNewPatient.Modified_Date = DateTime.Now;
                    objNewPatient.PicturePath = !string.IsNullOrEmpty(PatientModel.PicturePath) ? PatientModel.PicturePath : null;
                    objNewPatient.alternate_id = PatientModel.alternate_id;
                   // objNewPatient.team_id = PatientModel.team_id;
                    if (PatientModel.PatientInsuranceList != null && PatientModel.PatientInsuranceList.Count > 0)
                    {
                        foreach (var item in PatientModel.PatientInsuranceList)
                        {
                            if (item != null)
                            {
                                Patient_Insurance objNewPatientInsurance = new Patient_Insurance();
                                long patInsuranceId = Convert.ToInt64(ctx.SP_TableIdGenerator("Patient_Insurance_Id").FirstOrDefault().ToString());//
                                objNewPatientInsurance.Patient_Insurance_Id = patInsuranceId;
                                objNewPatientInsurance.Insurance_Id = item.Insurance_Id;
                                objNewPatientInsurance.Pri_Sec_Oth_Type = item.Pri_Sec_Oth_Type;
                                objNewPatientInsurance.Policy_Number = item.Policy_Number;
                                objNewPatientInsurance.Subscriber = item.Subscriber;
                                objNewPatientInsurance.Relationship = item.Relationship;
                                objNewPatientInsurance.Co_Payment = item.Co_Payment;
                                objNewPatientInsurance.Group_Number = item.Group_Number;
                                objNewPatientInsurance.Group_Name = item.Group_Name;
                                objNewPatientInsurance.Deductions = item.Deductions;
                                objNewPatientInsurance.Co_Payment_Per = item.Co_Payment_Per;
                                objNewPatientInsurance.CCN = item.CCN;
                                objNewPatientInsurance.Visits_Start_Date = item.Visits_Start_Date;
                                objNewPatientInsurance.Visits_End_Date = item.Visits_End_Date;
                                objNewPatientInsurance.Access_Carolina_Number = item.Access_Carolina_Number;
                                objNewPatientInsurance.Is_Capitated_Patient = item.Is_Capitated_Patient;
                                objNewPatientInsurance.IS_Active = item.IS_Active;
                                ctx.Patient_Insurance.Add(objNewPatientInsurance);
                                ctx.SaveChanges();
                            }
                        }
                    }
                    ctx.SaveChanges();
                    //Check if the patient was synced with InboxHealth
                    SyncedPatient syncedPatient = ctx.SyncedPatients.Where(s => s.Patient_Account == PatientModel.Patient_Account).FirstOrDefault<SyncedPatient>();
                    if (syncedPatient != null)
                    {
                        syncedPatient.UpdatedDate = DateTime.Now;
                        ctx.SaveChanges();
                        //Update the Patient in Inbox Health if the Practice is Synced 
                        UpdateSyncedPatient(PatientModel.Patient_Account, syncedPatient.GeneratedId);
                    }
                    result = PatientModel.Patient_Account;
                }
            }
            if (result > 0)
            {
                objResponse.Status = "Sucess";
                objResponse.Response = result;
            }
            else
            {
                objResponse.Status = "Error";
            }
            return objResponse;
        }

        private void UpdateSyncedPatient(long patient_Account, long generatedId)
        {
            PatientUpdateRequest patientUpdateRequest;
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var patient = ctx.Patients.Where(s => s.Patient_Account == patient_Account).FirstOrDefault();
                    Models.InboxHealth.Patient patients = new Models.InboxHealth.Patient();
                    if (patient.Hold_Pt_Billing == true)
                    {
                        patientUpdateRequest = new PatientUpdateRequest()
                        {
                            id = generatedId,
                            patient = new Models.InboxHealth.Patient()
                            {
                                first_name = patient.First_Name,
                                last_name = patient.Last_Name,
                                date_of_birth = patient.Date_Of_Birth?.ToString("yyyy-MM-dd"),
                                email = patient.Email_Address,
                                primary = true,
                                home_phone = patient.Home_Phone,
                                sex = (int)patient.Gender == 1 ? "Male" : "Female",
                                address_line_1 = patient.Address,
                                city = patient.City,
                                state = patient.State,
                                zip = patient.ZIP,
                                //Set below two values by Hamza Ikhlaq for  inbox statement fixation
                                billing_status = "excluded",
                                precollection = false
                            }

                        };
                    }
                    else
                    {
                              patientUpdateRequest = new PatientUpdateRequest()
                    {
                        id = generatedId,
                        patient = new Models.InboxHealth.Patient()
                        {
                            first_name = patient.First_Name,
                            last_name = patient.Last_Name,
                            date_of_birth = patient.Date_Of_Birth?.ToString("yyyy-MM-dd"),
                            email = patient.Email_Address,
                            primary = true,
                            home_phone = patient.Home_Phone,
                            sex = (int)patient.Gender == 1 ? "Male" : "Female",
                            address_line_1 = patient.Address,
                            city = patient.City,
                            state = patient.State,
                            zip = patient.ZIP,
                            //Set below two values by Hamza Ikhlaq for  inbox statement fixation
                            billing_status = "billable",
                            precollection = false
                        }

                    };
                    }
              
                    var update = _deltaSyncRepository.UpdatePatient(patientUpdateRequest);
                    var practiceSyncId = ctx.PracticeSynchronizations.Where(x => x.PracticeId == patient.Practice_Code).FirstOrDefault().PracticeId;
                    if (update.IsSuccessful)
                    {
                        ctx.PracticeSynchronizationLogs.Add(new PracticeSynchronizationLog()
                        {
                            PracticeSyncId = practiceSyncId,
                            LogMessage = "Patient with Id " + generatedId + " has been successfully updated.",
                            LogTime = DateTime.Now
                        });
                        ctx.SaveChanges();
                    }
                    else
                    {
                        ctx.PracticeSynchronizationLogs.Add(new PracticeSynchronizationLog()
                        {
                            PracticeSyncId = practiceSyncId,
                            LogMessage = "Patient with Id " + generatedId + " has failed to updated.",
                            LogTime = DateTime.Now
                        });
                        ctx.SaveChanges();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public ResponseModel GetAppointments(long PatientAccount)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                List<SP_PATIENTAPPSEARCH_Result> AppointmentsList = null;
                using (var ctx = new NPMDBEntities())
                {
                    AppointmentsList = ctx.SP_PATIENTAPPSEARCH(PatientAccount).ToList();
                }
                objResponse.Status = "Sucess";
                objResponse.Response = AppointmentsList;

            }
            catch (Exception)
            {
                throw;
            }
            return objResponse;
        }

        public ResponseModel GetPatientClaimsSummary(long PatientAccount, bool IncludeDeleted, bool isAmountDueGreaterThanZero = false)
        {
             ResponseModel objResponse = new ResponseModel();
            try
            {
                ClaimSummaryViewModel model = new ClaimSummaryViewModel();
                using (var ctx = new NPMDBEntities())
                {

                    if (IncludeDeleted)
                    {
                        //include deleted
                        if (isAmountDueGreaterThanZero)
                        {
                            //amount due greater than zero
                            model.claimList = ctx.Claims.Where(c => c.Patient_Account == PatientAccount && (c.Deleted ?? false) == true && c.Amt_Due > 0).ToList();
                        }
                        else
                        {
                            //any value of amount due
                            model.claimList = ctx.Claims.Where(c => c.Patient_Account == PatientAccount && (c.Deleted ?? false) == true).ToList();
                        }
                    }
                    else
                    {
                        //exclude deleted
                        if (isAmountDueGreaterThanZero)
                        {
                            //amount due greater than zero
                            model.claimList = ctx.Claims.Where(c => c.Patient_Account == PatientAccount && (c.Deleted ?? false) == false && c.Amt_Due > 0).ToList();
                        }
                        else
                        {
                            //any value of amount due
                            model.claimlistdetails = ctx.Claims
     .Where(c => c.Patient_Account == PatientAccount && (c.Deleted ?? false) == false)
     .GroupJoin(ctx.Claim_Overpayment,
         c => c.Claim_No,
         co => co.Claim_No,
         (c, overpayments) => new
         {
             Claim = c,
             Overpayment = overpayments.FirstOrDefault() // get first matching record or null
        })
     .Select(result => new ClaimsDetail
     {
         Claim_No = result.Claim.Claim_No,
         Patient_Account = result.Claim.Patient_Account,
         Bill_Date = result.Claim.Bill_Date,
         DOS = result.Claim.DOS,
         Location_Code = result.Claim.Location_Code,
         Attending_Physician = result.Claim.Attending_Physician,
         Billing_Physician = result.Claim.Billing_Physician,
         Supervising_Physician = result.Claim.Supervising_Physician,
         Referring_Physician = result.Claim.Referring_Physician,
         PA_Number = result.Claim.PA_Number,
         Referral_Number = result.Claim.Referral_Number,
         ICN_Number = result.Claim.ICN_Number,
         Facility_Code = result.Claim.Facility_Code,
         Hospital_From = result.Claim.Hospital_From,
         Hospital_To = result.Claim.Hospital_To,
         Pri_Status = result.Claim.Pri_Status,
         Sec_Status = result.Claim.Sec_Status,
         Oth_Status = result.Claim.Oth_Status,
         Pat_Status = result.Claim.Pat_Status,
         Attach_Type_Code = result.Claim.Attach_Type_Code,
         Claim_Status = result.Claim.Claim_Status,
         Claim_Status_Date = result.Claim.Claim_Status_Date,
         Current_Visit = result.Claim.Current_Visit,
         Allowed_Visit = result.Claim.Allowed_Visit,
         Accident_Auto = result.Claim.Accident_Auto,
         Accident_Other = result.Claim.Accident_Other,
         Employment = result.Claim.Employment,
         Accident_Emergency = result.Claim.Accident_Emergency,
         Accident_Time = result.Claim.Accident_Time,
         Accident_Date = result.Claim.Accident_Date,
         Accident_State = result.Claim.Accident_State,
         Spinal_Manipulation_Condition_Code = result.Claim.Spinal_Manipulation_Condition_Code,
         Spinal_Manipulation_Description = result.Claim.Spinal_Manipulation_Description,
         Spinal_Manipulation_Xray_Availability = result.Claim.Spinal_Manipulation_Xray_Availability,
         Phy_Exam_Code = result.Claim.Phy_Exam_Code,
         Phy_Exam_Desc = result.Claim.Phy_Exam_Desc,
         Start_Care_Date = result.Claim.Start_Care_Date,
         Last_Seen_Date = result.Claim.Last_Seen_Date,
         Current_Illness_Date = result.Claim.Current_Illness_Date,
         X_Ray_Date = result.Claim.X_Ray_Date,
         Pri_Ins_Payment = result.Claim.Pri_Ins_Payment,
         Sec_Ins_Payment = result.Claim.Sec_Ins_Payment,
         Oth_Ins_Payment = result.Claim.Oth_Ins_Payment,
         Patient_Payment = result.Claim.Patient_Payment,
         Adjustment = result.Claim.Adjustment,
         Amt_Due = result.Claim.Amt_Due,
         Amt_Paid = result.Claim.Amt_Paid,
         Claim_Total = result.Claim.Claim_Total,
         DX_Code1 = result.Claim.DX_Code1,
         DX_Code2 = result.Claim.DX_Code2,
         DX_Code3 = result.Claim.DX_Code3,
         DX_Code4 = result.Claim.DX_Code4,
         DX_Code5 = result.Claim.DX_Code5,
         DX_Code6 = result.Claim.DX_Code6,
         DX_Code7 = result.Claim.DX_Code7,
         DX_Code8 = result.Claim.DX_Code8,
         DX_Code9 = result.Claim.DX_Code9,
         DX_Code10 = result.Claim.DX_Code10,
         DX_Code11 = result.Claim.DX_Code11,
         DX_Code12 = result.Claim.DX_Code12,
         AA = result.Claim.AA,
         BLOCK1213 = result.Claim.BLOCK1213,
         Pos = result.Claim.Pos,
         REBILL_DATE = result.Claim.REBILL_DATE,
         PTL_Status = result.Claim.PTL_Status,
         Delay_Reason_Code = result.Claim.Delay_Reason_Code,
         Ref_Date = result.Claim.Ref_Date,
         Add_CLIA_Number = result.Claim.Add_CLIA_Number,
         Special_Program_Code = result.Claim.Special_Program_Code,
         Print_Center = result.Claim.Print_Center,
         Injury_Date = result.Claim.Injury_Date,
         Injury_Time = result.Claim.Injury_Time,
         Epsdt_Services = result.Claim.Epsdt_Services,
         HCFA_Note = result.Claim.HCFA_Note,
         Patient_Payment_Plan = result.Claim.Patient_Payment_Plan,
         Patient_Statement = result.Claim.Patient_Statement,
         Include_In_Sdf = result.Claim.Include_In_Sdf,
         Is_Self_Pay = result.Claim.Is_Self_Pay,
         Deleted = result.Claim.Deleted,
         Created_By = result.Claim.Created_By,
         Created_Date = result.Claim.Created_Date,
         Modified_By = result.Claim.Modified_By,
         Modified_Date = result.Claim.Modified_Date,
         scan_no = result.Claim.scan_no,
         resolve = result.Claim.resolve,
         Reference_Number = result.Claim.Reference_Number,
         Scan_Date = result.Claim.Scan_Date,
         Ordering_Physician = result.Claim.Ordering_Physician,
         Response_Code = result.Claim.Response_Code,
         Condition_Code = result.Claim.Condition_Code,
         Reference_Claim_No = result.Claim.Reference_Claim_No,
         EDC_date = result.Claim.EDC_date,
         Institution_Condition_Code = result.Claim.Institution_Condition_Code,
         SpecialProgramCode = result.Claim.SpecialProgramCode,
         ServiceAuthExceptionCode = result.Claim.ServiceAuthExceptionCode,
         Map_Claim_History = result.Claim.Map_Claim_History,
         LMP_Date = result.Claim.LMP_Date,
         SCAN_DATE_PTL = result.Claim.SCAN_DATE_PTL,
         PAGE_NO = result.Claim.PAGE_NO,
         Weight = result.Claim.Weight,
         Transport_Distance = result.Claim.Transport_Distance,
         Transportation_Reason_Code = result.Claim.Transportation_Reason_Code,
         Transportation_Condition_Code = result.Claim.Transportation_Condition_Code,
         Transport_Code = result.Claim.Transport_Code,
         Condition_Indicator = result.Claim.Condition_Indicator,
         Archive = result.Claim.Archive,
         Arbitration = result.Claim.Arbitration,
         Last_Seen_Physician = result.Claim.Last_Seen_Physician,
         Is_Draft = result.Claim.Is_Draft,
         Draft_Patient_Account = result.Claim.Draft_Patient_Account,
         Copay_Owed = result.Claim.Copay_Owed,
         Copay_Waived = result.Claim.Copay_Waived,
         Manifestation_Date = result.Claim.Manifestation_Date,
         ASSUMED_CARE_DATE = result.Claim.ASSUMED_CARE_DATE,
         RELINQISHED_CARE_DATE = result.Claim.RELINQISHED_CARE_DATE,
         Claims_Status_Code = result.Claim.Claims_Status_Code,
         Coded_By = result.Claim.Coded_By,
         TCM_Cal_Dos = result.Claim.TCM_Cal_Dos,
         Stop_Submission = result.Claim.Stop_Submission,
         Additional_Estatement = result.Claim.Additional_Estatement,
         Last_statement_Sent_date = result.Claim.Last_statement_Sent_date,
         Last_Work_Date = result.Claim.Last_Work_Date,
         BATCH_NO = result.Claim.BATCH_NO,
         Batch_Date = result.Claim.Batch_Date,
         Plan_Code = result.Claim.Plan_Code,
         Linked_Claims = result.Claim.Linked_Claims,
         Authorization_Req = result.Claim.Authorization_Req,
         PROMISED_AMT_WAIVE = result.Claim.PROMISED_AMT_WAIVE,
         PROMISED_AMT_WAIVE_DATE = result.Claim.PROMISED_AMT_WAIVE_DATE,
         PROMISED_AMT_WAIVE_BY = result.Claim.PROMISED_AMT_WAIVE_BY,
         PROMISED_AMT = result.Claim.PROMISED_AMT,
         PROMISED_AMT_DATE = result.Claim.PROMISED_AMT_DATE,
         PROMISED_AMT_ENTERED_BY = result.Claim.PROMISED_AMT_ENTERED_BY,
         DWC_ID = result.Claim.DWC_ID,
         DWC_DETAIL_ID = result.Claim.DWC_DETAIL_ID,
         Advance_Pat_Payment = result.Claim.Advance_Pat_Payment,
         PA_TRACKING_ID = result.Claim.PA_TRACKING_ID,
         Created_From = result.Claim.Created_From,
         Modified_From = result.Claim.Modified_From,
         practice_code = result.Claim.practice_code,
         Claim_Type=result.Claim.Claim_Type,
         Additional_Claim_Info=result.Claim.Additional_Claim_Info,
         Is_Corrected=result.Claim.Is_Corrected,
         is_resubmitted=result.Claim.is_resubmitted,
         Luo=result.Claim.Luo,
         Resource_Physician=result.Claim.Resource_Physician,
         RSCode=result.Claim.RSCode,
         Insurance_over_paid = result.Overpayment != null ? result.Overpayment.Insurance_over_paid : (decimal?)null,
         Patient_credit_balance = result.Overpayment != null ? result.Overpayment.Patient_credit_balance : (decimal?)null
     }).ToList();



                        }
                    }
                    var objResult = ctx.SP_CLAIMSUMMARYAMOUNTS(PatientAccount);
                   //var objResult = ctx.SP_CLAIMSUMMARYAMOUNTS_Hamza(PatientAccount);
                    if (objResult != null)
                    {
                        var item = objResult.FirstOrDefault();
                        if (item != null)
                        {
                            model.TOTAL_CHARGES = (item.TOTAL_CHARGES.HasValue ? Decimal.Round(item.TOTAL_CHARGES.Value, 2) : 0).ToString();
                            model.TOTAL_PAYMENT = (item.TOTAL_PAYMENT.HasValue ? Decimal.Round(item.TOTAL_PAYMENT.Value, 2) : 0).ToString();
                            model.INSURANCE_DUE = (item.INSURANCE_DUE.HasValue ? Decimal.Round(item.INSURANCE_DUE.Value, 2) : 0).ToString();
                            model.PAT_DUE = (item.PAT_DUE.HasValue ? Decimal.Round(item.PAT_DUE.Value, 2) : 0).ToString();
                            model.COLLECTION_DUE = (item.COLLECTION_DUE.HasValue ? Decimal.Round(item.COLLECTION_DUE.Value, 2) : 0).ToString();
                            model.INS_TOTAL_PAYMENT = (item.INS_TOTAL_PAYMENT.HasValue ? Decimal.Round(item.INS_TOTAL_PAYMENT.Value, 2) : 0).ToString();
                            model.PATIENT_PAYMENTS = (item.PATIENT_PAYMENTS.HasValue ? Decimal.Round(item.PATIENT_PAYMENTS.Value, 2) : 0).ToString();
                            model.Insurance_over_paid = (item.INSURANCE_OVERPAID.HasValue ? Decimal.Round(item.INSURANCE_OVERPAID.Value, 2) : 0);
                            model.Patient_credit_balance = (item.PATIENT_CREDIT_BALANCE.HasValue ? Decimal.Round(item.PATIENT_CREDIT_BALANCE.Value, 2) : 0);

                        }
                    }
                    long inboxPayment=0;
                    var res = ctx.BATCHPAYMENTS.Where(p => p.PatientAccount == PatientAccount)?.ToList();
                    if (res.Count > 0)
                    {
                        foreach (var item in res)
                        {
                            inboxPayment = inboxPayment + (Convert.ToInt64(item.Amount) - Convert.ToInt64(item.PostedAmount));
                        }


                    }


                    model.InboxPaymnet = inboxPayment.ToString();
                }
               

                objResponse.Status = "Sucess";
                objResponse.Response = model;


            }
            catch (Exception)
            {
                objResponse.Status = "Error";
            }
            return objResponse;
        }

        public ResponseModel GetPracticeClaims(ClaimSearchViewModel model)
        {
            ResponseModel objResponse = new ResponseModel();
            //List<SP_GetClaimsListByDOS_Result> listClaims = null;
            //try
            //{
            //    ClaimSearchResponseViewModel claimsList = new ClaimSearchResponseViewModel();
            //    using (var ctx = new NPMDBEntities())
            //    {
            //        ctx.SP_GetClaimsListByDOS(model.DOSFrom, model.DOSTo, model.PracticeCode);
            //    }

            //    objResponse.Status = "Success";
            //    objResponse.Response = model;


            //}
            //catch (Exception)
            //{
            //    objResponse.Status = "Error";
            //}
            return objResponse;
        }

        public ResponseModel GetPatientClaim(long ClaimNo)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                Claim PatientClaim = null;
                using (var ctx = new NPMDBEntities())
                {

                    PatientClaim = ctx.Claims.SingleOrDefault(c => c.Claim_No == ClaimNo && (c.Deleted == null || c.Deleted == false));
                }
                if (PatientClaim != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = PatientClaim;
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

        public List<Practice> GetPatientReferrals(long PatientAccount)
        {
            throw new NotImplementedException();
        }

        public void TestFunc()
        {
            Models.Patient obj = new Models.Patient();

        }

        public ResponseModel GetPatientNotes(long PatientAccount, int? number)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var notesWithUser = from note in ctx.Patient_Notes
                                        join user in ctx.Users on note.Ptn_Created_By equals user.UserId into userGroup
                                        from user in userGroup.DefaultIfEmpty()
                                        where note.Patient_Account == PatientAccount
                                              && (note.Ptn_Deleted == null || note.Ptn_Deleted == false)
                                              && ((number == 0 && note.IsAuto_Note == true)
                                                  || (number != 0 && (note.IsAuto_Note == false || note.IsAuto_Note == null)))
                                        orderby note.Ptn_Created_Date descending
                                        select new
                                        {
                                            note.Patient_Notes_Id,
                                            note.Ptn_Note_Content,
                                            note.Ptn_Created_By,
                                            CreatedByUser = user != null ? user.UserName : null,
                                            note.Ptn_Created_Date,
                                            note.IsAuto_Note,
                                            note.Patient_Account
                                        };

                    var PatientNotesList = notesWithUser.ToList();

                    if (PatientNotesList != null)
                    {
                        objResponse.Status = "Sucess";
                        objResponse.Response = PatientNotesList;
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


        [HttpGet]
        public ResponseModel DeletePatientInsurance(long PatientAccount, long PatientInsuranceId)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                List<PatientInsuranceViewModel> PatientInsuranceList = null;
                using (var ctx = new NPMDBEntities())
                {

                    Patient_Insurance objPatientInsurance = ctx.Patient_Insurance.SingleOrDefault(c => c.Patient_Insurance_Id == PatientInsuranceId && (c.Deleted == null || c.Deleted == false));
                    if (objPatientInsurance != null)
                    {
                        objPatientInsurance.Deleted = true;
                        ctx.SaveChanges();
                    }

                    // PatientInsuranceList = ctx.SP_PATIENTINSSERACH(PatientAccount).ToList();

                    PatientInsuranceList = new List<PatientInsuranceViewModel>(); // 

                    var patInsurances = ctx.SP_PATIENTINSSERACH(PatientAccount).ToList();
                    if (patInsurances != null)
                    {
                        foreach (var item in patInsurances)
                        {
                            PatientInsuranceViewModel objpatinsvmModel = new PatientInsuranceViewModel();
                            objpatinsvmModel.Access_Carolina_Number = item.Access_Carolina_Number;
                            objpatinsvmModel.Allowed_Visits = item.Allowed_Visits;
                            objpatinsvmModel.CCN = item.CCN;
                            objpatinsvmModel.coverage_description = item.Access_Carolina_Number;
                            objpatinsvmModel.Co_Payment = item.Co_Payment;
                            objpatinsvmModel.Co_Payment_Per = item.Co_Payment_Per;
                            objpatinsvmModel.Created_By = item.Created_By;
                            objpatinsvmModel.Created_Date = item.Created_Date;
                            objpatinsvmModel.Created_From = item.Created_From;
                            objpatinsvmModel.Deductions = item.Deductions;
                            objpatinsvmModel.Deleted = item.Deleted;
                            objpatinsvmModel.Effective_Date = item.Effective_Date;
                            objpatinsvmModel.Eligibility_Difference = item.Eligibility_Difference;
                            objpatinsvmModel.Eligibility_Enquiry_Date = item.Eligibility_Enquiry_Date;
                            objpatinsvmModel.Eligibility_Status = item.Eligibility_Status;

                            objpatinsvmModel.Eligibility_S_No = item.Eligibility_S_No;
                            objpatinsvmModel.Filing_Indicator = item.Filing_Indicator;
                            objpatinsvmModel.Filing_Indicator_Code = item.Filing_Indicator_Code;
                            objpatinsvmModel.Group_Name = item.Group_Name;
                            objpatinsvmModel.Group_Number = item.Group_Number;
                            objpatinsvmModel.Insurance_Id = item.Insurance_Id;
                            objpatinsvmModel.Is_Capitated_Patient = item.Is_Capitated_Patient;
                            objpatinsvmModel.MCR_Sec_Payer = item.MCR_Sec_Payer;
                            objpatinsvmModel.MCR_Sec_Payer_Code = item.MCR_Sec_Payer_Code;
                            objpatinsvmModel.Modified_By = item.Modified_By;
                            objpatinsvmModel.Modified_Date = item.Modified_Date;
                            objpatinsvmModel.Patient_Account = item.Patient_Account;
                            objpatinsvmModel.Patient_Insurance_Id = item.Patient_Insurance_Id;
                            objpatinsvmModel.PayerDescription = item.PayerDescription;
                            objpatinsvmModel.Plan_Name = item.Plan_Name;
                            objpatinsvmModel.Plan_Name_Type = item.Plan_Name_Type;

                            objpatinsvmModel.Plan_type = item.Plan_type;
                            objpatinsvmModel.Policy_Number = item.Policy_Number;
                            objpatinsvmModel.Pri_Sec_Oth_Type = item.Pri_Sec_Oth_Type;
                            objpatinsvmModel.Relationship = item.Relationship;
                            objpatinsvmModel.Remaining_Visits = item.Remaining_Visits;
                            objpatinsvmModel.Subscriber = item.Subscriber;
                            objpatinsvmModel.Termination_Date = item.Termination_Date;
                            objpatinsvmModel.Visits_End_Date = item.Visits_End_Date;
                            objpatinsvmModel.Visits_Start_Date = item.Visits_Start_Date;
                            objpatinsvmModel.SubscriberName = item.SubscriberName;
                            objpatinsvmModel.IS_Active = item.IS_Active;

                            PatientInsuranceList.Add(objpatinsvmModel);
                        }

                    }




                    if (PatientInsuranceList != null)
                    {
                        objResponse.Status = "Sucess";
                        objResponse.Response = PatientInsuranceList;
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
        public ResponseModel DeletePatientNote(long PatientAccount, long PatientNotesId)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                List<Patient_Notes> PatientNotesList = null;
                using (var ctx = new NPMDBEntities())
                {

                    Patient_Notes objPatientNote = ctx.Patient_Notes.SingleOrDefault(c => c.Patient_Notes_Id == PatientNotesId && (c.Ptn_Deleted == null || c.Ptn_Deleted == false));
                    if (objPatientNote != null)
                    {
                        objPatientNote.Ptn_Deleted = true;
                        ctx.SaveChanges();
                    }

                    PatientNotesList = ctx.Patient_Notes.Where(c => c.Patient_Account == PatientAccount && (c.Ptn_Deleted == null || c.Ptn_Deleted == false)).ToList();
                    if (PatientNotesList != null)
                    {
                        objResponse.Status = "Sucess";
                        objResponse.Response = PatientNotesList;
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


        public ResponseModel GetPatientNote(long PatientNotesId)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                Patient_Notes PatientNote = null;
                using (var ctx = new NPMDBEntities())
                {
                    PatientNote = ctx.Patient_Notes.SingleOrDefault(c => c.Patient_Notes_Id == PatientNotesId && (c.Ptn_Deleted == null || c.Ptn_Deleted == false));
                }

                if (PatientNote != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = PatientNote;
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

        public ResponseModel SavePatientNotes([FromBody] Patient_Notes PatientNote, long userId)
        {
            ResponseModel objResponse = new ResponseModel();
            Patient_Notes objNewPatientNote = null;

            using (var ctx = new NPMDBEntities())
            {
                if (PatientNote.Patient_Notes_Id != 0)
                {
                    objNewPatientNote = ctx.Patient_Notes.SingleOrDefault(p => p.Patient_Notes_Id == PatientNote.Patient_Notes_Id);
                    if (objNewPatientNote != null)
                    {
                        objNewPatientNote.Ptn_Note_Content = PatientNote.Ptn_Note_Content;
                        objNewPatientNote.Ptn_Modified_By = userId;
                        objNewPatientNote.Ptn_Modified_Date = DateTime.Now;
                        ctx.SaveChanges();
                    }
                }
                else
                {
                    objNewPatientNote = new Patient_Notes();
                    long patNotesid = Convert.ToInt64(ctx.SP_TableIdGenerator("Patient_Notes_Id").FirstOrDefault().ToString());
                    objNewPatientNote = PatientNote;
                    objNewPatientNote.Patient_Notes_Id = patNotesid;
                    objNewPatientNote.Ptn_Created_By = userId;
                    objNewPatientNote.Ptn_Created_Date = DateTime.Now;
                    objNewPatientNote.Ptn_Deleted = false;
                    ctx.Patient_Notes.Add(objNewPatientNote);
                    ctx.SaveChanges();
                }
                if (PatientNote != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = PatientNote.Patient_Notes_Id;
                }
                else
                {
                    objResponse.Status = "Error";
                }
            }

            return objResponse;
        }

        public ResponseModel GetClaimNotes(long ClaimNo, int? number)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var claimNotesQuery = ctx.CLAIM_NOTES
                        .Where(c => c.Claim_No == ClaimNo &&
                                    c.Claim_No != 0 &&
                                    (c.Deleted == null || c.Deleted == false));

                    if (number == 2)
                    {
                        claimNotesQuery = claimNotesQuery
                            .Where(c => c.IsAuto_Note == false  || c.IsAuto_Note == null);
                    }
                    else
                    {
                        claimNotesQuery = claimNotesQuery
                            .Where(c => c.IsAuto_Note == true);
                    }

                    var result = (from note in claimNotesQuery
                                  join user in ctx.Users
                                  on note.Created_By equals user.UserId into userGroup
                                  from user in userGroup.DefaultIfEmpty()
                                  orderby note.Created_Date descending
                                  select new
                                  {
                                      note.Note_Id,
                                      note.Claim_No,
                                      note.Note_Detail,
                                      note.Created_Date,
                                      note.IsAuto_Note,
                                      note.Created_By,
                                      note.Claim_Notes_Id,
                                      UserName = user != null ? user.UserName : null
                                  }).ToList();

                    if (result != null && result.Any())
                    {
                        objResponse.Status = "Success";
                        objResponse.Response = result;
                    }
                    else
                    {
                        objResponse.Status = "Error";
                        objResponse.Response = null;
                    }
                }
            }
            catch (Exception ex)
            {
                objResponse.Status = "Error";
                objResponse.Response = ex.Message;
            }

            return objResponse;
        }



        public ResponseModel GetClaimNote(long ClaimNotesId)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                CLAIM_NOTES ClaimNote = null;
                using (var ctx = new NPMDBEntities())
                {
                    ClaimNote = ctx.CLAIM_NOTES.Where(c => c.Claim_Notes_Id == ClaimNotesId && (c.Deleted == null || c.Deleted == false)).OrderByDescending(c => c.Created_Date).SingleOrDefault();
                }

                if (ClaimNote != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = ClaimNote;
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
        public ResponseModel GetModifiers()
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {

                List<Modifier> modifier = null;

                using (var ctx = new NPMDBEntities())
                {
                    modifier = ctx.Modifiers.Where(c => c.Deleted == null || c.Deleted == false).ToList();
                }

                if (modifier != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = modifier;
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
        public ResponseModel SaveClaimNotes([FromBody] CLAIM_NOTES ClaimNote, long userId)
        {
            ResponseModel objResponse = new ResponseModel();
            CLAIM_NOTES objNewClaimNote = null;

            using (var ctx = new NPMDBEntities())
            {
                if (ClaimNote.Claim_Notes_Id != 0)
                {
                  
                    objNewClaimNote = ctx.CLAIM_NOTES.SingleOrDefault(p => p.Claim_Notes_Id == ClaimNote.Claim_Notes_Id);
                    
                        if (objNewClaimNote != null)
                        {
                            objNewClaimNote.Note_Detail = ClaimNote.Note_Detail;
                            objNewClaimNote.IsAuto_Note = ClaimNote.IsAuto_Note;
                            objNewClaimNote.Modified_Date = DateTime.Now;
                            objNewClaimNote.Created_By = userId;
                            ctx.SaveChanges();
                        }
      
                }
                else
                {
                    objNewClaimNote = new CLAIM_NOTES();
                    long patNotesid = Convert.ToInt64(ctx.SP_TableIdGenerator("Claim_Notes_Id").FirstOrDefault().ToString());//ctx.CLAIM_NOTES.Max(p => p.Claim_Notes_Id);

                    objNewClaimNote = ClaimNote;
                    objNewClaimNote.Claim_Notes_Id = patNotesid;
                    objNewClaimNote.Created_Date = DateTime.Now;
                    objNewClaimNote.Created_By = userId;
                    objNewClaimNote.IsAuto_Note = ClaimNote.IsAuto_Note;
                    objNewClaimNote.Deleted = false;
                    ctx.CLAIM_NOTES.Add(objNewClaimNote);
                    ctx.SaveChanges();
                }
            }
            if (objNewClaimNote != null)
            {
                objResponse.Status = "Sucess";
                objResponse.Response = ClaimNote.Claim_Notes_Id;
            }
            else
            {
                objResponse.Status = "Error";
            }

            return objResponse;
        }
        public ResponseModel GetServiceTypeCodesDescription(long userId)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    responseModel.Response = ctx.Service_Type_Codes_Description.ToList();
                    responseModel.Status = "Success";
                }
            }
            catch (Exception)
            {
                throw;
            }
            return responseModel;
        }

        public ResponseModel DeleteClaimNote(long ClaimNo, long ClaimNotesId)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                List<CLAIM_NOTES> PatientNotesList = null;
                using (var ctx = new NPMDBEntities())
                {

                    CLAIM_NOTES objClaimNote = ctx.CLAIM_NOTES.SingleOrDefault(c => c.Claim_Notes_Id == ClaimNotesId && (c.Deleted == null || c.Deleted == false));
                    if (objClaimNote != null)
                    {
                        objClaimNote.Deleted = true;
                        ctx.SaveChanges();
                    }

                    PatientNotesList = ctx.CLAIM_NOTES.Where(c => c.Claim_No == ClaimNo && (c.Deleted == null || c.Deleted == false)).ToList();
                }

                if (PatientNotesList != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = PatientNotesList;
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


        public string GetInsurancePayerName(long? insuranceId)
        {
            using (var ctx = new NPMDBEntities())
            {
                var res = ctx.Insurances.Where(i => i.Insurance_Id == insuranceId).FirstOrDefault();
                if (res != null)
                {
                    var payerResult = ctx.Insurance_Payers.Where(ip => ip.Inspayer_Id == res.InsPayer_Id).FirstOrDefault();
                    if (payerResult != null)
                        return payerResult.Inspayer_Description;
                }

                return "";
            }
        }

        public ResponseModel GetClaimModel(long PatientAccount, long ClaimNo = 0)
        {
            ResponseModel objResponse = new ResponseModel();
                ClaimsViewModel ClaimViewModel = new ClaimsViewModel();

            GetPatientForClaims data =new GetPatientForClaims();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var myData = ctx.Patients.Where(p => p.Patient_Account == PatientAccount).Select(svm => new GetPatientForClaims { practiceCode = (long)svm.Practice_Code, Address = svm.Address }).FirstOrDefault();
                    if (ClaimNo != 0)
                    {
                        #region Claim Model
                        ClaimViewModel.ClaimModel = ctx.Claims.SingleOrDefault(c => c.Claim_No == ClaimNo && (c.Deleted == null || c.Deleted == false));
                        ClaimViewModel.ClaimModel.Facility_Name = (from c in ctx.Claims
                                                                   join f in ctx.Facilities on c.Facility_Code equals f.Facility_Code
                                                                   where c.Claim_No == ClaimNo
                                                                   select f.Facility_Name).FirstOrDefault();

                        #endregion
                        ClaimViewModel.PatientInsuranceList = new List<PatientInsuranceViewModel>();
                        var patInsurances = ctx.SP_PATIENTINSSERACH(PatientAccount).ToList();
                        if (patInsurances != null)
                        {
                            foreach (var item in patInsurances)
                            {
                                PatientInsuranceViewModel objpatinsvmModel = new PatientInsuranceViewModel();
                                objpatinsvmModel.Access_Carolina_Number = item.Access_Carolina_Number;
                                objpatinsvmModel.Allowed_Visits = item.Allowed_Visits;
                                objpatinsvmModel.CCN = item.CCN;
                                objpatinsvmModel.coverage_description = item.Access_Carolina_Number;
                                objpatinsvmModel.Co_Payment = item.Co_Payment;
                                objpatinsvmModel.Co_Payment_Per = item.Co_Payment_Per;
                                objpatinsvmModel.Created_By = item.Created_By;
                                objpatinsvmModel.Created_Date = item.Created_Date;
                                objpatinsvmModel.Created_From = item.Created_From;
                                objpatinsvmModel.Deductions = item.Deductions;
                                objpatinsvmModel.Deleted = item.Deleted;
                                objpatinsvmModel.Effective_Date = item.Effective_Date;
                                objpatinsvmModel.Eligibility_Difference = item.Eligibility_Difference;
                                objpatinsvmModel.Eligibility_Enquiry_Date = item.Eligibility_Enquiry_Date;
                                objpatinsvmModel.Eligibility_Status = item.Eligibility_Status;
                                objpatinsvmModel.Eligibility_S_No = item.Eligibility_S_No;
                                objpatinsvmModel.Filing_Indicator = item.Filing_Indicator;
                                objpatinsvmModel.Filing_Indicator_Code = item.Filing_Indicator_Code;
                                objpatinsvmModel.Group_Name = item.Group_Name;
                                objpatinsvmModel.Group_Number = item.Group_Number;
                                objpatinsvmModel.Insurance_Id = item.Insurance_Id;
                                objpatinsvmModel.Is_Capitated_Patient = item.Is_Capitated_Patient;
                                objpatinsvmModel.MCR_Sec_Payer = item.MCR_Sec_Payer;
                                objpatinsvmModel.MCR_Sec_Payer_Code = item.MCR_Sec_Payer_Code;
                                objpatinsvmModel.Modified_By = item.Modified_By;
                                objpatinsvmModel.Modified_Date = item.Modified_Date;
                                objpatinsvmModel.Patient_Account = item.Patient_Account;
                                objpatinsvmModel.Patient_Insurance_Id = item.Patient_Insurance_Id;
                                objpatinsvmModel.PayerDescription = item.PayerDescription;
                                objpatinsvmModel.Plan_Name = item.Plan_Name;
                                objpatinsvmModel.Plan_Name_Type = item.Plan_Name_Type;
                                objpatinsvmModel.Plan_type = item.Plan_type;
                                objpatinsvmModel.Policy_Number = item.Policy_Number;
                                objpatinsvmModel.Pri_Sec_Oth_Type = item.Pri_Sec_Oth_Type;
                                objpatinsvmModel.Relationship = item.Relationship;
                                objpatinsvmModel.Remaining_Visits = item.Remaining_Visits;
                                objpatinsvmModel.Subscriber = item.Subscriber;
                                objpatinsvmModel.Termination_Date = item.Termination_Date;
                                objpatinsvmModel.Visits_End_Date = item.Visits_End_Date;
                                objpatinsvmModel.Visits_Start_Date = item.Visits_Start_Date;
                                objpatinsvmModel.SubscriberName = item.SubscriberName;
                                objpatinsvmModel.IS_Active = item.IS_Active;
                                if (objpatinsvmModel.IS_Active == false)
                                {
                                    ClaimViewModel.PatientInsuranceList.Add(objpatinsvmModel);
                                }

                            }
                        }
                        //  ClaimViewModel.Patient_Address =myData.Address;
                        #region Claim Insurance
                        ClaimViewModel.claimInusrance = (from ci in ctx.Claim_Insurance
                                                                      join ins in ctx.Insurances on ci.Insurance_Id equals ins.Insurance_Id
                                                                      join inp in ctx.Insurance_Payers on ins.InsPayer_Id equals inp.Inspayer_Id
                                                                      join g in ctx.Guarantors on ci.Subscriber equals g.Guarantor_Code into uc
                                                                      from c in uc.DefaultIfEmpty()
                                                                      join cb in ctx.CSI_Batch
                                                                          on new { ClaimNo = (long?)ci.Claim_No, InsuranceId = (long?)ci.Insurance_Id }
                                                                          equals new { ClaimNo = cb.Claim_Number, InsuranceId = cb.insurance_id } into batchJoin
                                                                      from cbatch in batchJoin.DefaultIfEmpty()
                                                                      where (ci.Deleted ?? false) == false && ci.Claim_No == ClaimNo
                                                                      select new ClaimInsuranceViewModel
                                                                      {
                                                                          claimInsurance = ctx.Claim_Insurance
                                                                              .FirstOrDefault(ici => ici.Claim_No == ClaimNo && ici.Claim_Insurance_Id == ci.Claim_Insurance_Id),
                                                                          InsurancePayerName = inp.Inspayer_Description,
                                                                          SubscriberName = c.Guarant_Fname + " " + c.Guarant_Lname,
                                                                          Status277 = cbatch.Status_277
                                                                      })
              .OrderBy(i => i.claimInsurance.Pri_Sec_Oth_Type.ToLower() == "o")
              .ThenBy(i => i.claimInsurance.Pri_Sec_Oth_Type.ToLower() == "s")
              .ThenBy(i => i.claimInsurance.Pri_Sec_Oth_Type.ToLower() == "p")
              .ToList();





                        //(from ci in ctx.Claim_Insurance
                        //                             join ins in ctx.Insurances on ci.Insurance_Id equals ins.Insurance_Id
                        //                             join inp in ctx.Insurance_Payers on ins.InsPayer_Id equals inp.Inspayer_Id
                        //                             join g in ctx.Guarantors on ci.Subscriber equals g.Guarantor_Code into uc
                        //                             from c in uc.DefaultIfEmpty()
                        //                             where (ci.Deleted ?? false) == false && (ci.Claim_No == ClaimNo)
                        //                             select new ClaimInsuranceViewModel()
                        //                             {
                        //                                 claimInsurance = ctx.Claim_Insurance.Where(ici => ici.Claim_No == ClaimNo && ici.Claim_Insurance_Id == ci.Claim_Insurance_Id).FirstOrDefault(),
                        //                                 InsurancePayerName = inp.Inspayer_Description,
                        //                                 SubscriberName = c.Guarant_Fname + " " + c.Guarant_Lname
                        //                             }).OrderBy(i => i.claimInsurance.Pri_Sec_Oth_Type.ToLower() == "o").ThenBy(i => i.claimInsurance.Pri_Sec_Oth_Type.ToLower() == "s").ThenBy(i => i.claimInsurance.Pri_Sec_Oth_Type.ToLower() == "p").ToList();

                        #endregion

                        #region Claim Charges
                        //  ClaimViewModel.claimCharges = ctx.Claim_Charges.Where(ci => ci.Claim_No == ClaimNo && (ci.Deleted == null || ci.Deleted == false)).Select(svm => new ClaimChargesViewModel { amt = "", Description = ctx.Procedures.FirstOrDefault(p => (p.ProcedureCode == svm.Procedure_Code && p.Alternate_Code==svm.Alternate_Code) || (p.ProcedureCode == svm.Procedure_Code && (p.Alternate_Code == null))).ProcedureDescription, claimCharges = ctx.Claim_Charges.Where(ci => ci.Claim_No == ClaimNo && ci.claim_charges_id == svm.claim_charges_id).FirstOrDefault() }).OrderBy(c => c.claimCharges.Sequence_No).ToList();                                            
                        var claimChargesList = new List<ClaimChargesViewModel>();

                        var claimCharges = ctx.Claim_Charges
                            .Where(ci => ci.Claim_No == ClaimNo && (ci.Deleted == null || ci.Deleted == false))
                            .ToList(); // Execute the query and bring results into memory

                        foreach (var svm in claimCharges)
                        {
                            // Get the ProcedureDescription
                            // 1. Check for an exact match of ProcedureCode and Alternate_Code
                           var procedureDescription = ctx.Procedures
                                .Where(p => p.ProcedureCode == svm.Procedure_Code &&
                                            p.Alternate_Code == svm.Alternate_Code)
                                .Select(p => p.ProcedureDescription)
                                .FirstOrDefault();

                            // If the first query returns a description, return it
                            if (procedureDescription == null)
                            {
                                // 2. Check for a match where ProcedureCode is matched and Alternate_Code is null or empty
                                procedureDescription = ctx.Procedures
                                    .Where(p => p.ProcedureCode == svm.Procedure_Code &&
                                                string.IsNullOrEmpty(p.Alternate_Code))
                                    .Select(p => p.ProcedureDescription)
                                    .FirstOrDefault();
                            }

                            // If the second query returns a description, return it
                            if (procedureDescription == null)
                            {
                                // 3. Fallback: Return the first matched ProcedureDescription based on ProcedureCode
                                procedureDescription = ctx.Procedures
                                    .Where(p => p.ProcedureCode == svm.Procedure_Code)
                                    .Select(p => p.ProcedureDescription)
                                    .FirstOrDefault();
                            }
                            // Add to the claim charges list
                            claimChargesList.Add(new ClaimChargesViewModel
                            {
                                amt = "",
                                Description = procedureDescription,
                                claimCharges = ctx.Claim_Charges
                                    .FirstOrDefault(ci => ci.Claim_No == ClaimNo && ci.claim_charges_id == svm.claim_charges_id)
                            });
                        }
                        // Now order the list and assign it to the view model
                        ClaimViewModel.claimCharges = claimChargesList
                            .OrderBy(c => c.claimCharges.Sequence_No)
                            .ToList();


                        for (int i = 0; i < ClaimViewModel.claimCharges.Count(); i++)
                        {
                            //..Below line added by Tamour Ali 16/08/2023 for checking if cpt is anesthesia or not
                            ClaimViewModel.claimCharges[i].IsAnesthesiaCpt = CheckIfAnesthesiaCpt(ClaimViewModel.claimCharges[i].claimCharges.Procedure_Code);
                            var dCode1 = ClaimViewModel.claimCharges[i].claimCharges.Drug_Code;
                            try
                            {
                                if (ClaimViewModel.claimCharges[i].claimCharges.Drug_Code != null && ClaimViewModel.claimCharges[i].claimCharges.Drug_Code.Length > 5)
                                {
                                    var dCode = (ClaimViewModel.claimCharges[i].claimCharges.Drug_Code)?.Insert(5, "-");
                                    if (dCode.Length >= 10)
                                        dCode1 = dCode?.Insert(10, "-");
                                }
                            }
                            catch (Exception ex)
                            {
                                NPMLogger.GetInstance().Error(ex.ToString());
                            }
                            ClaimViewModel.claimCharges[i].Drug_Code = dCode1;
                            ClaimViewModel.claimCharges[i].claimCharges.Drug_Code = ClaimViewModel.claimCharges[i].claimCharges.Drug_Code;
                        }

                        if (ClaimViewModel != null && ClaimViewModel.claimCharges != null)
                        {
                            foreach (var item in ClaimViewModel.claimCharges)
                            {
                                var maxDate = Convert.ToDateTime("10/01/2015");
                                var ndcModel = ctx.NDC_CrossWalk.Where(scf => scf.HCPCS_Code == item.claimCharges.Procedure_Code && scf.Effective_Date_To >= maxDate).ToList();
                                item.claimCharges.NDCCodeList = new List<SelectListViewModel>();
                                foreach (var nItem in ndcModel)
                                {
                                    SelectListViewModel objmodel = new SelectListViewModel();
                                    objmodel.Id = nItem.NDC_ID;
                                    objmodel.Name = nItem.NDC2;
                                    objmodel.Meta = new ExpandoObject();
                                    objmodel.Meta.Qualifier = nItem.Qualifier;
                                    item.claimCharges.NDCCodeList.Add(objmodel);
                                }
                            }
                        }
                        #endregion

                        #region Claim Payments
                        ClaimViewModel.claimPayments = (from ci in ctx.Claim_Payments
                                                        join ins in ctx.Insurances on ci.Insurance_Id equals ins.Insurance_Id into ps
                                                        from pp in ps.DefaultIfEmpty()
                                                        join inp in ctx.Insurance_Payers on pp.InsPayer_Id equals inp.Inspayer_Id into uc
                                                        from c in uc.DefaultIfEmpty()
                                                        where (ci.Deleted ?? false) == false && (ci.Claim_No == ClaimNo)
                                                        select new ClaimPaymentViewModel()
                                                        {
                                                            claimPayments = ctx.Claim_Payments.Where(ici => ici.Claim_No == ClaimNo && ici.claim_payments_id == ci.claim_payments_id).FirstOrDefault(),
                                                            InsurancePayerName = c.Inspayer_Description
                                                        }).OrderBy(p => p.claimPayments.Date_Entry).ToList();

                        if (ClaimViewModel.ClaimModel != null && !string.IsNullOrEmpty(ClaimViewModel.ClaimModel.DX_Code1))
                            ClaimViewModel.DX1Description = ctx.Diagnosis.FirstOrDefault(d => d.Diag_Code == ClaimViewModel.ClaimModel.DX_Code1)?.Diag_Description;
                        ClaimViewModel.DX1EffectiveDate = ctx.Diagnosis.FirstOrDefault(d => d.Diag_Code == ClaimViewModel.ClaimModel.DX_Code1)?.Diag_Effective_Date;
                        ClaimViewModel.DX1ExpiryDate = ctx.Diagnosis.FirstOrDefault(d => d.Diag_Code == ClaimViewModel.ClaimModel.DX_Code1)?.Diag_Expiry_Date;

                        if (ClaimViewModel.ClaimModel != null && !string.IsNullOrEmpty(ClaimViewModel.ClaimModel.DX_Code2))
                            ClaimViewModel.DX2Description = ctx.Diagnosis.FirstOrDefault(d => d.Diag_Code == ClaimViewModel.ClaimModel.DX_Code2)?.Diag_Description;
                        ClaimViewModel.DX2EffectiveDate = ctx.Diagnosis.FirstOrDefault(d => d.Diag_Code == ClaimViewModel.ClaimModel.DX_Code2)?.Diag_Effective_Date;
                        ClaimViewModel.DX2ExpiryDate = ctx.Diagnosis.FirstOrDefault(d => d.Diag_Code == ClaimViewModel.ClaimModel.DX_Code2)?.Diag_Expiry_Date;

                        if (ClaimViewModel.ClaimModel != null && !string.IsNullOrEmpty(ClaimViewModel.ClaimModel.DX_Code3))
                            ClaimViewModel.DX3Description = ctx.Diagnosis.FirstOrDefault(d => d.Diag_Code == ClaimViewModel.ClaimModel.DX_Code3)?.Diag_Description;
                        ClaimViewModel.DX3EffectiveDate = ctx.Diagnosis.FirstOrDefault(d => d.Diag_Code == ClaimViewModel.ClaimModel.DX_Code3)?.Diag_Effective_Date;
                        ClaimViewModel.DX3ExpiryDate = ctx.Diagnosis.FirstOrDefault(d => d.Diag_Code == ClaimViewModel.ClaimModel.DX_Code3)?.Diag_Expiry_Date;

                        if (ClaimViewModel.ClaimModel != null && !string.IsNullOrEmpty(ClaimViewModel.ClaimModel.DX_Code4))
                            ClaimViewModel.DX4Description = ctx.Diagnosis.FirstOrDefault(d => d.Diag_Code == ClaimViewModel.ClaimModel.DX_Code4)?.Diag_Description;
                        ClaimViewModel.DX4EffectiveDate = ctx.Diagnosis.FirstOrDefault(d => d.Diag_Code == ClaimViewModel.ClaimModel.DX_Code4)?.Diag_Effective_Date;
                        ClaimViewModel.DX4ExpiryDate = ctx.Diagnosis.FirstOrDefault(d => d.Diag_Code == ClaimViewModel.ClaimModel.DX_Code4)?.Diag_Expiry_Date;

                        if (ClaimViewModel.ClaimModel != null && !string.IsNullOrEmpty(ClaimViewModel.ClaimModel.DX_Code5))
                            ClaimViewModel.DX5Description = ctx.Diagnosis.FirstOrDefault(d => d.Diag_Code == ClaimViewModel.ClaimModel.DX_Code5)?.Diag_Description;
                        ClaimViewModel.DX5EffectiveDate = ctx.Diagnosis.FirstOrDefault(d => d.Diag_Code == ClaimViewModel.ClaimModel.DX_Code5)?.Diag_Effective_Date;
                        ClaimViewModel.DX5ExpiryDate = ctx.Diagnosis.FirstOrDefault(d => d.Diag_Code == ClaimViewModel.ClaimModel.DX_Code5)?.Diag_Expiry_Date;


                        if (ClaimViewModel.ClaimModel != null && !string.IsNullOrEmpty(ClaimViewModel.ClaimModel.DX_Code6))
                            ClaimViewModel.DX6Description = ctx.Diagnosis.FirstOrDefault(d => d.Diag_Code == ClaimViewModel.ClaimModel.DX_Code6)?.Diag_Description;
                        ClaimViewModel.DX6EffectiveDate = ctx.Diagnosis.FirstOrDefault(d => d.Diag_Code == ClaimViewModel.ClaimModel.DX_Code6)?.Diag_Effective_Date;
                        ClaimViewModel.DX6ExpiryDate = ctx.Diagnosis.FirstOrDefault(d => d.Diag_Code == ClaimViewModel.ClaimModel.DX_Code6)?.Diag_Expiry_Date;

                        if (ClaimViewModel.ClaimModel != null && !string.IsNullOrEmpty(ClaimViewModel.ClaimModel.DX_Code7))
                            ClaimViewModel.DX7Description = ctx.Diagnosis.FirstOrDefault(d => d.Diag_Code == ClaimViewModel.ClaimModel.DX_Code7)?.Diag_Description;
                        ClaimViewModel.DX7EffectiveDate = ctx.Diagnosis.FirstOrDefault(d => d.Diag_Code == ClaimViewModel.ClaimModel.DX_Code7)?.Diag_Effective_Date;
                        ClaimViewModel.DX7ExpiryDate = ctx.Diagnosis.FirstOrDefault(d => d.Diag_Code == ClaimViewModel.ClaimModel.DX_Code7)?.Diag_Expiry_Date;


                        if (ClaimViewModel.ClaimModel != null && !string.IsNullOrEmpty(ClaimViewModel.ClaimModel.DX_Code8))
                            ClaimViewModel.DX8Description = ctx.Diagnosis.FirstOrDefault(d => d.Diag_Code == ClaimViewModel.ClaimModel.DX_Code8)?.Diag_Description;
                        ClaimViewModel.DX8EffectiveDate = ctx.Diagnosis.FirstOrDefault(d => d.Diag_Code == ClaimViewModel.ClaimModel.DX_Code8)?.Diag_Effective_Date;
                        ClaimViewModel.DX8ExpiryDate = ctx.Diagnosis.FirstOrDefault(d => d.Diag_Code == ClaimViewModel.ClaimModel.DX_Code8)?.Diag_Expiry_Date;

                        if (ClaimViewModel.ClaimModel != null && !string.IsNullOrEmpty(ClaimViewModel.ClaimModel.DX_Code9))
                            ClaimViewModel.DX9Description = ctx.Diagnosis.FirstOrDefault(d => d.Diag_Code == ClaimViewModel.ClaimModel.DX_Code9)?.Diag_Description;
                        ClaimViewModel.DX9EffectiveDate = ctx.Diagnosis.FirstOrDefault(d => d.Diag_Code == ClaimViewModel.ClaimModel.DX_Code9)?.Diag_Effective_Date;
                        ClaimViewModel.DX9ExpiryDate = ctx.Diagnosis.FirstOrDefault(d => d.Diag_Code == ClaimViewModel.ClaimModel.DX_Code9)?.Diag_Expiry_Date;


                        if (ClaimViewModel.ClaimModel != null && !string.IsNullOrEmpty(ClaimViewModel.ClaimModel.DX_Code10))
                            ClaimViewModel.DX10Description = ctx.Diagnosis.FirstOrDefault(d => d.Diag_Code == ClaimViewModel.ClaimModel.DX_Code10)?.Diag_Description;
                        ClaimViewModel.DX10EffectiveDate = ctx.Diagnosis.FirstOrDefault(d => d.Diag_Code == ClaimViewModel.ClaimModel.DX_Code10)?.Diag_Effective_Date;
                        ClaimViewModel.DX10ExpiryDate = ctx.Diagnosis.FirstOrDefault(d => d.Diag_Code == ClaimViewModel.ClaimModel.DX_Code10)?.Diag_Expiry_Date;


                        if (ClaimViewModel.ClaimModel != null && !string.IsNullOrEmpty(ClaimViewModel.ClaimModel.DX_Code11))
                            ClaimViewModel.DX11Description = ctx.Diagnosis.FirstOrDefault(d => d.Diag_Code == ClaimViewModel.ClaimModel.DX_Code11)?.Diag_Description;
                        ClaimViewModel.DX11EffectiveDate = ctx.Diagnosis.FirstOrDefault(d => d.Diag_Code == ClaimViewModel.ClaimModel.DX_Code11)?.Diag_Effective_Date;
                        ClaimViewModel.DX11ExpiryDate = ctx.Diagnosis.FirstOrDefault(d => d.Diag_Code == ClaimViewModel.ClaimModel.DX_Code11)?.Diag_Expiry_Date;

                        if (ClaimViewModel.ClaimModel != null && !string.IsNullOrEmpty(ClaimViewModel.ClaimModel.DX_Code12))
                            ClaimViewModel.DX12Description = ctx.Diagnosis.FirstOrDefault(d => d.Diag_Code == ClaimViewModel.ClaimModel.DX_Code12)?.Diag_Description;
                        ClaimViewModel.DX12EffectiveDate = ctx.Diagnosis.FirstOrDefault(d => d.Diag_Code == ClaimViewModel.ClaimModel.DX_Code12)?.Diag_Effective_Date;
                        ClaimViewModel.DX12ExpiryDate = ctx.Diagnosis.FirstOrDefault(d => d.Diag_Code == ClaimViewModel.ClaimModel.DX_Code12)?.Diag_Expiry_Date;

                        ClaimViewModel.ClaimDate = DateTime.Now;

                        #endregion


                        #region ub claim fields




                        //                    List<Claims_Occurrence_Code> list = ctx.Claims_Occurrence_Code
                        //.Where(c => c.Claim_no == ClaimNo)
                        //.ToList();




                        //                    foreach (var cc in list)
                        //                    {
                        //                        ClaimViewModel.UbClaimDropdown.OccCode.Add(new Claims_Occurrence_Code
                        //                        {
                        //                            Claims_Occurrence_Code_Id = cc.Claims_Occurrence_Code_Id,
                        //                            Practice_Code = cc.Practice_Code,
                        //                            Claim_no = cc.Claim_no,
                        //                            OccCode = cc.OccCode,
                        //                            Descriptions = cc.Descriptions,
                        //                            Date = cc.Date,
                        //                            Isdeleted = cc.Isdeleted,
                        //                            Modified_By = cc.Modified_By,
                        //                            Modified_Date = cc.Modified_Date
                        //                        });
                        //                    }


                        //            List<Claims_Occurrence_Code> list = new List<Claims_Occurrence_Code>();

                        //            list = ctx.Claims_Occurrence_Code.Where
                        //(c => c.Claim_no == ClaimNo).ToList();


                       

                        ClaimViewModel.UbClaimDropdown.CcOde = ctx.Claims_Condition_Code.Where
                            (c => c.Claim_No == ClaimNo && c.Isdeleted==false).Select(cc => new ConditionCodeModel
                            {
                                CCID = cc.Claims_Condition_Code_Id,
                                Practice_Code = cc.Practice_Code,
                                Claim_No = cc.Claim_No,
                                ConditionCode = cc.ConditionCode,
                                Descriptions = cc.Descriptions,
                                Date = cc.Date,
                            }).ToList();



                        ClaimViewModel.UbClaimDropdown.OccCode = ctx.Claims_Occurrence_Code.Where
           (c => c.Claim_no == ClaimNo && c.Isdeleted==false).ToList()
           .Select(cc => new OccurrenceCodeModel
           {

               OCID = cc.Claims_Occurrence_Code_Id,
               Practice_Code = cc.Practice_Code,
               Claim_no = cc.Claim_no,
               OccCode = cc.OccCode,
               Descriptions = cc.Descriptions,
               Date2 = cc.Date.ToString(),
               Isdeleted = cc.Isdeleted,
           }).ToList();

                        ClaimViewModel.UbClaimDropdown.OccSpanCode = ctx.Claims_Occurence_Span_Code
      .Where(c => c.ClaimNo == ClaimNo && c.Isdeleted==false).ToList()
      .Select(cc => new OccurenceSpanModel
      {
          OSCID = cc.Claims_Occurence_Span_Code_Id,
          Practice_Code = cc.Practice_Code,
          ClaimNo = cc.ClaimNo,
          OccSpanCode = cc.OccSpanCode,
          DateFrom = cc.DateFrom.ToString(),
          DateThrough =   cc.DateThrough.ToString(),
          Descriptions = cc.Descriptions,
          Isdeleted = cc.Isdeleted
      }).ToList();

                        ClaimViewModel.UbClaimDropdown.ValueCode = ctx.Claims_Value_Code
                       .Where(c => c.Claim_No == ClaimNo && c.Isdeleted==false)
                       .Select(cc => new ValueeCode
                       {
                           VCID = cc.Claims_Value_Code_Id,
                           Practice_Code = cc.Practice_Code,
                           Claim_No = cc.Claim_No,
                           Amount = cc.Amount,
                           Value_Codes_Id = cc.Value_Codes_Id,
                           Isdeleted = cc.Isdeleted
                       }).ToList();




                        ClaimViewModel.Admission_Details = ctx.Admission_Details.Where(c => c.Claim_No == ClaimNo).Select(cc => new Admission_Detail
                        {
                            Admhour = cc.Admhour,
                            AdmSource = cc.AdmSource,
                            Dischargehour = cc.Dischargehour,
                            Discharge_status_Id = cc.Discharge_status_Id,
                            Type_of_Bill = cc.Type_of_Bill,
                            Type_Of_Admission_Id = cc.Type_Of_Admission_Id,
                            Id = cc.Admission_Details_Id


                        }).FirstOrDefault();
                        #endregion
                    }
                    else
                    {
                     //   ClaimViewModel.Patient_Address = myData.Address;
                        ClaimViewModel.PatientInsuranceList = new List<PatientInsuranceViewModel>();
                        var patInsurances = ctx.SP_PATIENTINSSERACH(PatientAccount).ToList();
                        if (patInsurances != null)
                        {
                            foreach (var item in patInsurances)
                            {
                                PatientInsuranceViewModel objpatinsvmModel = new PatientInsuranceViewModel();
                                objpatinsvmModel.Access_Carolina_Number = item.Access_Carolina_Number;
                                objpatinsvmModel.Allowed_Visits = item.Allowed_Visits;
                                objpatinsvmModel.CCN = item.CCN;
                                objpatinsvmModel.coverage_description = item.Access_Carolina_Number;
                                objpatinsvmModel.Co_Payment = item.Co_Payment;
                                objpatinsvmModel.Co_Payment_Per = item.Co_Payment_Per;
                                objpatinsvmModel.Created_By = item.Created_By;
                                objpatinsvmModel.Created_Date = item.Created_Date;
                                objpatinsvmModel.Created_From = item.Created_From;
                                objpatinsvmModel.Deductions = item.Deductions;
                                objpatinsvmModel.Deleted = item.Deleted;
                                objpatinsvmModel.Effective_Date = item.Effective_Date;
                                objpatinsvmModel.Eligibility_Difference = item.Eligibility_Difference;
                                objpatinsvmModel.Eligibility_Enquiry_Date = item.Eligibility_Enquiry_Date;
                                objpatinsvmModel.Eligibility_Status = item.Eligibility_Status;
                                objpatinsvmModel.Eligibility_S_No = item.Eligibility_S_No;
                                objpatinsvmModel.Filing_Indicator = item.Filing_Indicator;
                                objpatinsvmModel.Filing_Indicator_Code = item.Filing_Indicator_Code;
                                objpatinsvmModel.Group_Name = item.Group_Name;
                                objpatinsvmModel.Group_Number = item.Group_Number;
                                objpatinsvmModel.Insurance_Id = item.Insurance_Id;
                                objpatinsvmModel.Is_Capitated_Patient = item.Is_Capitated_Patient;
                                objpatinsvmModel.MCR_Sec_Payer = item.MCR_Sec_Payer;
                                objpatinsvmModel.MCR_Sec_Payer_Code = item.MCR_Sec_Payer_Code;
                                objpatinsvmModel.Modified_By = item.Modified_By;
                                objpatinsvmModel.Modified_Date = item.Modified_Date;
                                objpatinsvmModel.Patient_Account = item.Patient_Account;
                                objpatinsvmModel.Patient_Insurance_Id = item.Patient_Insurance_Id;
                                objpatinsvmModel.PayerDescription = item.PayerDescription;
                                objpatinsvmModel.Plan_Name = item.Plan_Name;
                                objpatinsvmModel.Plan_Name_Type = item.Plan_Name_Type;
                                objpatinsvmModel.Plan_type = item.Plan_type;
                                objpatinsvmModel.Policy_Number = item.Policy_Number;
                                objpatinsvmModel.Pri_Sec_Oth_Type = item.Pri_Sec_Oth_Type;
                                objpatinsvmModel.Relationship = item.Relationship;
                                objpatinsvmModel.Remaining_Visits = item.Remaining_Visits;
                                objpatinsvmModel.Subscriber = item.Subscriber;
                                objpatinsvmModel.Termination_Date = item.Termination_Date;
                                objpatinsvmModel.Visits_End_Date = item.Visits_End_Date;
                                objpatinsvmModel.Visits_Start_Date = item.Visits_Start_Date;
                                objpatinsvmModel.SubscriberName = item.SubscriberName;
                                objpatinsvmModel.IS_Active = item.IS_Active;
                                if (objpatinsvmModel.IS_Active==false)
                                {
                                    ClaimViewModel.PatientInsuranceList.Add(objpatinsvmModel);
                                }
                                
                            }
                        }
                        ClaimViewModel.ClaimModel = new Claim();
                    }
                    ClaimViewModel.PatientAccount = PatientAccount;
                    ClaimViewModel.BillingPhysiciansList = ctx.Providers.Where(r => (r.Deleted ?? false) == false && r.Practice_Code == myData.practiceCode ).Select(svm => new SelectListViewModelForProvider { Id = svm.Provider_Code, Name = svm.Provid_FName + " " + svm.Provid_LName , provider_State =svm.STATE , SPECIALIZATION_CODE= svm.SPECIALIZATION_CODE , is_active=svm.Is_Active  }).ToList();
                    ClaimViewModel.AttendingPhysiciansList = ctx.Providers.Where(r => (r.Deleted ?? false) == false && r.Practice_Code == myData.practiceCode ).Select(svm => new SelectListViewModelForProvider { Id = svm.Provider_Code, Name = svm.Provid_FName + " " + svm.Provid_LName , is_active = svm.Is_Active }).ToList();
                    ClaimViewModel.PracticeLocationsList = ctx.Practice_Locations.Where(r => (r.Deleted ?? false) == false && r.Practice_Code == myData.practiceCode ).Select(svm => new SelectListViewModel { Id = svm.Location_Code, Name = svm.Location_Name }).ToList();
                    ClaimViewModel.ReferralPhysiciansList = ctx.Referral_Physicians.Where(r => !(r.Deleted ?? false)).Select(svm => new SelectListViewModel { Id = svm.Referral_Code, Name = svm.Referral_Fname + " " + svm.Referral_Lname, In_Active = svm.In_Active.HasValue ? svm.In_Active : null }).ToList();
                    //     ClaimViewModel.PTLReasons = ctx.DelayReasonsNews1.Where(r => !(r.Deleted ?? false)).Select(svm => new SelectListViewModel { Id = svm.PLTID, IdStr = svm.PTLReasonId, Name = svm.Description }).ToList();
                    ClaimViewModel.AdjustCodeList = ctx.EOB_Adjustment_Codes.Where(r => r.Inactive == null || r.Inactive == false).ToList();
                    ClaimViewModel.POSList = ctx.Place_Of_Services.Where(r => !(r.Deleted ?? false)).ToList();
                    // Resubmission Codes
                    ClaimViewModel.ResubmissionCodes = ctx.ResubmissionCodes.Select(c => new SelectListViewModel()
                    {
                        Id = c.RSCode,
                        Name = c.Description,
                        IdStr = c.RSCode.ToString()
                    }).ToList();
                }
                if (ClaimViewModel != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = ClaimViewModel;
                }
                else
                {
                    objResponse.Status = "Error";
                }
            }
            catch (Exception ex)
            {
                objResponse.Status = "Error";
                objResponse.Response = ex;
            }
            return objResponse;
        }

        public ResponseModel SaveClaimOld([FromBody] ClaimsViewModel ClaimModel)
        {
            ResponseModel objResponse = new ResponseModel();
            Claim objClaim = null;
            using (var ctx = new NPMDBEntities())
            {
                objClaim = ctx.Claims.SingleOrDefault(p => p.Claim_No == ClaimModel.ClaimModel.Claim_No);
                if (objClaim == null)
                {
                    ctx.Claims.Add(ClaimModel.ClaimModel);
                    ctx.SaveChanges();
                }
                else
                {
                    objClaim.DOS = ClaimModel.ClaimModel.DOS;
                    objClaim.Attending_Physician = ClaimModel.ClaimModel.Attending_Physician;
                    objClaim.Supervising_Physician = ClaimModel.ClaimModel.Supervising_Physician;
                    objClaim.Billing_Physician = ClaimModel.ClaimModel.Billing_Physician;
                    objClaim.Location_Code = ClaimModel.ClaimModel.Location_Code;
                    objClaim.Referring_Physician = ClaimModel.ClaimModel.Referring_Physician;
                    objClaim.Referral_Number = ClaimModel.ClaimModel.Referral_Number;
                    objClaim.PA_Number = ClaimModel.ClaimModel.PA_Number;
                    objClaim.Is_Self_Pay = ClaimModel.ClaimModel.Is_Self_Pay;
                    //  objClaim.Pri_Ins_Payment = ClaimModel.ClaimModel.Pri_Ins_Payment;
                    objClaim.Pri_Status = ClaimModel.ClaimModel.Pri_Status;
                    //  objClaim.Sec_Ins_Payment = ClaimModel.ClaimModel.Sec_Ins_Payment;
                    objClaim.Sec_Status = ClaimModel.ClaimModel.Sec_Status;
                    //    objClaim.Oth_Ins_Payment = ClaimModel.ClaimModel.Oth_Ins_Payment; Same missing for Patient Payment
                    objClaim.Oth_Status = ClaimModel.ClaimModel.Oth_Status;
                    objClaim.Pat_Status = ClaimModel.ClaimModel.Pat_Status;
                    objClaim.Facility_Code = ClaimModel.ClaimModel.Facility_Code;
                    objClaim.Hospital_From = ClaimModel.ClaimModel.Hospital_From;
                    objClaim.Hospital_To = ClaimModel.ClaimModel.Hospital_To;
                    objClaim.Hospital_From = ClaimModel.ClaimModel.Hospital_From;  // No Room Number column in table
                    objClaim.PTL_Status = ClaimModel.ClaimModel.PTL_Status;

                    if ((bool)ClaimModel.ClaimModel.PTL_Status)
                    {
                        objClaim.Delay_Reason_Code = ClaimModel.ClaimModel.Delay_Reason_Code;

                        // Claim Feedback
                        Claims_Ptl_Feedback objClaimFeedback = new Claims_Ptl_Feedback();
                        objClaimFeedback.Claim_No = ClaimModel.ClaimModel.Claim_No;
                        objClaimFeedback.User_Notes = ClaimModel.PTLReasonDetail;
                        if (string.IsNullOrEmpty(ClaimModel.PTLReasonDoctorFeedback))
                        {
                            objClaimFeedback.FeedBack = ClaimModel.PTLReasonDoctorFeedback;
                            objClaimFeedback.FeedBack_Date = System.DateTime.Now;
                        }
                        objClaimFeedback.Reasons = ClaimModel.ClaimModel.Delay_Reason_Code;

                    }

                    //
                    //objClaim.DOS = ClaimModel.ClaimModel.DOS;
                    //objClaim.DOS = ClaimModel.ClaimModel.DOS;
                    //objClaim.DOS = ClaimModel.ClaimModel.DOS;
                    //objClaim.DOS = ClaimModel.ClaimModel.DOS;
                    //objClaim.DOS = ClaimModel.ClaimModel.DOS;
                    //objClaim.DOS = ClaimModel.ClaimModel.DOS;
                    //objClaim.DOS = ClaimModel.ClaimModel.DOS;
                    //objClaim.DOS = ClaimModel.ClaimModel.DOS;
                    //objClaim.DOS = ClaimModel.ClaimModel.DOS;
                    //objClaim.DOS = ClaimModel.ClaimModel.DOS;

                    ctx.SaveChanges();
                }
            }

            if (objClaim != null)
            {
                objResponse.Status = "Sucess";
                objResponse.Response = objClaim.Claim_No;
            }
            else
            {
                objResponse.Status = "Error";
            }
            return objResponse;
        }
        //Added by HAMZA ZULFIQAR as per USER STORY 119: Reporting Dashboard Implementation For All Practices
        public ResponseModel GetFacilitySelectList(long practice_code)
        {
            ResponseModel responseModel = new ResponseModel();
            List<SelectListViewModel> list = new List<SelectListViewModel>();
            try
            {
                using (var db = new NPMDBEntities())
                {
                    var facilityList = db.Database.SqlQuery<FacilityResult>("exec UPS_GET_FACILITIES @PRACTICE_CODE", new SqlParameter("@PRACTICE_CODE", practice_code)).ToList();
                    list = facilityList.Select(f => new SelectListViewModel()
                    {
                        Id = f.Facility_Code,
                        Name = f.Facility_Code + "|" + f.Facility_Name,
                    }).ToList();

                    responseModel.Status = "Success";
                    responseModel.Response = list;
                    return responseModel;
                }
            }
            catch (Exception ex)
            {
                responseModel.Status = ex.ToString();
            }
            return responseModel;

        }

        public ResponseModel GetFacility(long PracticeCode)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                List<PRACTICE_FACILITY> PracticeFacilityList = null;
                using (var ctx = new NPMDBEntities())
                {
                    PracticeFacilityList = ctx.PRACTICE_FACILITY.Where(c => c.Practice_Code == PracticeCode && (c.Deleted == null || c.Deleted == false)).ToList();
                }
                if (PracticeFacilityList != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = PracticeFacilityList;
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

        public ResponseModel GetDiagnosis(string DiagCode, string DiagDesc, long PracticeCode)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                List<Claim_Provider_Diagnosis_Proc_Result> DiagnosisList = null;

                using (var ctx = new NPMDBEntities())
                {
                    //DiagnosisList = ctx.Diagnosis.Where(c => c.Diag_Code == DiagCode && (c.Deleted == null || c.Deleted == false)).ToList();
                    DiagnosisList = ctx.Claim_Provider_Diagnosis_Proc(PracticeCode, DiagCode, DiagDesc).ToList();
                }
                if (DiagnosisList != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = DiagnosisList;
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

        public ResponseModel AddDxToProvider(string DiagCode, long PracticeCode)
        {
            ResponseModel res = new ResponseModel();
            using (var ctx = new NPMDBEntities())
            {
                try
                {
                    var sp_result = ctx.Add_DxCode_For_Provider_diagnosis(PracticeCode, DiagCode);
                    if (sp_result > 0)
                    {
                        res.Status = "Success";
                        res.Response = sp_result;
                    }
                    else
                    {
                        res.Status = "error";
                        res.Response = "Error in Insertion";
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
            return res;
        }

        public ResponseModel GetProcedures(long PracticeCode)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                List<PRACTICE_FACILITY> PracticeFacilityList = null;
                using (var ctx = new NPMDBEntities())
                {
                    PracticeFacilityList = ctx.PRACTICE_FACILITY.Where(c => c.Practice_Code == PracticeCode && (c.Deleted == null || c.Deleted == false)).ToList();

                }

                if (PracticeFacilityList != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = PracticeFacilityList;
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

        public ResponseModel SearchFacilities([FromBody] FacilitySearchModel model)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                List<SP_FacilitiesSearch_Result> FacilityList = null;
                using (var ctx = new NPMDBEntities())
                {
                    if(model.PracticeCode == 0)
                    {
                        model.PracticeCode = null;
                    }
                    FacilityList = ctx.SP_FacilitiesSearch(model.FacilityCode, model.FacilityName, model.FacilityType, model.NPI, model.ZIP, model.City, model.State , model.PracticeCode).ToList();
                }

                if (FacilityList != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = FacilityList;
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

        public ResponseModel SavePatientClaimDiagnose(string DiagCode, long ClaimNo, bool IsEdit, int Sequence)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                Claim objClaim = null;
                using (var ctx = new NPMDBEntities())
                {
                    objClaim = ctx.Claims.SingleOrDefault(p => p.Claim_No == ClaimNo);
                    if (objClaim == null)
                    {
                        if (IsEdit == false)
                        {
                            if (string.IsNullOrEmpty(objClaim.DX_Code1))
                                objClaim.DX_Code1 = DiagCode;
                            else if (string.IsNullOrEmpty(objClaim.DX_Code2))
                                objClaim.DX_Code2 = DiagCode;
                            else if (string.IsNullOrEmpty(objClaim.DX_Code3))
                                objClaim.DX_Code3 = DiagCode;
                            else if (string.IsNullOrEmpty(objClaim.DX_Code4))
                                objClaim.DX_Code4 = DiagCode;
                            else if (string.IsNullOrEmpty(objClaim.DX_Code5))
                                objClaim.DX_Code5 = DiagCode;
                            else if (string.IsNullOrEmpty(objClaim.DX_Code6))
                                objClaim.DX_Code6 = DiagCode;
                            else if (string.IsNullOrEmpty(objClaim.DX_Code7))
                                objClaim.DX_Code7 = DiagCode;
                            else if (string.IsNullOrEmpty(objClaim.DX_Code8))
                                objClaim.DX_Code8 = DiagCode;
                            else if (string.IsNullOrEmpty(objClaim.DX_Code9))
                                objClaim.DX_Code9 = DiagCode;
                            else if (string.IsNullOrEmpty(objClaim.DX_Code10))
                                objClaim.DX_Code10 = DiagCode;
                            else if (string.IsNullOrEmpty(objClaim.DX_Code11))
                                objClaim.DX_Code11 = DiagCode;
                            else if (string.IsNullOrEmpty(objClaim.DX_Code12))
                                objClaim.DX_Code12 = DiagCode;
                        }
                        else
                        {
                            if (Sequence == 1)
                                objClaim.DX_Code1 = DiagCode;
                            else if (Sequence == 2)
                                objClaim.DX_Code2 = DiagCode;
                            else if (Sequence == 3)
                                objClaim.DX_Code3 = DiagCode;
                            else if (Sequence == 4)
                                objClaim.DX_Code4 = DiagCode;
                            else if (Sequence == 5)
                                objClaim.DX_Code5 = DiagCode;
                            else if (Sequence == 6)
                                objClaim.DX_Code6 = DiagCode;
                            else if (Sequence == 7)
                                objClaim.DX_Code7 = DiagCode;
                            else if (Sequence == 8)
                                objClaim.DX_Code8 = DiagCode;
                            else if (Sequence == 9)
                                objClaim.DX_Code9 = DiagCode;
                            else if (Sequence == 10)
                                objClaim.DX_Code10 = DiagCode;
                            else if (Sequence == 11)
                                objClaim.DX_Code11 = DiagCode;
                            else if (Sequence == 12)
                                objClaim.DX_Code12 = DiagCode;
                        }
                        ctx.SaveChanges();
                    }

                }
                objResponse.Status = "Sucess";

            }
            catch (Exception)
            {
                objResponse.Status = "Error";
            }
            return objResponse;
        }

        public ResponseModel EditDiagnosis(string DiagCode, long ClaimNo)
        {
            throw new NotImplementedException();
        }

        public ResponseModel DeleteClaim(long ClaimNo)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                List<Claim> PatientClaimsList = null;
                using (var ctx = new NPMDBEntities())
                {

                    Claim objClaim = ctx.Claims.SingleOrDefault(c => c.Claim_No == ClaimNo && (c.Deleted == null || c.Deleted == false));
                    if (objClaim != null)
                    {
                        objClaim.Deleted = true;
                        ctx.SaveChanges();
                    }

                    PatientClaimsList = ctx.Claims.Where(c => c.Claim_No == ClaimNo && (c.Deleted == null || c.Deleted == false)).ToList();

                }

                if (PatientClaimsList != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = PatientClaimsList;
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

        public ResponseModel GetState()
        {
            ResponseModel objResponse = new ResponseModel();
            List<State> states = null;

            try  
            {

              
                using (var ctx = new NPMDBEntities())
                {

                    states = ctx.States.Where(c => (c.Deleted == null || c.Deleted == false)).ToList();

                
               
           
                objResponse.Status = "Sucess";
                objResponse.Response = states;
                }

            }
            catch (Exception)
            {
                objResponse.Status = "Error";
            }
            return objResponse;
        }
        public ResponseModel GetCityState(string ZipCode)
        {
            ResponseModel objResponse = new ResponseModel();

            try
            {
                ZipCode = ZipCode.Length > 5 ? ZipCode.Substring(0, 5) : ZipCode;
                List<CityStateModel> cityStateList = new List<CityStateModel>();

                using (var ctx = new NPMDBEntities())
                {
                    var zipCityStates = ctx.Zip_City_State
                                           .Where(c => c.ZIP_Code == ZipCode && (c.Deleted == null || c.Deleted == false))
                                           .ToList();

                    if (zipCityStates.Any())
                    {
                        cityStateList = zipCityStates.Select(z => new CityStateModel
                        {
                            CityName = z.City_Name,
                            State = z.State_Code
                        }).ToList();

                        objResponse.Status = "Success";
                        objResponse.Response = cityStateList;
                    }
                    else
                    {
                        objResponse.Status = "Error";
                        objResponse.Response = "No records found.";
                    }
                }
            }
            catch (Exception ex)
            {
                objResponse.Status = "Error";
                objResponse.Response = $"Exception: {ex.Message}";
            }

            return objResponse;
        }


        public ResponseModel GetPatientPicture(long PatientAccount)
        {
            ResponseModel objResponse = new ResponseModel();

            try
            {
                var filename = $@"\Content\PatientsPictures\{PatientAccount}.";
                if (filename != null)
                {

                    objResponse.Status = "Sucess";
                    objResponse.Response = filename;
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

        public ResponseModel SearchInsurance([FromBody] InsuranceSearchViewModel model)
        {
            ResponseModel objResponse = new ResponseModel();
            List<InsuranceSearchViewModel> objInsuranceList = null;
            using (var ctx = new NPMDBEntities())
            {
                List<SP_InsuranceSearch_Result> searchResult = ctx.SP_InsuranceSearch(model.PracticeCode, model.PayerId, model.PayerDescription, model.NameId, model.InsuranceName, model.InsuranceId, model.GroupName, model.ZIP, model.InsuranceAddress, model.State, model.City, model.SearchFrom).ToList();

                if (searchResult != null)
                {
                    objResponse.Status = "success";
                    objResponse.Response = searchResult;
                }
                else
                {
                    objResponse.Status = "Error";
                }
                return objResponse;
            }
        }

        public ResponseModel SavePatientInsurance(Patient_Insurance Model)
        {
            ResponseModel objResponse = new ResponseModel();
            Patient_Insurance objNewPatientInsurance = null;

            using (var ctx = new NPMDBEntities())
            {
                if (Model.Patient_Insurance_Id != 0)
                {
                    objNewPatientInsurance = ctx.Patient_Insurance.SingleOrDefault(p => p.Patient_Insurance_Id == Model.Patient_Insurance_Id);
                    if (objNewPatientInsurance != null)
                    {

                        objNewPatientInsurance.Insurance_Id = Model.Insurance_Id;
                        objNewPatientInsurance.Pri_Sec_Oth_Type = Model.Pri_Sec_Oth_Type;
                        objNewPatientInsurance.Policy_Number = Model.Policy_Number;
                        objNewPatientInsurance.Subscriber = Model.Subscriber;
                        objNewPatientInsurance.Relationship = Model.Relationship;
                        objNewPatientInsurance.Co_Payment = Model.Co_Payment;
                        objNewPatientInsurance.Group_Number = Model.Group_Number;
                        objNewPatientInsurance.Group_Name = Model.Group_Name;

                        objNewPatientInsurance.Deductions = Model.Deductions;
                        objNewPatientInsurance.Co_Payment_Per = Model.Co_Payment_Per;
                        objNewPatientInsurance.CCN = Model.CCN;
                        objNewPatientInsurance.Visits_Start_Date = Model.Visits_Start_Date;
                        objNewPatientInsurance.Visits_End_Date = Model.Visits_End_Date;
                        objNewPatientInsurance.Access_Carolina_Number = Model.Access_Carolina_Number;
                        objNewPatientInsurance.Is_Capitated_Patient = Model.Is_Capitated_Patient;
                        objNewPatientInsurance.Created_Date = DateTime.Now;
                        objNewPatientInsurance.IS_Active = Model.IS_Active;
                        objNewPatientInsurance.Deleted = false;


                        ctx.SaveChanges();

                    }
                }
                else
                {
                    objNewPatientInsurance = new Patient_Insurance();
                    long patInsuranceId = Convert.ToInt64(ctx.SP_TableIdGenerator("Patient_Insurance_Id").FirstOrDefault().ToString());//ctx.Patient_Insurance.Max(p => p.Patient_Insurance_Id);


                    objNewPatientInsurance = Model;
                    objNewPatientInsurance.Patient_Insurance_Id = patInsuranceId;
                    ctx.Patient_Insurance.Add(objNewPatientInsurance);
                    ctx.SaveChanges();
                }
            }
            if (objNewPatientInsurance != null)
            {
                objResponse.Status = "Sucess";
                objResponse.Response = objNewPatientInsurance.Patient_Insurance_Id;
            }
            else
            {
                objResponse.Status = "Error";
            }

            return objResponse;
        }
        public ResponseModel SavePatientInsurance(PatientInsuranceViewModel Model, long userId)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    if (Model.Patient_Insurance_Id != 0)
                    {
                        var patientInsurance = ctx.Patient_Insurance.FirstOrDefault(p => p.Patient_Insurance_Id == Model.Patient_Insurance_Id);
                        if (patientInsurance != null)
                        {
                            patientInsurance.Insurance_Id = Model.Insurance_Id;
                            patientInsurance.Pri_Sec_Oth_Type = Model.Pri_Sec_Oth_Type;
                            patientInsurance.Policy_Number = Model.Policy_Number;
                            patientInsurance.Subscriber = Model.Subscriber;
                            patientInsurance.Relationship = Model.Relationship;
                            patientInsurance.Co_Payment = Model.Co_Payment;
                            patientInsurance.Group_Number = Model.Group_Number;
                            patientInsurance.Group_Name = Model.Group_Name;
                            patientInsurance.Deductions = Model.Deductions;
                            patientInsurance.Co_Payment_Per = Model.Co_Payment_Per;
                            patientInsurance.CCN = Model.CCN;
                            patientInsurance.Effective_Date = Model.Effective_Date;
                            patientInsurance.Termination_Date = Model.Termination_Date;
                            patientInsurance.Access_Carolina_Number = Model.Access_Carolina_Number;
                            patientInsurance.Is_Capitated_Patient = Model.Is_Capitated_Patient;
                            patientInsurance.Modified_Date = DateTime.Now;
                            patientInsurance.Modified_By = userId;
                            patientInsurance.Deleted = false;
                            patientInsurance.IS_Active = Model.IS_Active;
                            ctx.Entry(patientInsurance).State = EntityState.Modified;
                        }
                    }
                    else
                    {
                        ctx.Patient_Insurance.Add(new Patient_Insurance()
                        {
                            Patient_Insurance_Id = Convert.ToInt64(ctx.SP_TableIdGenerator("Patient_Insurance_Id").FirstOrDefault().ToString()),
                            Insurance_Id = Model.Insurance_Id,
                            Pri_Sec_Oth_Type = Model.Pri_Sec_Oth_Type,
                            Policy_Number = Model.Policy_Number,
                            Subscriber = Model.Subscriber,
                            Relationship = Model.Relationship,
                            Co_Payment = Model.Co_Payment,
                            Group_Number = Model.Group_Number,
                            Group_Name = Model.Group_Name,
                            Deductions = Model.Deductions,
                            Co_Payment_Per = Model.Co_Payment_Per,
                            CCN = Model.CCN,
                            Effective_Date = Model.Effective_Date,
                            Termination_Date = Model.Termination_Date,
                            Access_Carolina_Number = Model.Access_Carolina_Number,
                            Is_Capitated_Patient = Model.Is_Capitated_Patient,
                            Created_Date = DateTime.Now,
                            Created_By = userId,
                            Patient_Account = Model.Patient_Account,
                            IS_Active = Model.IS_Active
                    });
                    }
                    ctx.SaveChanges();
                    objResponse.Status = "Sucess";
                }
            }
            catch (Exception ex)
            {
                objResponse.Status = ex.ToString();
            }
            return objResponse;
        }

        public ResponseModelForE InquiryByPracPatProvider(long PracticeCode, long PatAcccount, long ProviderCode, long insurance_id)
        {
            ResponseModelForE res = new ResponseModelForE();
            WSEligibilityResponse response = null;
            if (PracticeCode > 0 && PatAcccount > 0 && ProviderCode > 0)
            {
                try
                {
                    EligRequests_Result eligibilityModel = _iEligibility.GetEligibilityModel(PracticeCode, PatAcccount, ProviderCode, insurance_id).Response;
                    if (eligibilityModel != null)
                    {
                        response = _iEligibility.DoInquiry(new DoInquiryModel()
                        {
                            GediPayerID = eligibilityModel.INSPAYER_837_ID ?? "",
                            InsuranceNum = eligibilityModel.INSURANCENUM ?? "",
                            InsuredDOB = eligibilityModel.INSUREDDOB != null ? ((DateTime)eligibilityModel.INSUREDDOB).ToString("yyyyMMdd") : "",
                            InsuredFirstName = eligibilityModel.INSUREDFIRSTNAME ?? "",
                            InsuredLastName = eligibilityModel.INSUREDLASTNAME ?? "",
                            InsuredSSN = eligibilityModel.SSN ?? "",
                            InsuredState = eligibilityModel.INSUREDSTATE ?? "",
                            Npi = eligibilityModel.PRACTICE_NPI ?? "",
                            ProviderFirstName = eligibilityModel.PROVIDERFIRSTNAME.ToString(),
                            ProviderLastName = eligibilityModel.PROVIDERLASTNAME.ToString()

                        }, (long)eligibilityModel.PRACTICE_CODE);

                        if (response != null && response.SuccessCode == SuccessCode.Success)
                        {
                            XmlDocument doc = new XmlDocument();
                            if(response.ResponseAsXml != null)
                            {
                                doc.LoadXml(response.ResponseAsXml);
                                string json = JsonConvert.SerializeXmlNode(doc);
                                res.Status = "Success";
                                res.Response = "Green";
                                res.SuccessCode = response.SuccessCode;

                                res.Data = json;
                                res.SuccessCodeText = response.ExtraProcessingInfo.AllMessages;
                            }
                            else
                            {
                                res.Status = "Success";
                                res.Response = "Red";
                                res.SuccessCode = 2;
                            }
                      
                        }
                        else
                        {
                            res.Status = "Success";
                            res.Response = "Red";
                            res.SuccessCode = response.SuccessCode;
                            res.SuccessCodeText = response.ExtraProcessingInfo.AllMessages;
                        }
                    }
                    else
                    {
                        res.Status = "error";
                        res.Response = "No Information Found for Patient";

                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
            return res;
        }
        public ResponseModel ValidateAddress(ValidateAddreesRequestViewModel model)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                var auth_id = !string.IsNullOrEmpty(model.auth_id) ? model.auth_id : GlobalVariables.auth_id;
                var auth_token = !string.IsNullOrEmpty(model.auth_token) ? model.auth_token : GlobalVariables.auth_token;
                var candidates = !string.IsNullOrEmpty(model.candidates) ? model.candidates : GlobalVariables.candidates;
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(GlobalVariables.ValidateAddressBaseUrl);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var urlParameters = $"?auth-id={auth_id}&auth-token={auth_token}&candidates={candidates}&street={model.street}&city={model.city}&state={model.state}&zipcode={model.zipcode}";
                    HttpResponseMessage responseMessage = client.GetAsync(urlParameters).Result;
                    if (responseMessage.IsSuccessStatusCode)
                    {
                        IEnumerable<ValidateAddressResponseViewModel> resp = responseMessage.Content.ReadAsAsync<IEnumerable<ValidateAddressResponseViewModel>>().Result;
                        if (resp != null && resp.Count() > 0)
                        {
                            responseModel.Response = resp;
                            responseModel.Status = "Success";
                        }
                        else
                        {
                            responseModel.Status = "No Address Found";
                        }
                    }
                    else
                    {
                        responseModel.Status = responseMessage.StatusCode.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                responseModel.Status = ex.ToString();
            }
            return responseModel;
        }

        public ResponseModel GetPatientSelectList(string searchText, long practiceCode)
        {
            ResponseModel responseModel = new ResponseModel();
            List<SelectListViewModel> list;
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var patients = ctx.Patients.Where(p => ((p.Deleted ?? false) == false && p.Practice_Code == practiceCode) && (p.First_Name.Contains(searchText)
                         || p.Last_Name.Contains(searchText) || p.Patient_Account.ToString().Contains(searchText))).ToList();
                    list = patients.Select(p => new SelectListViewModel()
                    {
                        Id = p.Patient_Account,
                        //Name = p.Patient_Account +","+p.Last_Name + p.First_Name + " | " + Convert.ToDateTime(p.Date_Of_Birth).ToString("MM/dd/yyyy")
                        Name = p.Patient_Account + "," + p.Last_Name +" "+ p.First_Name
                    }).ToList();
                    if (list != null)
                    {
                        responseModel.Status = "Success";
                        responseModel.Response = list;
                    }
                    else
                    {
                        responseModel.Status = "Error";
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return responseModel;
        }

        public ResponseModel GetInsuranceSelectList(string searchText)
        {
            ResponseModel responseModel = new ResponseModel();
            List<SelectListViewModel> list;
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    list = (from i in ctx.Insurances
                            join ip in ctx.Insurance_Payers on i.InsPayer_Id equals ip.Inspayer_Id
                            join ipn in ctx.Insurance_Names on ip.Insname_Id equals ipn.Insname_Id
                            join ig in ctx.Insurance_Groups on ipn.Insgroup_Id equals ig.Insgroup_Id
                            where (ig.Insgroup_Name.Contains(searchText) || ip.Inspayer_Description.Contains(searchText) ||
                            ipn.Insname_Description.Contains(searchText)) && (i.Deleted ?? false) == false
                            select new SelectListViewModel()
                            {
                                Id = i.Insurance_Id,
                                IdStr = "Group: " + ig.Insgroup_Name + ", Payer: " + ip.Inspayer_Description + ", Name: " + ipn.Insname_Description,
                                Name = ipn.Insname_Description
                            }).Take(200).ToList();
                    if (list != null)
                    {
                        responseModel.Status = "Success";
                        responseModel.Response = list;
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

        public ResponseModel GetProviderSelectList(string searchText, long practiceCode, bool all = false)
        {
            ResponseModel responseModel = new ResponseModel();
            List<SelectListViewModel> list = new List<SelectListViewModel>();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    if (!all)
                    {
                        list = ctx.Providers.Where(p => p.Practice_Code == practiceCode && (p.Deleted ?? false) == false
                                   && (p.Provid_FName.Contains(searchText) || p.Provid_LName.Contains(searchText) ||
                                   p.Practice_Code.ToString().Contains(searchText) || p.NPI.Contains(searchText) || p.Provider_Code.ToString().Contains(searchText)
                                   )).Select(p => new SelectListViewModel()
                                   {
                                       Id = p.Provider_Code,
                                       Name = p.Provider_Code + "|" + p.Provid_LName + ", " + p.Provid_FName
                                   }).ToList();
                    }
                    else
                    {
                        list = ctx.Providers.Where(p => p.Practice_Code == practiceCode && (p.Deleted ?? false) == false).Select(p => new SelectListViewModel()
                        {
                            Id = p.Provider_Code,
                            Name = p.Provider_Code + "|" + p.Provid_LName + ", " + p.Provid_FName
                        }).ToList();
                    }
                    if (list != null)
                    {
                        responseModel.Status = "Success";
                        responseModel.Response = list;
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

        public ResponseModel GetLocationSelectList(string searchText, long practiceCode, bool all = false)
        {
            ResponseModel responseModel = new ResponseModel();
            List<SelectListViewModel> list = new List<SelectListViewModel>();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    if (!all)
                    {
                        list = ctx.Practice_Locations.Where(p => (p.Practice_Code == practiceCode && (p.Deleted ?? false) == false) && (p.Location_Name.Contains(searchText) ||
                        p.Location_Code.ToString().Contains(searchText))).Select(p => new SelectListViewModel()
                        {
                            Id = p.Location_Code,
                            Name = p.Location_Code + "|" + p.Location_Name,
                        }).ToList();
                    }
                    else
                    {
                        list = ctx.Practice_Locations.Where(p => (p.Practice_Code == practiceCode && (p.Deleted ?? false) == false)).Select(p => new SelectListViewModel()
                        {
                            Id = p.Location_Code,
                            Name = p.Location_Code + "|" + p.Location_Name,
                        }).ToList();
                    }
                    responseModel.Status = "Success";
                    responseModel.Response = list;
                }
            }
            catch (Exception ex)
            {
                responseModel.Status = ex.ToString();
            }
            return responseModel;
        }

        //for panel billing - pir ubaid
        public ResponseModel GetPanelBillingLocationSelectList(long practiceCode, bool all = false)
        {
            ResponseModel responseModel = new ResponseModel();
            List<SelectListViewModel> list = new List<SelectListViewModel>();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    if (!all)
                    {
                        // Only fetch locations that match the practice code and are not deleted
                        list = ctx.Practice_Locations
                            .Where(p => p.Practice_Code == practiceCode && (p.Deleted ?? false) == false)
                            .Select(p => new SelectListViewModel()
                            {
                                Id = p.Location_Code,
                                Name = p.Location_Code + "|" + p.Location_Name,
                            })
                            .ToList();
                    }
                    else
                    {
                        // Fetch all locations that match the practice code and are not deleted
                        list = ctx.Practice_Locations
                            .Where(p => p.Practice_Code == practiceCode && (p.Deleted ?? false) == false)
                            .Select(p => new SelectListViewModel()
                            {
                                Id = p.Location_Code,
                                Name = p.Location_Code + "|" + p.Location_Name,
                            })
                            .ToList();
                    }

                    responseModel.Status = "Success";
                    responseModel.Response = list;
                }
            }
            catch (Exception ex)
            {
                responseModel.Status = ex.ToString();
            }
            return responseModel;
        }

        public ResponseModel GetPracticeDefaultLocation(long practiceCode)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    responseModel.Response = ctx.Practice_Locations.Where(pl => pl.Practice_Code == practiceCode && !(pl.Deleted ?? false) && (pl.Is_Active ?? false) && (pl.is_default ?? false)).Select(pl => new SelectListViewModel()
                    {
                        Id = pl.Location_Code,
                        Name = pl.Location_Code + "|" + pl.Location_Name
                    }).FirstOrDefault();
                    responseModel.Status = "Success";
                }
            }
            catch (Exception ex)
            {
                responseModel.Status = ex.ToString();
            }
            return responseModel;
        }

        public ResponseModel GetPatientSummary(long patientAccount, long practiceCode)
        {
            ResponseModel responseModel = new ResponseModel();
            PatientSummaryVM patientSummary;
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    patientSummary = ctx.Patients.Where(p => p.Patient_Account == patientAccount && p.Practice_Code == practiceCode).Select(p => new PatientSummaryVM()
                    {
                        Address = p.Address,
                        City = p.City,
                        DOB = p.Date_Of_Birth,
                        FirstName = p.First_Name,
                        LastName = p.Last_Name,
                        Gender = p.Gender,
                        MI = p.MI,
                        PatientAccount = p.Patient_Account,
                        State = p.State,
                        ZIP = p.ZIP
                    }).FirstOrDefault();
                    if (patientSummary != null)
                    {
                        responseModel.Status = "Success";
                        responseModel.Response = patientSummary;
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

        public ResponseModel GetClaimSummaryByNo(long claimNo, long practiceCode)
        {
            ResponseModel responseModel = new ResponseModel();
            ClaimSummaryVM claimSummary;
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    claimSummary = ctx.Claims.Where(c => c.Claim_No == claimNo).Select(c => new ClaimSummaryVM()
                    {
                        Claim_No = c.Claim_No,
                        Amt_Due = c.Amt_Due,
                        Amt_Paid = c.Amt_Paid,
                        Claim_Total = c.Claim_Total,
                        DOS = c.DOS,
                        Patient_Account = c.Patient_Account
                    }).FirstOrDefault();
                    if (claimSummary != null)
                    {
                        responseModel.Status = "Success";
                        responseModel.Response = claimSummary;
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

        public ResponseModel GetPatientClaimsForStatement(long patientAccount)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var results = ctx.GetClaimsForStatement(patientAccount).ToList();
                    responseModel.Status = "Success";
                    responseModel.Response = results;
                }
            }
            catch (Exception ex)
            {
                responseModel.Status = ex.ToString();
            }
            return responseModel;
        }

        public ResponseModel GeneratePatientStatement(PatientStatementRequest model, long v)
        {
            ResponseModel responseModel = new ResponseModel();
            PatientStatementResponse patientStatementResponse = new PatientStatementResponse();
            try
            {
                PracticeFTPViewModel ftpInfo = practiceRepository.GetPracticeFTPInfo(model.PracticeCode, FTPType.PatientStatement);
                // FTP not enabled, and confirmation is not made
                if (ftpInfo == null && !model.Confirmation)
                {
                    responseModel.Status = "Confirmation";
                    responseModel.Response = "Patient Statement uploading service is not enabled for the selected practice, are you sure you want to generate patient statement?";
                    return responseModel;
                }
                // FTP not enabled, and confirmation is made
                else if (ftpInfo == null && model.Confirmation)
                {
                    patientStatementResponse = GenerateXMLStatement(model);
                    UpdateClaimStatus(patientStatementResponse.ClaimsInfo.Select(c => c.ClaimNo).ToList(), "B", "Pat_Status");
                    AddPatientStatementNote(patientStatementResponse, v);
                }
                // FTP is enabled
                else
                {
                    patientStatementResponse = GenerateXMLStatement(model);
                    if (!Debugger.IsAttached)
                    {
                        ftp.upload(ftpInfo.Host, ftpInfo.Port, ftpInfo.Username, ftpInfo.Password, patientStatementResponse.Path, ftpInfo.Destination);
                    }
                    UpdateClaimStatus(patientStatementResponse.ClaimsInfo.Select(c => c.ClaimNo).ToList(), "B", "Pat_Status");
                    AddPatientStatementNote(patientStatementResponse, v);
                }
                #region excluded
                //switch (model.Format)
                //{
                //    case "xml":
                //        {
                //            fileGenerationResponse = GenerateXMLStatement(model);
                //            break;
                //        }
                //    case "txt":
                //        {
                //            fileGenerationResponse = GenerateTXTStatement(model);
                //            break;
                //        }
                //    default:
                //        {
                //            fileGenerationResponse = GenerateTXTStatement(model);
                //            break;
                //        }
                //} 
                #endregion
                responseModel.Status = "Success";
                return responseModel;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private PatientStatementResponse GenerateXMLStatement(PatientStatementRequest model)
        {
            var response = new PatientStatementResponse();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var patientsAccounts = string.Join(",", model.statementRequest.Select(i => i.PatientAccount).ToList());
                    var excludedClaims = string.Join(",", model.statementRequest.Select(m => new { m.PatientAccount, m.ExcludedClaimsIds }).SelectMany(m => m.ExcludedClaimsIds).ToList());
                    var patientsWithClaims = ctx.GetPatientWithClaimsForStatement(patientsAccounts, string.IsNullOrEmpty(excludedClaims) ? null : excludedClaims).GroupBy(g => g.Patient_Account).ToList();

                    XmlDocument doc = new XmlDocument();
                    // Document Node
                    XmlNode docNode = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
                    doc.AppendChild(docNode);
                    // Root Node
                    XmlNode rootNote = doc.CreateElement("root");
                    doc.AppendChild(rootNote);
                    foreach (var pcGroup in patientsWithClaims)
                    {
                        List<PatientStatementViewModelFromSpXML> statementsList = new List<PatientStatementViewModelFromSpXML>();
                        string[] claimsStrArray = pcGroup.Select(e => e.claim_no.ToString()).ToArray();
                        string claimsStr = string.Join(",", claimsStrArray);
                        statementsList.AddRange(
                            ctx.Database.SqlQuery<PatientStatementViewModelFromSpXML>("SP_GENERATEPATIENTSTATEMENT_XML @PATACCT,@PRAC,@CLMNO", parameters: new[] {
                                new SqlParameter("@PATACCT", Convert.ToInt64(pcGroup.Key)),
                                new SqlParameter("@PRAC", model.PracticeCode),
                                new SqlParameter("@CLMNO", claimsStr)
                            }).ToList());
                        var statementGroups = statementsList.GroupBy(g => g.claim_no);
                        // Billing Date
                        XmlNode billingDateNode = doc.CreateElement("Bill_Date");
                        billingDateNode.AppendChild(doc.CreateTextNode(DateTime.Now.ToString("MM/dd/yyyy")));
                        rootNote.AppendChild(billingDateNode);
                        // Practice Name
                        XmlNode practiceName = doc.CreateElement("Practice_Name");
                        practiceName.AppendChild(doc.CreateTextNode(statementGroups.FirstOrDefault().FirstOrDefault().PRAC_NAME));
                        rootNote.AppendChild(practiceName);
                        // Practice Address
                        XmlNode practiceAddress = doc.CreateElement("Practice_Address");
                        practiceAddress.AppendChild(doc.CreateTextNode(statementGroups.FirstOrDefault().FirstOrDefault().PAT_BILLING_ADDRESS + " " + statementGroups.FirstOrDefault().FirstOrDefault().PAT_BILLING_CITY_STATE_ZIP));
                        rootNote.AppendChild(practiceAddress);
                        // Patient Name
                        XmlNode patientName = doc.CreateElement("Patient_Name");
                        patientName.AppendChild(doc.CreateTextNode(statementGroups.FirstOrDefault().FirstOrDefault().NAME));
                        rootNote.AppendChild(patientName);
                        // Patient Account Number
                        XmlNode patientAccount = doc.CreateElement("Patient_Account");
                        patientAccount.AppendChild(doc.CreateTextNode(statementGroups.FirstOrDefault().FirstOrDefault().patient_account));
                        rootNote.AppendChild(patientAccount);
                        // Patient Address
                        XmlNode patientAddress = doc.CreateElement("Patient_Address");
                        // Address
                        XmlNode patientAddressAddress = patientAddress.AppendChild(doc.CreateElement("Address"));
                        patientAddressAddress.AppendChild(doc.CreateTextNode(statementGroups.FirstOrDefault().FirstOrDefault().PAT_ADDRESS));
                        // City
                        XmlNode patientAddressCity = patientAddress.AppendChild(doc.CreateElement("City"));
                        patientAddressCity.AppendChild(doc.CreateTextNode(statementGroups.FirstOrDefault().FirstOrDefault().CITY));
                        // State
                        XmlNode patientAddressState = patientAddress.AppendChild(doc.CreateElement("State"));
                        patientAddressState.AppendChild(doc.CreateTextNode(statementGroups.FirstOrDefault().FirstOrDefault().STATE));
                        // Zip
                        XmlNode patientAddressZip = patientAddress.AppendChild(doc.CreateElement("Zip"));
                        patientAddressZip.AppendChild(doc.CreateTextNode(statementGroups.FirstOrDefault().FirstOrDefault().ZIP));
                        rootNote.AppendChild(patientAddress);
                        // Claims
                        XmlNode claims = doc.CreateElement("Claims");

                        decimal totalDue = 0;
                        // Writing Claim items
                        foreach (var group in statementGroups)
                        {
                            XmlNode claim = doc.CreateElement("Claim");
                            XmlNode cptCharges = doc.CreateElement("Charges");
                            foreach (var item in group)
                            {
                                XmlNode charge = doc.CreateElement("Charge");
                                Type PropertyType = item.GetType();
                                for (int j = 0; j < 5; j++)
                                {
                                    PropertyInfo propertyInfo = PropertyType.GetProperties()[j];
                                    XmlNode childNode = doc.CreateElement(propertyInfo.Name);
                                    var columnValue = propertyInfo.GetValue(item);
                                    var columnValueString = columnValue == null ? "" : columnValue.ToString();
                                    if (propertyInfo.Name == "amount")
                                    {
                                        string strAmount = columnValueString;
                                        decimal decAmount;
                                        if (decimal.TryParse(strAmount, out decAmount))
                                        {
                                            childNode.AppendChild(doc.CreateTextNode(string.Format("{0:C}", decAmount)));
                                            charge.AppendChild(childNode);
                                        }
                                    }
                                    else
                                    {
                                        childNode.AppendChild(doc.CreateTextNode(columnValueString));
                                        charge.AppendChild(childNode);
                                    }
                                }
                                cptCharges.AppendChild(charge);
                            }
                            totalDue += (group.FirstOrDefault().amtDue ?? 0);
                            claim.AppendChild(cptCharges);
                            XmlNode DuePerClaim = doc.CreateElement("Due_Per_Claim");
                            DuePerClaim.AppendChild(doc.CreateTextNode(string.Format("{0:C}", group.FirstOrDefault().amtDue)));
                            claim.AppendChild(DuePerClaim);
                            claims.AppendChild(claim);
                            response.ClaimsInfo.Add(new PatientStatementResponseClaimInfo
                            {
                                ClaimNo = group.FirstOrDefault().claim_no,
                                AmountDue = group.FirstOrDefault().amtDue ?? 0
                            });
                        }
                        XmlNode TodalDueNode = doc.CreateElement("Todal_Due");
                        TodalDueNode.AppendChild(doc.CreateTextNode(string.Format("{0:C}", totalDue)));
                        claims.AppendChild(TodalDueNode);
                        rootNote.AppendChild(claims);
                    }
                    // Saving file
                    if (!Directory.Exists(HostingEnvironment.MapPath("~/" + ConfigurationManager.AppSettings["PatientStatementsPath"])))
                        Directory.CreateDirectory(HostingEnvironment.MapPath("~/" + ConfigurationManager.AppSettings["PatientStatementsPath"]));
                    var filePath = HostingEnvironment.MapPath("~/" + ConfigurationManager.AppSettings["PatientStatementsPath"]) + "/" + DateTime.Now.Ticks + ".xml";
                    response.Path = filePath;
                    doc.Save(filePath);
                }
                return response;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #region excluded
        //private bool GenerateTXTStatement(PatientStatementRequest model)
        //{
        //    bool response = true;
        //    try
        //    {
        //        //using (var ctx = new NPMDBEntities())
        //        //{
        //        //    List<PatientStatementViewModelFromSp> statementsList = new List<PatientStatementViewModelFromSp>();
        //        //    foreach (var claimId in model.ClaimIds)
        //        //    {
        //        //        statementsList.AddRange(ctx.Database.SqlQuery<PatientStatementViewModelFromSp>("SP_GENERATEPATIENTSTATEMENT @PATACCT,@PRAC,@CLMNO", parameters: new[] { new SqlParameter("@PATACCT", model.PatientAccount), new SqlParameter("@PRAC", model.PracticeCode), new SqlParameter("@CLMNO", claimId.ToString()) }).ToList());
        //        //    }
        //        //    var statementGroups = statementsList.GroupBy(g => g.claim_no);
        //        //    StringBuilder txt = new StringBuilder();
        //        //    // BOF
        //        //    txt.Append("**BOF");
        //        //    txt.Append(Environment.NewLine);
        //        //    // File Headers
        //        //    txt.Append("*Bill Date= " + "\"" + DateTime.Now.ToString("MM/dd/yyyy") + "\""); /*Billing Date(Date and time when statement has been generated.)*/
        //        //    txt.Append(Environment.NewLine);
        //        //    txt.Append("*Practice Name= \"" + statementGroups.FirstOrDefault().FirstOrDefault().PRAC_NAME + "\""); /*Practice Name*/
        //        //    txt.Append(Environment.NewLine);
        //        //    txt.Append("*Practice Address= \"" + statementGroups.FirstOrDefault().FirstOrDefault().ADDRESS + "\"");
        //        //    txt.Append(Environment.NewLine);
        //        //    txt.Append("*Patient Name= \"" + statementGroups.FirstOrDefault().FirstOrDefault().NAME + "\""); /*Patient Name*/
        //        //    txt.Append(Environment.NewLine);
        //        //    txt.Append("*Patient Address= \"" + statementGroups.FirstOrDefault().FirstOrDefault().PAT_ADDRESS + " " + statementGroups.FirstOrDefault().FirstOrDefault().CITY + " " + statementGroups.FirstOrDefault().FirstOrDefault().STATE + " " + statementGroups.FirstOrDefault().FirstOrDefault().ZIP + "\""); /*Patient Address*/
        //        //    txt.Append(Environment.NewLine);
        //        //    txt.Append(Environment.NewLine);
        //        //    // Writing Claim items
        //        //    foreach (var group in statementGroups)
        //        //    {
        //        //        foreach (var item in group)
        //        //        {
        //        //            // Table Rows
        //        //            Type PropertyType = item.GetType();
        //        //            for (int j = 0; j < 5; j++)
        //        //            {
        //        //                PropertyInfo propertyInfo = PropertyType.GetProperties()[j];
        //        //                if (propertyInfo.CanRead)
        //        //                {
        //        //                    var columnValue = propertyInfo.GetValue(item);
        //        //                    if (columnValue == null)
        //        //                    {
        //        //                        txt.Append("*" + propertyInfo.GetCustomAttributes(typeof(DisplayAttribute), false).Cast<DisplayAttribute>().Single().Name + "= " + "\"\"");
        //        //                        txt.Append(Environment.NewLine);
        //        //                    }
        //        //                    else
        //        //                    {
        //        //                        string columnStringValue = columnValue.ToString();
        //        //                        if (propertyInfo.Name == "amount")
        //        //                        {
        //        //                            string strAmount = columnStringValue;
        //        //                            decimal decAmount;
        //        //                            if (decimal.TryParse(strAmount, out decAmount))
        //        //                            {
        //        //                                txt.Append("*" + propertyInfo.GetCustomAttributes(typeof(DisplayAttribute), false).Cast<DisplayAttribute>().Single().Name + "= \"" + string.Format("{0:C}", decAmount) + "\"");
        //        //                                txt.Append(Environment.NewLine);
        //        //                            }
        //        //                        }
        //        //                        else
        //        //                        {
        //        //                            txt.Append("*" + propertyInfo.GetCustomAttributes(typeof(DisplayAttribute), false).Cast<DisplayAttribute>().Single().Name + "= \"" + columnStringValue + "\"");
        //        //                            txt.Append(Environment.NewLine);
        //        //                        }
        //        //                    }
        //        //                }
        //        //            }
        //        //        }
        //        //        txt.Append("*Due Per Claim:" + string.Format("{0:C}", group.FirstOrDefault().amtDue));
        //        //        txt.Append(Environment.NewLine);
        //        //        txt.Append(Environment.NewLine);
        //        //    }
        //        //    txt.Append(Environment.NewLine);
        //        //    txt.Append("**EOF");
        //        //    // Saving File
        //        //    if (!string.IsNullOrEmpty(txt.ToString()))
        //        //    {
        //        //        try
        //        //        {
        //        //            if (!Directory.Exists(HostingEnvironment.MapPath("~/" + ConfigurationManager.AppSettings["PatientStatementsPath"])))
        //        //                Directory.CreateDirectory(HostingEnvironment.MapPath("~/" + ConfigurationManager.AppSettings["PatientStatementsPath"]));
        //        //            File.WriteAllText(HostingEnvironment.MapPath("~/" + ConfigurationManager.AppSettings["PatientStatementsPath"]) + "/" + DateTime.Now.Ticks + ".txt", txt.ToString());
        //        //        }
        //        //        catch (Exception)
        //        //        {
        //        //            response = false;
        //        //        }
        //        //    }
        //        //}
        //        //response = true;
        //    }
        //    catch (Exception)
        //    {
        //        response = false;
        //    }
        //    return response;
        //}

        //private bool GenerateFormatedCSVStatement(PatientStatementRequest model)
        //{
        //    bool response = true;
        //    //try
        //    //{
        //    //    using (var ctx = new NPMDBEntities())
        //    //    {
        //    //        List<PatientStatementViewModelFromSp> statementsList = new List<PatientStatementViewModelFromSp>();
        //    //        foreach (var claimId in model.ClaimIds)
        //    //        {
        //    //            statementsList.AddRange(ctx.Database.SqlQuery<PatientStatementViewModelFromSp>("SP_GENERATEPATIENTSTATEMENT @PATACCT,@PRAC,@CLMNO", parameters: new[] { new SqlParameter("@PATACCT", model.PatientAccount), new SqlParameter("@PRAC", model.PracticeCode), new SqlParameter("@CLMNO", claimId.ToString()) }).ToList());
        //    //        }
        //    //        var statementGroups = statementsList.GroupBy(g => g.claim_no);
        //    //        StringBuilder csv = new StringBuilder();
        //    //        // File Headers
        //    //        csv.Append(CleanCSVString("Bill Date: " + DateTime.Now.ToString("MM/dd/yyyy"))); /*Billing Date(Date and time when statement has been generated.)*/
        //    //        csv.Append(Environment.NewLine);
        //    //        csv.Append(CleanCSVString(statementGroups.FirstOrDefault().FirstOrDefault().PRAC_NAME)); /*Practice Name*/
        //    //        csv.Append(Environment.NewLine);
        //    //        csv.Append(CleanCSVString(statementGroups.FirstOrDefault().FirstOrDefault().ADDRESS));
        //    //        csv.Append(Environment.NewLine);
        //    //        csv.Append(CleanCSVString(statementGroups.FirstOrDefault().FirstOrDefault().NAME)); /*Patient Name*/
        //    //        csv.Append(Environment.NewLine);
        //    //        csv.Append(CleanCSVString(statementGroups.FirstOrDefault().FirstOrDefault().PAT_ADDRESS + " " + statementGroups.FirstOrDefault().FirstOrDefault().CITY + " " + statementGroups.FirstOrDefault().FirstOrDefault().STATE + " " + statementGroups.FirstOrDefault().FirstOrDefault().ZIP)); /*Patient Address*/
        //    //        csv.Append(Environment.NewLine);
        //    //        // Table Headers
        //    //        List<string> headers = new List<string>();
        //    //        headers.Add("Claim #");
        //    //        headers.Add("Date");
        //    //        headers.Add("Code");
        //    //        headers.Add("Description");
        //    //        headers.Add("Amount ($)");
        //    //        for (int i = 0; i < headers.Count(); i++)
        //    //        {
        //    //            if (i > 0) csv.Append(",");
        //    //            csv.Append(CleanCSVString(headers[i]));
        //    //        }
        //    //        csv.Append(Environment.NewLine);
        //    //        // Writing Claim items
        //    //        foreach (var group in statementGroups)
        //    //        {
        //    //            foreach (var item in group)
        //    //            {
        //    //                // Table Rows
        //    //                Type PropertyType = item.GetType();
        //    //                StringBuilder csvRow = new StringBuilder();
        //    //                for (int j = 0; j < 5; j++)
        //    //                {
        //    //                    PropertyInfo propertyInfo = PropertyType.GetProperties()[j];
        //    //                    if (propertyInfo.CanRead)
        //    //                    {
        //    //                        if (j != 0)
        //    //                        {
        //    //                            csvRow.Append(",");
        //    //                        }
        //    //                        var columnValue = propertyInfo.GetValue(item);
        //    //                        if (columnValue == null)
        //    //                        {
        //    //                            csvRow.Append("");
        //    //                        }
        //    //                        else
        //    //                        {
        //    //                            string columnStringValue = columnValue.ToString();
        //    //                            string cleanedColumnValue = CleanCSVString(columnStringValue);
        //    //                            if (columnValue.GetType() == typeof(string) && !columnStringValue.Contains(","))
        //    //                            {
        //    //                                cleanedColumnValue = "=" + cleanedColumnValue;
        //    //                            }
        //    //                            if (propertyInfo.Name == "amount")
        //    //                            {
        //    //                                string strAmount = columnStringValue;
        //    //                                decimal decAmount;
        //    //                                if (decimal.TryParse(strAmount, out decAmount))
        //    //                                {
        //    //                                    csvRow.Append(string.Format("{0:C}", decAmount));
        //    //                                }
        //    //                            }
        //    //                            else
        //    //                                csvRow.Append(cleanedColumnValue);
        //    //                        }
        //    //                    }
        //    //                }
        //    //                csv.AppendLine(csvRow.ToString());
        //    //            }
        //    //            csv.AppendLine("Due Per Claim:" + string.Format("{0:C}", group.FirstOrDefault().amtDue));
        //    //        }
        //    //        // Saving File
        //    //        if (!string.IsNullOrEmpty(csv.ToString()))
        //    //        {
        //    //            try
        //    //            {
        //    //                if (!Directory.Exists(HostingEnvironment.MapPath("~/" + ConfigurationManager.AppSettings["PatientStatementsPath"])))
        //    //                    Directory.CreateDirectory(HostingEnvironment.MapPath("~/" + ConfigurationManager.AppSettings["PatientStatementsPath"]));
        //    //                File.WriteAllText(HostingEnvironment.MapPath("~/" + ConfigurationManager.AppSettings["PatientStatementsPath"]) + "/" + DateTime.Now.Ticks + ".csv", csv.ToString());
        //    //            }
        //    //            catch (Exception)
        //    //            {
        //    //                response = false;
        //    //            }
        //    //        }
        //    //    }
        //    //    response = true;
        //    //}
        //    //catch (Exception)
        //    //{
        //    //    response = false;
        //    //}
        //    return response;
        //} 
        #endregion

        protected string CleanCSVString(string input)
        {
            string output = "\"" + input.Replace("\"", "\"\"").Replace("\r\n", " ").Replace("\r", " ").Replace("\n", "") + "\"";
            return output;
        }

        public ResponseModel GetStatementPatient(long practiceCode)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var results = ctx.GetPatientForStatement(practiceCode).ToList();
                    if (results != null)
                    {
                        responseModel.Status = "Success";
                        responseModel.Response = results;
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
        //Added By Pir Ubaid (USER STORY : 205 Prior Authorization)
        public ResponseModel GetPAByAccount(long aCCOUNT_NO)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var results = ctx.GET_PA_BY_ACCOUNT(aCCOUNT_NO).ToList();

                    if (results != null && results.Any())
                    {
                        responseModel.Status = "Success";
                        responseModel.Response = results;
                    }
                    else
                    {
                        responseModel.Status = "No data found";
                    }
                }
            }
            catch (Exception ex)
            {
                responseModel.Status = ex.ToString();
            }
            return responseModel;
        }




        public ResponseModel GetClaimAndDos(long patientAcc)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var results = ctx.Claims
                        .Where(c => c.Patient_Account == patientAcc)
                        .Select(c => new { c.Claim_No, c.DOS })
                        .ToList();

                    if (results != null && results.Any())
                    {
                        responseModel.Status = "Success";
                        responseModel.Response = results;
                    }
                    else
                    {
                        responseModel.Status = "No data found";
                    }
                }
            }
            catch (Exception ex)
            {
                responseModel.Status = ex.ToString();
            }
            return responseModel;
        }

        // Added by Pir Ubaid (USER STORY 598 : Collection Status Addition )
        [HttpGet]
        public ResponseModel CheckCollectionStatus(long Claim_no)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                List<SP_PATIENTSTATEMENTCOUNT_BYCLAIM_Result> objCollectionStatus = null;
                using (var ctx = new NPMDBEntities())
                {
                    objCollectionStatus = ctx.SP_PATIENTSTATEMENTCOUNT_BYCLAIM(Claim_no).ToList();
                }

                if (objCollectionStatus != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = objCollectionStatus;
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



        //Added by Pir Ubaid (USER STORY 3055 : Claim summary view changes - Dr. Patel)
        public ResponseModel getClaimDetails(long Claim_no)
        {
            ResponseModel objResponse = new ResponseModel();

            try
            {
                
                List<GetClaimChargesByClaimNo_Result> objClaimDetails = null;

                using (var ctx = new NPMDBEntities())
                {
                    objClaimDetails = ctx.GetClaimChargesByClaimNo(Claim_no).ToList();
                }

                if (objClaimDetails != null && objClaimDetails.Any())
                {
                    objResponse.Status = "Success";
                    objResponse.Response = objClaimDetails;
                }
                else
                {
                    objResponse.Status = "No data found";
                }
            }
            catch (Exception ex)
            {
                objResponse.Status = "Error";
                objResponse.Response = ex.Message;
            }

            return objResponse;
        }

        public ResponseModel AddOrUpdatePanelCPTCodes(PanelCPTCodeList panelCPTCodeList, long userId)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var context = new NPMDBEntities())
                {
                    // Use a transaction to ensure atomicity
                    using (var transaction = context.Database.BeginTransaction())
                    {
                        // Step 1: Retrieve or Create 
                        Panel_Billing_Code_detail panelBillingCodeDetail = context.Panel_Billing_Code_detail
                             .FirstOrDefault(p => p.Panel_Billing_Code_Id == panelCPTCodeList.Panel_Billing_Code_Id
                                && p.Deleted == false);

                        //Panel_Billing_Code_detail panelBillingCodeDetail = context.Panel_Billing_Code_detail
                        //  .FirstOrDefault(p => p.Practice_Code == panelCPTCodeList.PracticeCode
                        //                       && p.Provider_Code == panelCPTCodeList.ProviderCode
                        //                       && p.Location_Code == panelCPTCodeList.LocationCode
                        //                       && p.Panel_Code == panelCPTCodeList.PanelCode
                        //                        && (p.Deleted == false));

                        bool isNewRecord = panelBillingCodeDetail == null;

                        if (isNewRecord)
                        {
                            // Generate new ID for Panel_Billing_Code_Id
                            long newPanelBillingCodeId = Convert.ToInt64(context.SP_TableIdGenerator("Panel_Billing_Code_Id").FirstOrDefault()?.ToString());

                            // Create new Panel_Billing_Code_detail record
                            panelBillingCodeDetail = new Panel_Billing_Code_detail
                            {
                                Panel_Billing_Code_Id = newPanelBillingCodeId,
                                Practice_Code = panelCPTCodeList.PracticeCode,
                                Provider_Code = panelCPTCodeList.ProviderCode,
                                Location_Code = panelCPTCodeList.LocationCode,
                                Panel_Code = panelCPTCodeList.PanelCode,
                                Created_By = userId,
                                Created_Date = DateTime.Now,
                                Deleted = false
                            };
                            context.Panel_Billing_Code_detail.Add(panelBillingCodeDetail);
                        }
                        else
                        {
                            // Update existing record
                            panelBillingCodeDetail.Panel_Code = panelCPTCodeList.PanelCode;
                            panelBillingCodeDetail.Practice_Code = panelCPTCodeList.PracticeCode;
                            panelBillingCodeDetail.Provider_Code = panelCPTCodeList.ProviderCode;
                            panelBillingCodeDetail.Location_Code = panelCPTCodeList.LocationCode;
                            panelBillingCodeDetail.Modified_By = userId;
                            panelBillingCodeDetail.Modified_Date = DateTime.Now;
                        }

                        // Defer SaveChanges for later
                        context.SaveChanges();

                        // Step 2: Handle Panel_Billing_Code_Cptdetail
                        foreach (var cptCode in panelCPTCodeList.CPTCodes)
                        {
                            Panel_Billing_Code_Cptdetail panelBillingCodeCPTDetail = context.Panel_Billing_Code_Cptdetail
                                .FirstOrDefault(p => p.Panel_Billing_Code_CPTId == cptCode.Panel_Billing_Code_CPTId
                                                     && (p.Deleted == false));
                            //Panel_Billing_Code_Cptdetail panelBillingCodeCPTDetail = context.Panel_Billing_Code_Cptdetail
                            //    .FirstOrDefault(p => p.PanelBillingCodeId == panelBillingCodeDetail.Panel_Billing_Code_Id
                            //                         && p.Cpt_Code == cptCode.CPTCode

                            //                         && p.Alternate_Code == cptCode.AlternateCode
                            //                         && p.M_1 == cptCode.M_1
                            //                         && p.M_2 == cptCode.M_2
                            //                         && p.M_3 == cptCode.M_3
                            //                         && p.M_4 == cptCode.M_4
                            //                         && p.Units == cptCode.Units
                            //                         );

                            if (panelBillingCodeCPTDetail != null)
                            {
                                // Update existing CPT detail
                                panelBillingCodeCPTDetail.Cpt_Description = cptCode.CPTDescription;
                                panelBillingCodeCPTDetail.Alternate_Code = cptCode.AlternateCode;
                                panelBillingCodeCPTDetail.Charges = cptCode.Charges;
                                panelBillingCodeCPTDetail.Units = cptCode.Units;
                                panelBillingCodeCPTDetail.M_1 = cptCode.M_1;
                                panelBillingCodeCPTDetail.M_2 = cptCode.M_2;
                                panelBillingCodeCPTDetail.M_3 = cptCode.M_3;
                                panelBillingCodeCPTDetail.M_4 = cptCode.M_4;
                                panelBillingCodeCPTDetail.Modified_By = userId;
                                panelBillingCodeCPTDetail.Modified_Date = DateTime.Now;
                            }
                            else
                            {
                                // Generate new ID for Panel_Billing_Code_CPTId
                                long newPanelBillingCodeCPTId = Convert.ToInt64(context.SP_TableIdGenerator("Panel_Billing_Code_CPTId").FirstOrDefault()?.ToString());

                                // Add new CPT detail
                                panelBillingCodeCPTDetail = new Panel_Billing_Code_Cptdetail
                                {
                                    Panel_Billing_Code_CPTId = newPanelBillingCodeCPTId,
                                    PanelBillingCodeId = panelBillingCodeDetail.Panel_Billing_Code_Id,
                                    Cpt_Code = cptCode.CPTCode,
                                    Cpt_Description = cptCode.CPTDescription,
                                    Alternate_Code = cptCode.AlternateCode,
                                    Charges = cptCode.Charges,
                                    Units = cptCode.Units,
                                    M_1 = cptCode.M_1,
                                    M_2 = cptCode.M_2,
                                    M_3 = cptCode.M_3,
                                    M_4 = cptCode.M_4,
                                    Created_By = userId,
                                    Created_Date = DateTime.Now,
                                    Deleted = false
                                };
                                context.Panel_Billing_Code_Cptdetail.Add(panelBillingCodeCPTDetail);
                            }
                        }

                        // Save all changes at once
                        context.SaveChanges();

                        // Commit transaction
                        transaction.Commit();

                        // Set success response
                        response.Status = "Successfully added or updated Panel Billing data.";
                    }
                }
            }
            catch (Exception ex)
            {
                response.Status = $"Error: {ex.Message}";
                if (ex.InnerException != null)
                {
                    response.Status += $" Inner Exception: {ex.InnerException.Message}";
                }
            }

            return response;
        }
       
        public ResponseModel getPanelCodeCpt(long practiceCode, long providerCode, long locationCode, string cptCode)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    //var objCPTDetails = ctx.GetPanelCptDetails(practiceCode, providerCode, locationCode, cptCode).ToList();
                    var objCPTDetails = ctx.Database.SqlQuery<GetPanelCptDetails_Result>(
                     "EXEC GetPanelCptDetails @PracticeCode, @ProviderCode, @LocationCode, @CptCode",
                      new SqlParameter("@PracticeCode", practiceCode),
                      new SqlParameter("@ProviderCode", providerCode),
                      new SqlParameter("@LocationCode", locationCode),
                      new SqlParameter("@CptCode", cptCode)
                      ).ToList();
                    if (objCPTDetails.Any())
                    {
                        objResponse.Status = "Success";
                        objResponse.Response = objCPTDetails;
                    }
                    else
                    {
                        objResponse.Status = "No Data Found";
                    }
                }
            }
            catch (Exception ex)
            {
                objResponse.Status = "Error";
                objResponse.Response = ex.Message;
            }
            return objResponse;
        }

        //public ResponseModel getPanelCodeCpt(long practiceCode, long providerCode, long locationCode, string cptCode)
        //{
        //    ResponseModel objResponse = new ResponseModel();
        //    try
        //    {
        //        using (var ctx = new NPMDBEntities())
        //        {
        //            Fetch all CPT details for the given criteria

        //           var objCPTDetail = ctx.GetPanelCptDetails(practiceCode, providerCode, locationCode, cptCode).ToList();

        //            Check if any records are found
        //            if (objCPTDetail.Any())
        //            {
        //                Prioritize rows with null or empty Alternate_Code
        //               var objCPTDetails = objCPTDetail
        //                   .OrderBy(detail => string.IsNullOrEmpty(detail.Alternate_Code) ? 0 : 1) // Prioritize null or empty Alternate_Code
        //                   .FirstOrDefault(); // Pick the first record based on priority

        //                objResponse.Status = "Success";
        //                objResponse.Response = objCPTDetails; // Return only the prioritized record
        //            }
        //            else
        //            {
        //                objResponse.Status = "No Data Found";
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        objResponse.Status = "Error";
        //        objResponse.Response = ex.Message;
        //    }
        //    return objResponse;
        //}



        //panel billing : get panel code details by panel coode in claim
        public ResponseModel getPanelCodeCptClaim(long practiceCode, long providerCode, long locationCode, string panelCode)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                List<GetPanelCodeCPTDetailsClaim_Result> objCPTDetailsClaim = null;
                using (var ctx = new NPMDBEntities())
                {
                    objCPTDetailsClaim = ctx.GetPanelCodeCPTDetailsClaim(practiceCode, providerCode, locationCode,panelCode).ToList();
                }

                if (objCPTDetailsClaim != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = objCPTDetailsClaim;
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

        //public ResponseModel IsAlternateCodeRemoved(string Cpt_Code,string Cpt_Description )
        //{
        //    ResponseModel objResponse = new ResponseModel();
        //    try
        //    {
        //        string Cpt_Code = null;
        //        string Cpt_Description = null;
        //        using (var ctx = new NPMDBEntities())
        //        {
        //            Cpt_Code = ctx.Procedures.Where(p => p.Alternate_Code == AlternateCode).FirstOrDefault()?.ProcedureCode;
        //            Cpt_Description = ctx.Procedures.Where(p => p.Alternate_Code == AlternateCode).FirstOrDefault()?.ProcedureDescription;
        //        }

        //        if (Cpt_Code != null)
        //        {
        //            objResponse.Status = "Sucess";
        //            objResponse.Response = new
        //            {
        //                Cpt_Description = Cpt_Description,
        //                Cpt_Code = Cpt_Code,
        //            };
        //        }
        //        else
        //        {
        //            objResponse.Status = "Incorrect Alternate Code";
        //        }
        //        return objResponse;

        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}
        public ResponseModel IsAlternateCodeRemoved(string Cpt_Code, string Cpt_Description, string AlernateCode)
        {
            ResponseModel objResponse = new ResponseModel();

            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    // Find the matching procedure based on AlternateCode
                    var procedure = ctx.Procedures.FirstOrDefault(p => p.Alternate_Code == AlernateCode);

                    if (procedure != null)
                    {
                        // Check if the provided CptCode matches
                        if (procedure.ProcedureCode == Cpt_Code)
                        {
                            // Check if the provided CptDescription matches
                            if (procedure.ProcedureDescription == Cpt_Description)
                            {
                                objResponse.Status = "Success";
                                objResponse.Response = new
                                {
                                    Cpt_Code = procedure.ProcedureCode,
                                    Cpt_Description = procedure.ProcedureDescription
                                };
                            }
                            else
                            {
                                objResponse.Status = "Error";
                                objResponse.Response = "CPT Description does not match for the provided CPT Code.";
                            }
                        }
                        else
                        {
                            objResponse.Status = "Error";
                            objResponse.Response = "CPT Code does not match for the provided Alternate Code.";
                        }
                    }
                    else
                    {
                        objResponse.Status = "Error";
                        objResponse.Response = "Alternate Code does not match any procedure.";
                    }
                }
            }
            catch (Exception ex)
            {
                objResponse.Status = "Error";
                objResponse.Response = "An error occurred while processing the request.";
                Console.WriteLine(ex.Message); // Replace with proper logging
            }

            return objResponse;
        }

        public ResponseModel GetPanelAlternateCode(long practiceCode, long providerCode, long locationCode, string alternateCode)
        {
            //    ResponseModel responseModel = new ResponseModel();
            //    try
            //    {
            //        using (var ctx = new NPMDBEntities())
            //        {
            //            // Step 1: Filter plans and calculate rank
            //            var rankedPlans = (from plan in ctx.Provider_Cpt_Plan
            //                               join details in ctx.Provider_Cpt_Plan_Details
            //                               on plan.Provider_Cpt_Plan_Id equals details.Provider_Cpt_Plan_Id
            //                               where plan.Practice_Code == practiceCode
            //                                     && plan.Provider_Code == providerCode
            //                                     && plan.Location_Code == locationCode
            //                                     && plan.self_pay == false
            //                                     && details.Alternate_Code == alternateCode // Filter by Alternate Code
            //                               let isActive = (plan.Effective_Date == null || plan.Effective_Date <= DateTimeOffset.Now)
            //                                              && (plan.Expiry_Date == null || plan.Expiry_Date >= DateTimeOffset.Now)
            //                               let rank = plan.Effective_Date != null && plan.Expiry_Date != null ? 1 :
            //                                          plan.Effective_Date != null || plan.Expiry_Date != null ? 2 : 3
            //                               orderby rank, plan.Created_Date descending
            //                               select new
            //                               {
            //                                   plan.Provider_Cpt_Plan_Id,
            //                                   Rank = rank
            //                               }).ToList();

            //            // Step 2: Get the top-ranked plan
            //            var topPlanId = rankedPlans
            //                .Where(p => p.Rank == rankedPlans.Min(r => r.Rank)) // Get the plan with the best rank
            //                .Select(p => p.Provider_Cpt_Plan_Id)
            //                .FirstOrDefault();

            //            if (topPlanId != null)
            //            {
            //                // Step 3: Fetch matching details for the top-ranked plan
            //                var results = (from details in ctx.Provider_Cpt_Plan_Details
            //                               where details.Provider_Cpt_Plan_Id == topPlanId
            //                                     && details.Alternate_Code == alternateCode
            //                               select new
            //                               {
            //                                   details.Provider_Cpt_Plan_Detail_Id,
            //                                   details.Provider_Cpt_Plan_Id,
            //                                   details.Cpt_Code,
            //                                   details.Cpt_Description,
            //                                   details.Alternate_Code,
            //                                   details.Charges
            //                               }).ToList();

            //                if (results.Any())
            //                {
            //                    responseModel.Status = "Success";
            //                    responseModel.Response = results;
            //                }
            //                else
            //                {
            //                    responseModel.Status = "No Data Found";
            //                }
            //            }
            //            else
            //            {
            //                responseModel.Status = "No Data Found";
            //            }
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        responseModel.Status = "Error";
            //        responseModel.Response = ex.Message;
            //    }
            //    return responseModel;
            ResponseModel objResponse = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var objAlternateCodeDetails = ctx.GetPanelALternateCodeDetails(practiceCode, providerCode, locationCode, alternateCode).ToList();

                    if (objAlternateCodeDetails.Any())
                    {
                        objResponse.Status = "Success";
                        objResponse.Response = objAlternateCodeDetails;
                    }
                    else
                    {
                        objResponse.Status = "No Data Found";
                    }
                }
            }
            catch (Exception ex)
            {
                objResponse.Status = "Error";
                objResponse.Response = ex.Message;
            }
            return objResponse;

        }

        //
        //Panel billing - pir ubaid
        public ResponseModel CheckPanelCodeExists(long practiceCode, long providerCode, long locationCode, string PanelCode, long panelBillingCodeId)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {

                    // Query to check if panel code exists excluding the current record
                    bool panelCodeExists = ctx.Panel_Billing_Code_detail
                        .Any(p => p.Practice_Code == practiceCode &&
                                  p.Provider_Code == providerCode &&
                                  p.Location_Code == locationCode &&
                                  p.Panel_Code == PanelCode &&
                                  (panelBillingCodeId == null || p.Panel_Billing_Code_Id != panelBillingCodeId));

                    if (panelCodeExists)
                    {
                        responseModel.Status = "Exists";
                        responseModel.Response = "Entered Panel code already exists. Please enter a unique Panel code.";
                    }
                    else
                    {
                        responseModel.Status = "NotExists";
                        responseModel.Response = "Panel code is unique.";
                    }
                }
            }
            catch (Exception ex)
            {
                responseModel.Status = "Error";
                responseModel.Response = ex.Message;
            }
            return responseModel;
        }

        //

       
        public ResponseModel GetPanelBillingSummaryByPractice(long Practice_Code)
     {
            ResponseModel objResponse = new ResponseModel();

            try
            {
                List<GetPanelBillingSummaryByPractice_Result> panelBillingSummaryList = new List<GetPanelBillingSummaryByPractice_Result>();

                using (var ctx = new NPMDBEntities())
                {
                    // Execute the stored procedure to get the result
                    var result = ctx.GetPanelBillingSummaryByPractice(Practice_Code).ToList();

                    // Map the result to your model class
                    foreach (var item in result)
                    {
                        panelBillingSummaryList.Add(new GetPanelBillingSummaryByPractice_Result
                        {
                            Panel_Billing_Code_Id = item.Panel_Billing_Code_Id,
                            Panel_Code = item.Panel_Code,
                            Provider_Name = item.Provider_Name,
                            Location = item.Location,
                            No_of_CPTs = item.No_of_CPTs,
                            Created_By = item.Created_By, // Assuming Created_By is a BigInt
                            Created_Date = item.Created_Date,
                            Modified_By = item.Modified_By,
                            Modified_Date = item.Modified_Date,
                            Status = item.Status
                        });
                    }
                }

                // Check if any data was returned
                if (panelBillingSummaryList.Any())
                {
                    objResponse.Status = "Success";
                    objResponse.Response = panelBillingSummaryList;
                }
                else
                {
                    objResponse.Status = "No data found";
                    objResponse.Response = null;  // Or you can set an empty list
                }
            }
            catch (Exception ex)
            {
                objResponse.Status = "Error";
                objResponse.Response = ex.Message;
            }

            return objResponse;
        }

        //

        // panel billing - pir ubaid
        public ResponseModel GetPanelCodeDetailsForEdits(long panelBillingCodeId)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    // Query to fetch panel billing code details based on Panel_Billing_Code_Id
                    var results = (from pbd in ctx.Panel_Billing_Code_detail
                                   join pbc in ctx.Panel_Billing_Code_Cptdetail on pbd.Panel_Billing_Code_Id equals pbc.PanelBillingCodeId
                                   where pbd.Panel_Billing_Code_Id == panelBillingCodeId
                                         && pbd.Deleted == false
                                         && pbc.Deleted == false
                                   select new
                                   {
                                       pbd.Panel_Billing_Code_Id,
                                       pbc.Panel_Billing_Code_CPTId,
                                       pbd.Practice_Code,
                                       pbd.Provider_Code,
                                       pbd.Location_Code,
                                       pbd.Panel_Code,
                                       pbc.Created_By,
                                       pbc.Created_Date,
                                       pbc.Modified_By,
                                       pbc.Modified_Date,
                                       pbc.Deleted,
                                       pbc.Cpt_Code,
                                       pbc.Cpt_Description,
                                       pbc.M_1,
                                       pbc.M_2,
                                       pbc.M_3,
                                       pbc.M_4,
                                       pbc.Alternate_Code,
                                       pbc.Charges,
                                       pbc.Units
                                   }).ToList();

                    if (results.Any())
                    {
                        // If results are found, set the response model
                        responseModel.Status = "Success";
                        responseModel.Response = results;
                    }
                    else
                    {
                        // If no results found, return a message indicating no data
                        responseModel.Status = "No Data Found";
                        responseModel.Response = "No data found for the given Panel_Billing_Code_Id.";
                    }
                }
            }
            catch (Exception ex)
            {
                // If an error occurs, capture it and return an error message
                responseModel.Status = "Error";
                responseModel.Response = ex.Message;
            }

            return responseModel;
        }

        //
        public ResponseModel deleteRows(long[] rowIds)
        {
                ResponseModel res = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    // Loop through each rowId to delete the corresponding entry from the Panel_Billing_Code_Cptdetail table
                    foreach (var rowId in rowIds)
                    {
                        // Find the record by its Panel_Billing_Code_CPTId
                        var record = ctx.Panel_Billing_Code_Cptdetail
                                        .FirstOrDefault(x => x.Panel_Billing_Code_CPTId == rowId);

                        if (record != null)
                        {
                            // Delete the record from the table
                            ctx.Panel_Billing_Code_Cptdetail.Remove(record);
                        }
                    }

                    // Save the changes to the database
                    ctx.SaveChanges();

                    res.Status = "Success";
                    res.Response = rowIds;
                }
            }
            catch (Exception ex)
            {
                string errorMessage = "Error: " + ex.Message;
                if (ex.InnerException != null)
                {
                    errorMessage += " Inner Exception: " + ex.InnerException.Message;
                }
                res.Status = errorMessage;
                res.Response = null; // Set the response to null in case of an error.
            }
            return res;
        }

        // panel billing - pir ubaid
        public ResponseModel PanelCodeStatus(int panelBillingCodeId)
        {

            ResponseModel res = new ResponseModel();
            Panel_Billing_Code_Cptdetail csr = new Panel_Billing_Code_Cptdetail();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    if (panelBillingCodeId != null)
                    {
                        var result = ctx.Panel_Billing_Code_Cptdetail.Where(e => e.PanelBillingCodeId == panelBillingCodeId).ToList();
                        foreach (var r in result)
                        {

                            if (r.Deleted == true)
                            {
                                r.Deleted = false;
                            }
                            else if (r.Deleted == null)
                            {
                                r.Deleted = null;
                            }
                            else
                            {
                                r.Deleted = true;
                            }


                        }
                        ctx.SaveChanges();

                        res.Status = "Success";
                        res.Response = result;
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMessage = "Error: " + ex.Message;
                if (ex.InnerException != null)
                {
                    errorMessage += " Inner Exception: " + ex.InnerException.Message;
                }
                res.Status = errorMessage;
                res.Response = null; // Set the response to null in case of an error.
            }
            return res;


        }



        //

        public string GetPatientPicturePath(long patientAccount)
        {
            using (var ctx = new NPMDBEntities())
            {
                return ctx.Patients.Where(p => p.Patient_Account == patientAccount && !(p.Deleted ?? false)).Select(p => p.PicturePath).FirstOrDefault();
            }
        }

        public ResponseModel GetCitiesByZipCode(string ZipCode)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                if (ZipCode == null)
                {
                    ZipCode = "0";
                }
                ZipCode = ZipCode.Length > 5 ? ZipCode.Substring(0, 5) : ZipCode;
                List<CityStateModel> objModel = new List<CityStateModel>();
                using (var ctx = new NPMDBEntities())
                {
                    objModel = ctx.Zip_City_State.Where(c => c.ZIP_Code == ZipCode && (c.Deleted == null || c.Deleted == false)).Select(zcs => new CityStateModel()
                    {
                        CityName = zcs.City_Name.Trim(),
                        State = zcs.State_Code
                    }).ToList();
                }
                objResponse.Status = "Sucess";
                objResponse.Response = objModel;
            }
            catch (Exception)
            {
                throw;
            }
            return objResponse;
        }

        public ResponseModel AddPatientStatementNote(PatientStatementResponse patientStatementResponse, long userId)
        {
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    List<CLAIM_NOTES> cLAIM_NOTEs = new List<CLAIM_NOTES>();
                    patientStatementResponse.ClaimsInfo.ForEach(claimInfo =>
                     {
                         cLAIM_NOTEs.Add(new CLAIM_NOTES
                         {
                             Claim_Notes_Id = Convert.ToInt64(ctx.SP_TableIdGenerator("Claim_Notes_Id").FirstOrDefault().ToString()),
                             Claim_No = claimInfo.ClaimNo,
                             IsAuto_Note = true,
                             Note_Detail = $"Statement has been sent to patient for {claimInfo.AmountDue}",
                             Created_By = userId,
                             Created_Date = DateTime.Now
                         }) ;
                     });
                    ctx.CLAIM_NOTES.AddRange(cLAIM_NOTEs);
                    ctx.SaveChanges();
                    return new ResponseModel { Status = "Success" };
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public ResponseModel GetAllViolated()
        {
            throw new NotImplementedException();
        }

        //public ResponseModel AddToScrubberQueue(ClaimsViewModel claimModel)
        //{
        //    ResponseModel res = new ResponseModel();
        //    using (var ctx = new NPMDBEntities())
        //    {
        //        ScrubberQueue searchedClaim = null;
        //        searchedClaim = ctx.ScrubberQueues.Where(r => r.ClaimNo == claimModel.ClaimModel.Claim_No).FirstOrDefault();
        //        if (searchedClaim == null)
        //        {
        //            //var pat = ctx.Patients.Where(p => p.Patient_Account == claimModel.PatientAccount).Single();
        //            //string patName = pat.Last_Name + pat.First_Name;

        //            //var facility = ctx.Facilities.Where(fac => fac.Facility_Code == claimModel.ClaimModel.Facility_Code).Single();


        //            ctx.insertInScrubberQueue(claimModel.ClaimModel.Claim_No, claimModel.ClaimModel.Patient_Account, DateTime.Now, claimModel.BillingPhysiciansList.FirstOrDefault<SelectListViewModelForProvider>().Id, null, null, null, null);
        //            res.Status = "Success";
        //            res.Response = claimModel;
        //            return res;
        //        }
        //        res.Status = "Claim already present in the scrubber queue";
        //        res.Response = null;
        //        return res;
        //    }
        //}
        public ResponseModel GetAllClaimsInQueue()
        {
            throw new NotImplementedException();
        }

        //..Below methos are created by Tamour Ali 16/08/2023, For Checking if CPT is of Anesthesia Type or Not.
        public bool CheckIfAnesthesiaCpt(string procedureCode)
        {
            if (procedureCode != null && procedureCode.Length == 5 && IsNumeric(procedureCode))
            {
                int codeAsInt = int.Parse(procedureCode);
                if(codeAsInt==01960)
                {
                    //..This condition is for (ANESTH VAGINAL DELIVERY) CPT code validation, as it needs to be skipped for anesthesia.
                    return false;
                }
                return codeAsInt >= 100 && codeAsInt <= 1999;
            }

            return false;
        }
        private bool IsNumeric(string input)
        {
            return int.TryParse(input, out _);
        }


        public List<Packet277CAClaimReason_Messages> Show277CAClaimReasons(string claimNo)
        {

            List<Packet277CAClaimReason_Messages> Claim_Reasons = new List<Packet277CAClaimReason_Messages>();


            string connectionString = ConfigurationManager.ConnectionStrings["NPMEDI277CA"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                // var cmd = new SqlCommand("SP_277CA_LinkingMessage", con); SP_showEDIHistory
                var cmd = new SqlCommand("SP_showEDIHistory_bkp2", con);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandTimeout = 120;
                cmd.Parameters.AddWithValue("@claimNo", claimNo);
                con.Open();

                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    var Claim_Rsn = new Packet277CAClaimReason_Messages()
                    {


                        Practice_Code = rdr["Practice_Code"].ToString(),
                        Status = rdr["Status"].ToString(),
                        Claims = rdr["ClaimNo"].ToString(),
                        Message = rdr["message"] == null ? null : rdr["message"].ToString(),
                        Status_Level = rdr["statusLevel"].ToString(),
                        FileEntry_Date = rdr["FileEntryDate"].ToString(),
                        Status_Date = rdr["StatusDate"] == null ? null : rdr["StatusDate"].ToString(),
                        Submit_Date = rdr["SubmitDate"] == null ? null : rdr["SubmitDate"].ToString(),
                        Insurance_Name = rdr["InsuranceName"].ToString(),
                        DOS = rdr["DOS"].ToString(),

                        Batch_Name = rdr["batchName"].ToString(),
                        Batch_Status = rdr["batchStatus"].ToString(),
                        File277CA = rdr["File277CA"] == null ? "" : rdr["File277CA"].ToString(),
                    };

                    string[] dosDate = Claim_Rsn.DOS.Split(' ');
                    Claim_Rsn.DOS = dosDate[0];

                    string[] statsDate = Claim_Rsn.Status_Date.Split(' ');
                    if (statsDate[0] != null && statsDate[0] != "")
                    {
                        // Convert the string to a DateTime object
                        DateTime date = DateTime.ParseExact(statsDate[0], "yyyyMMdd", CultureInfo.InvariantCulture);
                        // Format the DateTime object to the desired format
                        string formattedDate = date.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                        Claim_Rsn.Status_Date = formattedDate;
                    }
                    else
                    {
                        Claim_Rsn.Status_Date = statsDate[0];
                    }

                    string[] sbmtDate = Claim_Rsn.Submit_Date.Split(' ');
                    Claim_Rsn.Submit_Date = sbmtDate[0];

                    string[] FEDate = Claim_Rsn.FileEntry_Date.Split(' ');
                    Claim_Rsn.FileEntry_Date = FEDate[0];


                    Claim_Reasons.Add(Claim_Rsn);
                }

                //result += cmd.ExecuteNonQuery();
            }
            return Claim_Reasons;
        }
        public ResponseModel GetAdmissionType(long PatientAccount, string claimNo)
        {
            ResponseModel objResponse = new ResponseModel();

            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var results = (from c in ctx.Claims
                                   join ad in ctx.Admission_Details on c.Claim_No equals ad.Claim_No
                                   where ((c.Deleted ?? false) == false && c.Patient_Account == PatientAccount && ad.Type_Of_Admission_Id == 4)
                                   select new
                                   {
                                       ad.Type_Of_Admission_Id,
                                       ad.Claim_No
                                   }).ToList();

                    if (claimNo.ToString() == "0")
                    {
                        if (results.Count > 0)
                        {
                            objResponse.Status = "Success";
                            objResponse.Response = false;
                        }
                        else
                        {
                            objResponse.Status = "Success";
                            objResponse.Response = true;
                        }
                    }
                    else
                    {
                        if (results.Any(r => r.Claim_No.ToString() != claimNo.ToString()))
                        {
                            objResponse.Status = "Success";
                            objResponse.Response = false;
                        }
                        else
                        {
                            objResponse.Status = "Success";
                            objResponse.Response = true;
                        }
                    }
                }
            }
            catch (Exception)
            {
                objResponse.Status = "Error";
                objResponse.Response = false;
            }

            return objResponse;
        }

        public async Task<ResponseModel> AddClaimOverPayment(ClaimOverpayment claimOverpayment, long userId)
        {
            // Initialize the response model
            ResponseModel res = new ResponseModel();

            // Use a single context instance for the entire operation
            using (var ctx = new NPMDBEntities())
            {
             
                    var existingClaimOverpayment = ctx.Claim_Overpayment.FirstOrDefault(x => x.Claim_No == claimOverpayment.Claim_No);

                    try
                    {
                        if (existingClaimOverpayment != null)
                        {
                            // Update the existing Claim_Overpayment entity
                            existingClaimOverpayment.Insurance_over_paid = claimOverpayment.Insurance_over_paid;
                            existingClaimOverpayment.Patient_credit_balance = claimOverpayment.Patient_credit_balance;
                            existingClaimOverpayment.Total_Responsibility = claimOverpayment.Total_Responsibility;
                            existingClaimOverpayment.Created_By = userId; // Assuming you want to update Created_By as well
                            existingClaimOverpayment.Created_Date = DateTimeOffset.Now; // Assuming you want to update Created_Date as well
                            existingClaimOverpayment.Deleted = false; // Assuming you want to reset Deleted status

                            // Save the changes asynchronously
                            await ctx.SaveChangesAsync();

                            // Set the response to indicate success
                            res.Status = "Success";
                            res.Response = "Claim overpayment updated successfully.";
                        }
                        else
                        {
                            // Create a new Claim_Overpayment instance if it doesn't exist
                            Claim_Overpayment claimoverpay = new Claim_Overpayment
                            {
                                Claim_No = claimOverpayment.Claim_No,
                                Insurance_over_paid = claimOverpayment.Insurance_over_paid,
                                Patient_credit_balance = claimOverpayment.Patient_credit_balance,
                                Total_Responsibility = claimOverpayment.Total_Responsibility,
                                Created_By = userId,
                                Created_Date = DateTimeOffset.Now,
                                Deleted = false
                            };

                            // Add the new Claim_Overpayment entity to the database context
                            ctx.Claim_Overpayment.Add(claimoverpay);

                            // Save the changes asynchronously
                            await ctx.SaveChangesAsync();

                            // Set the response to indicate success
                            res.Status = "Success";
                            res.Response = "Claim overpayment added successfully.";
                        }
                    }
                    catch (Exception ex)
                    {
                        // Handle any errors
                        res.Status = "Failed";
                        res.Response = $"Error processing claim overpayment: {ex.Message}";
                    }
                
                
            }

            return res;
        }

        public async Task<ResponseModel> GetAllClaimOverPayment(long claimNo)
        {
            ResponseModel objResponse = new ResponseModel();
            using (var ctx = new NPMDBEntities()) {
                var claimOverPayments = await ctx.Claim_Overpayment
                                                 .Where(c => c.Claim_No == claimNo && c.Deleted != true)
                                                 .ToListAsync();

                if (claimOverPayments.Any())
                {
                    objResponse.Status = "Success";
                    objResponse.Response = claimOverPayments;  // Returning the data as a list of ClaimOverpayment objects
                }
                else
                {
                    objResponse.Status = "No Data Found";
                    objResponse.Response = null;
                }
            }
                // Query the Claim_Overpayment table for the claim with the provided claimNo
           

            return objResponse;
        }

        public async Task<ResponseModel> GetStateList()
        {
            ResponseModel objResponse = new ResponseModel();

            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var stateList = await ctx.Zip_City_State
                     .Where(s => s.Deleted == false && s.State_Code != null&& s.State_Code!="") // Exclude deleted records
                     .Select(s => new { s.State_Code }) // Replace with your actual state name column
                     .Distinct()
                     .ToListAsync();


                    if (stateList.Any())
                    {
                        objResponse.Status = "Success";
                        objResponse.Response = stateList; // Returning the distinct state codes and names
                    }
                    else
                    {
                        objResponse.Status = "No Data Found";
                        objResponse.Response = null;
                    }
                }
            }
            catch (Exception ex)
            {
                objResponse.Status = "Error";
                objResponse.Response = ex.Message; // You can log the exception for better debugging
            }

            return objResponse;
        }
        //added by pir ubaid - search insurance from provider payers - payer mapping functionality

        //public ResponseModel ProviderPayerSearchInsurance([FromBody] InsuranceSearchViewModel model)
        //{
        //    ResponseModel objResponse = new ResponseModel();
        //    List<InsuranceSearchViewModel> objInsuranceList = null;
        //    using (var ctx = new NPMDBEntities())
        //    {
        //        List<ProviderPayerInsuranceSearchTest_Result> searchResult = ctx.ProviderPayerInsuranceSearchTest(model.PracticeCode, model.PayerId, model.PayerDescription, model.NameId, model.InsuranceName, model.InsuranceId, model.GroupName, model.ZIP, model.InsuranceAddress, model.State, model.City, model.SearchFrom).ToList();

        //        if (searchResult != null)
        //        {
        //            objResponse.Status = "success";
        //            objResponse.Response = searchResult;
        //        }
        //        else
        //        {
        //            objResponse.Status = "Error";
        //        }
        //        return objResponse;
        //    }
        //}

        //--

        //public ResponseModel ProviderPayerSearchInsurance([FromBody] InsuranceSearchViewModel model)
        //{
        //    ResponseModel objResponse = new ResponseModel();
        //    using (var ctx = new NPMDBEntities())
        //    {
        //        //var searchResult = ctx.ProviderPayerInsuranceSearchTest(
        //        //    model.PracticeCode,
        //        //    model.PayerId,
        //        //    model.PayerDescription,
        //        //    model.NameId,
        //        //    model.InsuranceName,
        //        //    model.InsuranceId,
        //        //    model.GroupName,
        //        //    model.ZIP,
        //        //    model.InsuranceAddress,
        //        //    model.State,
        //        //    model.City,
        //        //    model.SearchFrom
        //        //).ToList();
        //   List<PayerMappedInsuranceSearch_Result> searchResult = ctx.PayerMappedInsuranceSearch(model.PracticeCode, model.PayerId, model.PayerDescription, model.NameId, model.InsuranceName, model.InsuranceId, model.GroupName, model.ZIP, model.InsuranceAddress, model.State, model.City, model.SearchFrom).ToList();

        //        if (searchResult != null)
        //        {
        //            if (searchResult.Any(x => x.Result == 0))
        //            {
        //                // Handle "no mapped insurance" case
        //                objResponse.Status = "No mapped";
        //                objResponse.Response = "This insurance is not mapped, Please map this insurance first.";
        //            }
        //            else if (searchResult.Any(x => x.Result == 1))
        //            {
        //                // Return actual data
        //                objResponse.Status = "success";
        //                objResponse.Response = searchResult;
        //            }

        //        }
        //        else
        //        {
        //            objResponse.Status = "Error";

        //        }

        //        return objResponse;
        //    }
        //}
        //--

        public ResponseModel ProviderPayerSearchInsurance([FromBody] InsuranceSearchViewModel model)
        {
            ResponseModel objResponse = new ResponseModel();
            using (var ctx = new NPMDBEntities())
            {
                try
                {
                    List<PayerMappedInsuranceSearch_Result> searchResult = ctx.PayerMappedInsuranceSearch(
                   model.PracticeCode,
                   model.PayerId,
                   model.PayerDescription,
                   model.NameId,
                   model.InsuranceName,
                   model.InsuranceId,
                   model.GroupName,
                   model.ZIP,
                   model.InsuranceAddress,
                   model.State,
                   model.City,
                   model.SearchFrom
                    ).ToList();

                    if (searchResult.Any())
                    {
                        if (searchResult.Any(x => x.Result == 0))
                        {
                            // Handle "no mapped insurance" case
                            objResponse.Status = "No mapped";
                            objResponse.Response = "This insurance is not mapped. Please map this insurance first.";
                        }
                        else
                        {
                            // Return actual data (Result = 1)
                            objResponse.Status = "success";
                            objResponse.Response = searchResult;
                        }
                    }
                    else
                    {
                        // No data found (empty result)
                        objResponse.Status = "success";
                        objResponse.Response = new List<PayerMappedInsuranceSearch_Result>(); // Empty list
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                
               

                return objResponse;
            }
        }

        //
        public ResponseModel GetClaimsWithPatientDue(long practicecode, long patientaccount)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    // Use parentheses to ensure correct logical grouping
                    var claims = ctx.Claims
                        .Where(c =>
                            (c.Pat_Status == "N" || c.Pat_Status == "B" || c.Pat_Status == "R") &&
                            c.Amt_Due > 0 &&
                            c.practice_code == practicecode &&
                            c.Patient_Account == patientaccount&&
                            c.Deleted==false
                        )
                        .Select(c => new
                        {
                            Claim_No = c.Claim_No,
                            Dos = c.DOS,
                            Amt_Due = c.Amt_Due
                        })
                        .ToList();

                    if (claims.Any())
                    {
                        objResponse.Status = "Success";
                        objResponse.Response = claims;
                    }
                    else
                    {
                        objResponse.Status = "No Claims Found";
                        objResponse.Response = null;
                    }
                }
            }
            catch (Exception ex)
            {
                objResponse.Status = "Error";
                objResponse.Response = ex.Message;
            }

            return objResponse;
        }


    }
}