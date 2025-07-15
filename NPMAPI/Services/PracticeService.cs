using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using NPMAPI.Enums;
using NPMAPI.Models;
using NPMAPI.Models.InboxHealth;
using NPMAPI.Models.ViewModels;
using NPMAPI.Repositories;
namespace NPMAPI.Services
{
    public class PracticeService : IPracticeRepository
    {
        private readonly IDeltaSyncRepository _deltaSyncRepository;
        public PracticeService()
        {
        }
        public PracticeService(IDeltaSyncRepository deltaSyncRepository)
        {
            _deltaSyncRepository = deltaSyncRepository;
        }
        #region Practice
        public ResponseModel GetPractices()
        {
            ResponseModel objResponse = new ResponseModel();
            List<Practice> practices = null;
            using (var ctx = new NPMDBEntities())
            {
                //practices = ctx.Practices.Where(p => p.Deleted == null || p.Deleted == false).ToList();
                //practices = ctx.Practices
                //            .Select(s => new Practice()
                //            {
                //                Practice_Code = s.Practice_Code,
                //                Prac_Name = s.Prac_Name
                //            }).ToList();
                practices = ctx.Practices.ToList();
            }
            if (practices != null)
            {
                objResponse.Status = "Sucess";
                objResponse.Response = practices;
            }
            else
            {
                objResponse.Status = "No Data Found";
            }
            return objResponse;
        }

        public ResponseModel GetPracticeModel()
        {
            ResponseModel objResponse = new ResponseModel();
            PracticeViewModel practiceViewModel = new PracticeViewModel();
            using (var ctx = new NPMDBEntities())
            {
                practiceViewModel.PracticeTypeComboFillingList = ctx.Practice_Types.ToList();
                practiceViewModel.SpecialityGroupsList = ctx.Specialty_Groups.ToList();
                //practiceViewModel.CategoryList = ctx.SpecialInstructionCateogry
            }
            if (practiceViewModel != null)
            {
                objResponse.Status = "Sucess";
                objResponse.Response = practiceViewModel;
            }
            else
            {
                objResponse.Status = "No Data Found";
            }
            return objResponse;
        }
        public ResponseModel GetPractice(long? practiceId)
        {
            ResponseModel objResponse = new ResponseModel();
            //PracticeViewModel practice = null;
            PracticeViewModel practiceViewModel = new PracticeViewModel();
            using (var ctx = new NPMDBEntities())
            {
                practiceViewModel.PracticeModel = ctx.Practices.FirstOrDefault(p => p.Practice_Code == practiceId);
                if (practiceViewModel.PracticeModel != null && !string.IsNullOrEmpty(practiceViewModel.PracticeModel.TAXONOMY_CODE))
                {
                    practiceViewModel.PracticeVendors = ctx.PracticeVendors.Where(p => p.PracticeId == practiceViewModel.PracticeModel.Practice_Code)?.ToList();
                    var specialityCategory = ctx.Specialty_Category.Where(c => c.TAXONOMY_CODE == practiceViewModel.PracticeModel.TAXONOMY_CODE).FirstOrDefault();
                    if (specialityCategory != null)
                    {
                        var group = ctx.Specialty_Groups.Where(t => t.GROUP_NO == specialityCategory.GROUP_NO).FirstOrDefault();
                        if (group != null)
                        {
                            practiceViewModel.PracticeModel.GroupNo = group.GROUP_NO;
                        }
                    }
                }
                practiceViewModel.Specializations = ctx.Specializations.ToList();
                practiceViewModel.SpecialityGroupsList = ctx.Specialty_Groups.ToList();
                practiceViewModel.ProvidersList = ctx.Providers.ToList();
                practiceViewModel.PracticeFacilities = ctx.GetPracticeFacilites(practiceId).ToList();
                practiceViewModel.PracticeLocations = ctx.Practice_Locations.Where(pf => pf.Deleted == null || pf.Deleted == false).Select(pl => new PracticeLocationViewModel()
                {
                    City = pl.Location_City,
                    CLIANum = pl.Clia_Number,
                    LocationCode = pl.Location_Code,
                    LocationName = pl.Location_Name,
                    Phone = pl.Phone_one,
                    PracticeLocationCode = pl.Practice_Code
                }).ToList();
                practiceViewModel.ProvidersComboFillingList = ctx.Providers.Where(p => p.Deleted == null || p.Deleted == false).Select(svm => new SelectListViewModel { Id = svm.Provider_Code, Name = svm.Provid_FName + " " + svm.Provid_LName }).ToList();
                practiceViewModel.LocationComboFillingList = ctx.Practice_Locations.Where(p => p.Deleted == null || p.Deleted == false).Select(svm => new SelectListViewModel { Id = svm.Location_Code, Name = svm.Location_Name }).ToList();
                practiceViewModel.PracticeTypeComboFillingList = ctx.Practice_Types.ToList();
                // practiceViewModel.LocationComboFillingList = ctx.Practice_Locations.Where(p => p.Deleted == null || p.Deleted == false).Select(svm => new SelectListViewModel { Id = svm.Location_Code, Name = svm.Location_Name }).ToList();
                practiceViewModel.WCBRatingList = ctx.WCBRatings.ToList();
                //practiceViewModel.CategoryList = ctx.SpecialInstructionCateogry
            }
            if (practiceViewModel != null)
            {
                objResponse.Status = "Sucess";
                objResponse.Response = practiceViewModel;
            }
            else
            {
                objResponse.Status = "No Data Found";
            }
            return objResponse;
        }
        public ResponseModel ActivateInActivePractice(long practiceId, bool isActive)
        {
            ResponseModel objResponse = new ResponseModel();
            Practice practice = null;
            List<Practice> practicelist = null;
            using (var ctx = new NPMDBEntities())
            {
                practice = ctx.Practices.Where(s => s.Practice_Code == practiceId).FirstOrDefault<Practice>();
                if (practice != null)
                {
                    practice.Is_Active = isActive;
                    ctx.SaveChanges();
                }
                practicelist = ctx.Practices.ToList();
            }
            if (practicelist != null)
            {
                objResponse.Status = "Sucess";
                objResponse.Response = practicelist;
            }
            else
            {
                objResponse.Status = "No Data Found";
            }
            return objResponse;
        }

        public ResponseModel PostSavePractice(Practice model, long userId)
        {
            long result = 0;
            //added by pir ubaid - mappd payer population
            using (var ct = new NPMDBEntities())
            using (var transaction = ct.Database.BeginTransaction()) // Declare transaction inside using

                try
                {
                ResponseModel objResponse = new ResponseModel();
                if (model.Practice_Code == 0)
                {
                    using (var ctx = new NPMDBEntities())
                    {
                        if (model.EFS == true)
                        {
                            model.EFSDate = DateTime.Now;
                        }
                        long practiceID = Convert.ToInt64(ctx.SP_TableIdGenerator("Practice_Code").FirstOrDefault().ToString());
                        model.Practice_Code = practiceID;
                        model.Created_Date = DateTime.Now;
                        model.Created_By = userId;
                        var entry = ctx.Practices.Add(model);
                        ctx.SaveChanges();
                        result = entry?.Practice_Code == null ? 0 : entry.Practice_Code;
                        //AssignNewPracticeToAdmins(practiceID, userId);
                    }
                }
                else
                {
                    using (var ctx = new NPMDBEntities())
                    {
                        Practice practice = ctx.Practices.Where(s => s.Practice_Code == model.Practice_Code).FirstOrDefault<Practice>();
                        if (practice != null)
                        {
                                //added by pir ubaid - mappd payer population
                                // Detect changes
                                bool isNPIChanged = practice.NPI != model.NPI;
                                bool isCodeOneChanged = practice.TAXONOMY_CODE != model.TAXONOMY_CODE;

                                //
                                practice.Prac_Name = model.Prac_Name;
                            practice.Commencement_Date = model.Commencement_Date;
                            practice.Contact_Person_Phone = model.Contact_Person_Phone;
                            practice.Email_Address = model.Email_Address;
                            practice.Email_Contact_Person = model.Email_Contact_Person;
                            practice.Invocie_Fax_Type = model.Invocie_Fax_Type;
                            practice.Invoice_Email_Address1 = model.Invoice_Email_Address1;
                            practice.Invoice_Email_Address2 = model.Invoice_Email_Address2;
                            practice.Invoice_Email_Address3 = model.Invoice_Email_Address3;
                            practice.Invoice_Fax = model.Invoice_Fax;
                            practice.Billing_Type = model.Billing_Type;
                            practice.Invoice_Practice_Name = model.Invoice_Practice_Name;
                            practice.Invoice_Prac_Address = model.Invoice_Prac_Address;
                            practice.Invoice_Prac_City = model.Invoice_Prac_City;
                            practice.Invoice_Prac_State = model.Invoice_Prac_State;
                            practice.Invoice_Prac_Zip = model.Invoice_Prac_Zip;
                            practice.Office_Manager = model.Office_Manager;
                            practice.Location_Number = model.Location_Number;
                            practice.Mailing_Address = model.Mailing_Address;
                            practice.Mailing_City = model.Mailing_City;
                            practice.Mailing_State = model.Mailing_State;
                            practice.Mailing_Zip = model.Mailing_Zip;
                            practice.Modified_Date = DateTime.Now;
                            practice.NPI = model.NPI;
                            practice.Noshow_Charges = model.Noshow_Charges;
                            practice.Prac_Address = model.Prac_Address;
                            practice.Prac_Address_Line2 = model.Prac_Address_Line2;
                            practice.Prac_Alternate_Phone = model.Prac_Alternate_Phone;
                            practice.Prac_Category = model.Prac_Category;
                            practice.Prac_City = model.Prac_City;
                            practice.PRAC_License_Number = model.PRAC_License_Number;
                            practice.Prac_Name = model.Prac_Name;
                            practice.Prac_Phone = model.Prac_Phone;
                            practice.Prac_Phone_Ext = model.Prac_Phone_Ext;
                            practice.Prac_State = model.Prac_State;
                            practice.Prac_Tax_Id = model.Prac_Tax_Id;
                            practice.Prac_Type = model.Prac_Type;
                            practice.Prac_URL = model.Prac_URL;
                            practice.Prac_Zip = model.Prac_Zip;
                            practice.TAXONOMY_CODE = model.TAXONOMY_CODE;
                            practice.Termination_Date = model.Termination_Date;
                            practice.Termination_Notice = model.Termination_Notice;
                            practice.Termination_Notice_Date = model.Termination_Notice_Date;
                            practice.EFS = model.EFS;
                            practice.isAutofillClaimDetails = model.isAutofillClaimDetails;
                            practice.Prac_Soft = model.Prac_Soft;
                            if (model.EFS == true)
                            {
                                practice.EFSDate = DateTime.Now;
                            }
                            else
                            {
                                practice.EFSDate = null;
                            }
                            practice.Billing_Percentage = model.Billing_Percentage;
                            practice.Practice_Pat_Bill_Name = model.Practice_Pat_Bill_Name;
                            practice.prac_doing_business = model.prac_doing_business;
                            ctx.SaveChanges();
                                //added by pir ubaid for updating the values in tables-payer mapping population
                                // Batch update Provider and Provider_Payers table if NPI changes
                                if (isNPIChanged)
                                {
                                    ctx.Database.ExecuteSqlCommand(
                                        "UPDATE Providers SET group_npi = {0} WHERE Practice_Code = {1}", model.NPI, model.Practice_Code);

                                    ctx.Database.ExecuteSqlCommand(
                                        "UPDATE Provider_Payers SET Group_NPI = {0} WHERE Practice_code = {1}", model.NPI, model.Practice_Code);
                                }

                                // Batch update Provider table if TAXONOMY_CODE changes
                                if (isCodeOneChanged)
                                {
                                    ctx.Database.ExecuteSqlCommand(
                                        "UPDATE Providers SET grp_taxonomy_id = {0} WHERE Practice_Code = {1}", model.TAXONOMY_CODE, model.Practice_Code);
                                }

                                //
                                //Check if the pratice was synced with InboxHealth
                                var syncedPractice = _deltaSyncRepository.GetSyncedPractice(model.Practice_Code);
                            if (syncedPractice != null)
    {
                                syncedPractice.UpdatedDate = DateTime.Now;
                                ctx.SaveChanges();
                                //Update the Enterprise in Inbox Health if the Practice is Synced 
                                UpdateSyncedPractice(model.Practice_Code, syncedPractice.GeneratedId);
                            }
                            result = practice.Practice_Code;
                        }
                    }
                }
                if (result > 0)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = result;
                        transaction.Commit(); // Commit if all operations succeed

                    }
                    else
                {
                    objResponse.Status = "Error";
                        transaction.Rollback(); // Rollback if operation fails

                    }
                    return objResponse;
            }
            catch (Exception ex)
            {
                    transaction.Rollback(); // Rollback on exception
                    throw ex;
            }
        }
        private void UpdateSyncedPractice(long practice_Code, long generatedId)
        {
            PracticeUpdateRequest practiceUpdateRequest;
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var practice = ctx.Practices.Where(s => s.Practice_Code == practice_Code).FirstOrDefault();
                    var SyncExcludedPractices = ctx.SyncExcludedPractices.Where(s => s.PracticeCode == practice_Code).FirstOrDefault();
                    practiceUpdateRequest = new PracticeUpdateRequest()
                    {
                        id = generatedId,
                        enterprise = new Enterprise()
                        {
                            name = practice.Prac_Name,
                            city = practice.Prac_City,
                            state = practice.Prac_State,
                            address_line_1 = practice.Prac_Address,
                            address_line_2 = practice.Prac_Address_Line2,
                            zip = practice.Prac_Zip,
                            support_phone_number = practice.Prac_Phone,
                            sales_tax = Convert.ToInt32(practice.Prac_Tax_Id),
                            default_quick_pay_description = "Copay",
                            statement_descriptor = "Nobility RCM",
                            time_zone = "Eastern Time (US & Canada)",
                            color_statements = true,
                            first_class = true,
                            return_envelope = true,
                            perforation = true,
                            //minimum_bill_amount_cents = 50
                        }
                    };
                    if (SyncExcludedPractices !=null && SyncExcludedPractices.PracticeCode == practice_Code && SyncExcludedPractices.ReSync == false && SyncExcludedPractices.IsDeleted == false) //.. This condition is added by Hamza Akhlaq for excluding hatel patel profile info from syncing
                    {
                        var update = _deltaSyncRepository.UpdatePractice(practiceUpdateRequest);
                        var practiceSyncId = ctx.PracticeSynchronizations.Where(x => x.PracticeId == practice_Code).FirstOrDefault().PracticeSyncId;
                        if (update.IsSuccessful)
                        {
                            ctx.PracticeSynchronizationLogs.Add(new PracticeSynchronizationLog()
                            {
                                PracticeSyncId = practiceSyncId,
                                LogMessage = "Practice with Id " + generatedId + " has been successfully updated.",
                                LogTime = DateTime.Now
                            });
                            ctx.SaveChanges();
                        }
                        else
                        {
                            ctx.PracticeSynchronizationLogs.Add(new PracticeSynchronizationLog()
                            {
                                PracticeSyncId = practiceSyncId,
                                LogMessage = "Practice with Id " + generatedId + " has failed to updated.",
                                LogTime = DateTime.Now
                            });
                            ctx.SaveChanges();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public ResponseModel GetPracticeSelectList(string searchText = "")
        {
            ResponseModel objResponse = new ResponseModel();
            List<SelectListViewModel> selectListViewModel = null;
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    if (!string.IsNullOrEmpty(searchText))
                    {
                        selectListViewModel = ctx.Practices.Where(t => (t.Practice_Code.ToString().Contains(searchText) || t.Prac_Name.Contains(searchText)) && (t.Deleted ?? false) == false).Select(x => new SelectListViewModel()
                        {
                            Id = x.Practice_Code,
                            Name = x.Practice_Code + "|" + x.Prac_Name
                        }).OrderBy(t => t.Name).ToList();
                    }
                    else
                    {
                        selectListViewModel = ctx.Practices.Where(t => (t.Deleted ?? false) == false).Select(x => new SelectListViewModel()
                        {
                            Id = x.Practice_Code,
                            Name = x.Practice_Code + "|" + x.Prac_Name
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
                }
            }
            catch (Exception)
            {
                throw;
            }
            return objResponse;
        }
        public PracticeFTPViewModel GetPracticeFTPInfo(long practiceCode, FTPType type)
        {
            try
            {
                var responseModel = new ResponseModel();
                using (var ctx = new NPMDBEntities())
                {
                    var practiceInfo = ctx.Practices.Where(p => p.Practice_Code == practiceCode).FirstOrDefault();
                    if (practiceInfo != null)
                    {
                        string[] ftpInfo = null;
                        string destination = "";
                        switch (type)
                        {
                            case FTPType.EDI:
                                {
                                    if (practiceInfo.FTP_ENABLE == true && !string.IsNullOrEmpty(practiceInfo.FTP_Path))
                                    {
                                        ftpInfo = practiceInfo.FTP_Path.Split('|');
                                        destination = ConfigurationManager.AppSettings["FTPDestination"];
                                    }
                                    break;
                                }
                            case FTPType.PatientStatement:
                                {
                                    if (practiceInfo.FTP_ENABLE == true && !string.IsNullOrEmpty(practiceInfo.FTP_Path_Patient_Statements))
                                    {
                                        ftpInfo = practiceInfo.FTP_Path_Patient_Statements.Split('|');
                                        destination = "";
                                    }
                                    break;
                                }
                            default:
                                break;
                        }
                        if (ftpInfo != null && ftpInfo.Length == 3)
                        {
                            return new PracticeFTPViewModel()
                            {
                                Username = ftpInfo[0],
                                Password = ftpInfo[1],
                                Host = ftpInfo[2],
                                Destination = destination,
                                Port = 22,
                                PracticeCode = practiceInfo.Practice_Code
                            };
                        }
                    }
                    return null;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        public PracticeFTPViewModel GetBatchCSIPracticeFTPInfo(long practiceCode, FTPType type)
        {
            try
            {
                var responseModel = new ResponseModel();
                using (var ctx = new NPMDBEntities())
                {
                    var practiceInfo = ctx.Practices.Where(p => p.Practice_Code == practiceCode).FirstOrDefault();
                    if (practiceInfo != null)
                    {
                        string[] ftpInfo = null;
                        string destination = "";
                        switch (type)
                        {
                            case FTPType.EDI:
                                {
                                    if (practiceInfo.FTP_ENABLE == true && !string.IsNullOrEmpty(practiceInfo.FTP_Path))
                                    {
                                        ftpInfo = practiceInfo.FTP_Path.Split('|');
                                        destination = ConfigurationManager.AppSettings["CSIFTPDestination"];
                                    }
                                    break;
                                }
                            case FTPType.PatientStatement:
                                {
                                    if (practiceInfo.FTP_ENABLE == true && !string.IsNullOrEmpty(practiceInfo.FTP_Path_Patient_Statements))
                                    {
                                        ftpInfo = practiceInfo.FTP_Path_Patient_Statements.Split('|');
                                        destination = "";
                                    }
                                    break;
                                }
                            default:
                                break;
                        }
                        if (ftpInfo != null && ftpInfo.Length == 3)
                        {
                            return new PracticeFTPViewModel()
                            {
                                Username = ftpInfo[0],
                                Password = ftpInfo[1],
                                Host = ftpInfo[2],
                                Destination = destination,
                                Port = 22,
                                PracticeCode = practiceInfo.Practice_Code
                            };
                        }
                    }
                    return null;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<List<PracticeFTPViewModel>> GetFTPEnabledPractices(long? practiceCode)
        {
            try
            {
                var responseModel = new ResponseModel();
                using (var ctx = new NPMDBEntities())
                {
                    var practices = await ctx.Practices.Where(p => !(p.Deleted ?? false) && (p.Is_Active ?? false) && p.Practice_Code == practiceCode && p.FTP_ENABLE == true && !string.IsNullOrEmpty(p.FTP_Path)).ToListAsync();
                    List<PracticeFTPViewModel> ftps = new List<PracticeFTPViewModel>();
                    foreach (var practice in practices)
                    {
                        string[] ftpInfo = practice.FTP_Path.Split('|');
                        if (ftpInfo.Length == 3)
                        {
                            ftps.Add(new PracticeFTPViewModel()
                            {
                                Username = ftpInfo[0],
                                Password = ftpInfo[1],
                                Host = ftpInfo[2],
                                Destination = ConfigurationManager.AppSettings["ERASource"],
                                Port = 22,
                                PracticeCode = practice.Practice_Code
                            });
                        }
                    }
                    return ftps;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        //public List<PracticeFTPViewModel> GetFTPEnabledPractices()
        //{
        //    try
        //    {
        //        var responseModel = new ResponseModel();
        //        using (var ctx = new NPMDBEntities())
        //        {
        //            var practices = ctx.Practices.Where(p => !(p.Deleted ?? false) && (p.Is_Active ?? false) && p.FTP_ENABLE == true && !string.IsNullOrEmpty(p.FTP_Path)).ToList();
        //            List<PracticeFTPViewModel> ftps = new List<PracticeFTPViewModel>();
        //            foreach (var practice in practices)
        //            {
        //                string[] ftpInfo = practice.FTP_Path.Split('|');
        //                if (ftpInfo.Length == 3)
        //                {
        //                    ftps.Add(new PracticeFTPViewModel()
        //                    {
        //                        Username = ftpInfo[0],
        //                        Password = ftpInfo[1],
        //                        Host = ftpInfo[2],
        //                        Destination = ConfigurationManager.AppSettings["ERASource"],
        //                        Port = 22,
        //                        PracticeCode = practice.Practice_Code
        //                    });
        //                }
        //            }
        //            return ftps;
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}
        #endregion Practice
        #region PracticeNotes
        public ResponseModel GetPracticeNotesList(long PracticeId)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                List<Practice_Notes> PracticeNotesList = null;
                using (var ctx = new NPMDBEntities())
                {
                    PracticeNotesList = ctx.Practice_Notes.Where(c => c.PRACTICE_Code == PracticeId && (c.Deleted == null || c.Deleted == false)).ToList();
                }
                if (PracticeNotesList != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = PracticeNotesList;
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
        public ResponseModel GetPracticeNote(long PracticeNotesId)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                Practice_Notes PracticeNote = null;
                using (var ctx = new NPMDBEntities())
                {
                    PracticeNote = ctx.Practice_Notes.SingleOrDefault(c => c.Practice_Notes_Id == PracticeNotesId && (c.Deleted == null || c.Deleted == false));
                }
                if (PracticeNote != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = PracticeNote;
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
        [HttpPost]
        public ResponseModel SavePracticeNote([FromBody] Practice_Notes PracticeNote)
        {
            ResponseModel objResponse = new ResponseModel();
            Practice_Notes objNewPracticeNote = null;
            using (var ctx = new NPMDBEntities())
            {
                if (PracticeNote.Practice_Notes_Id != 0)
                {
                    objNewPracticeNote = ctx.Practice_Notes.SingleOrDefault(p => p.Practice_Notes_Id == PracticeNote.Practice_Notes_Id);
                    if (objNewPracticeNote != null)
                    {
                        objNewPracticeNote.NOTE_CONTENT = PracticeNote.NOTE_CONTENT;
                        objNewPracticeNote.MODIFIED_DATE = DateTime.Now;
                        ctx.SaveChanges();
                    }
                }
                else
                {
                    objNewPracticeNote = new Practice_Notes();
                    long pracNotesid = Convert.ToInt64(ctx.SP_TableIdGenerator("Practice_Notes_Id").FirstOrDefault().ToString());  //ctx.Practice_Notes.Max(p => p.Practice_Notes_Id);    //
                    objNewPracticeNote = PracticeNote;
                    objNewPracticeNote.Practice_Notes_Id = pracNotesid;
                    ctx.Practice_Notes.Add(objNewPracticeNote);
                    ctx.SaveChanges();
                }
                if (PracticeNote != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = PracticeNote.Practice_Notes_Id;
                }
                else
                {
                    objResponse.Status = "Error";
                }
            }
            return objResponse;
        }
        [HttpGet]
        public ResponseModel DeletePracticetNote(long PracticeId, long PracticeNotesId)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                List<Practice_Notes> PracticeNotesList = null;
                using (var ctx = new NPMDBEntities())
                {
                    Practice_Notes objPatientNote = ctx.Practice_Notes.SingleOrDefault(c => c.Practice_Notes_Id == PracticeNotesId && (c.Deleted == null || c.Deleted == false));
                    if (objPatientNote != null)
                    {
                        objPatientNote.Deleted = true;
                        ctx.SaveChanges();
                    }
                    PracticeNotesList = ctx.Practice_Notes.Where(c => c.PRACTICE_Code == PracticeId && (c.Deleted == null || c.Deleted == false)).ToList();
                    if (PracticeNotesList != null)
                    {
                        objResponse.Status = "Sucess";
                        objResponse.Response = PracticeNotesList;
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
        #endregion PracticeNotes
        #region PracticeResources
        public ResponseModel GetPracticeResources(long practiceId, long LocationId)
        {
            ResponseModel objResponse = new ResponseModel();
            Insurance_Payers InsPayer = null;
            using (var ctx = new NPMDBEntities())
            {
                //InsPayer = ctx.Insurance_Payers.Where(p => p == p.)
                //            .Select(s => new Insurance_Payers()
                //            {
                //                Practice_Code = s.in,
                //                Prac_Name = s.Prac_Name
                //            }).SingleOrDefault<Insurance_Payers>();
            }
            if (InsPayer != null)
            {
                objResponse.Status = "Sucess";
                objResponse.Response = InsPayer;
            }
            else
            {
                objResponse.Status = "No Data Found";
            }
            return objResponse;
        }
        #endregion PracticeResources
        #region PracticeSpecialInstruction
        public ResponseModel GetSpecialInstructionList(long PracticeId)
        {
            ResponseModel objResponse = new ResponseModel();
            SpecialInstructionViewModel specialInstructionModel = new SpecialInstructionViewModel();
            using (var ctx = new NPMDBEntities())
            {
                specialInstructionModel.specialInstructionModel = ctx.Practice_Special_Instruction_Answers.Where(c => c.Practice_Code == PracticeId).ToList();
                specialInstructionModel.CategoryList = ctx.Practice_Special_Instruction_Category.Where(p => p.Deleted == null || p.Deleted == false).Select(svm => new SelectListViewModel { Id = svm.Category_Id, Name = svm.Category_Desc }).ToList();
            }
            if (specialInstructionModel != null)
            {
                objResponse.Status = "Sucess";
                objResponse.Response = specialInstructionModel;
            }
            else
            {
                objResponse.Status = "Error";
            }
            return objResponse;
        }
        public ResponseModel GetSpecialInstruction(long QuestionId, long PracticeId)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                Practice_Special_Instruction_Answers PracticeSpecialInstruction = null;
                using (var ctx = new NPMDBEntities())
                {
                    PracticeSpecialInstruction = ctx.Practice_Special_Instruction_Answers.SingleOrDefault(c => c.Practice_Code == PracticeId && c.Question_Id == QuestionId && (c.Deleted == null || c.Deleted == false));
                }
                if (PracticeSpecialInstruction != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = PracticeSpecialInstruction;
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
        public ResponseModel GetSpecialInstructionQuestionByCategory(long CategoryId)
        {
            ResponseModel objResponse = new ResponseModel();
            List<SelectListViewModel> specialInstructionQuestionModel = new List<SelectListViewModel>();
            using (var ctx = new NPMDBEntities())
            {
                specialInstructionQuestionModel = ctx.Practice_Special_Instruction_Questions.Where(p => (p.Deleted == null || p.Deleted == false) && p.Category_Id == CategoryId).Select(svm => new SelectListViewModel { Id = svm.Question_Id, Name = svm.Question_Desc }).ToList();
            }
            if (specialInstructionQuestionModel != null)
            {
                objResponse.Status = "Sucess";
                objResponse.Response = specialInstructionQuestionModel;
            }
            else
            {
                objResponse.Status = "Error";
            }
            return objResponse;
        }
        public ResponseModel SaveSpecialInstruction(Practice_Special_Instruction_Answers model, long uId)
        {
            ResponseModel objResponse = new ResponseModel();
            Practice_Special_Instruction_Answers objPracticeSpecialInstruction = null;
            using (var ctx = new NPMDBEntities())
            {
                objPracticeSpecialInstruction = ctx.Practice_Special_Instruction_Answers.SingleOrDefault(p => p.Practice_Code == model.Practice_Code && p.Question_Id == model.Question_Id);
                if (objPracticeSpecialInstruction != null)
                {
                    objPracticeSpecialInstruction.Modify_By = uId.ToString();
                    objPracticeSpecialInstruction.Modify_Date = DateTime.Now;
                    objPracticeSpecialInstruction.Special_Instruction = model.Special_Instruction;
                    objPracticeSpecialInstruction.Status = model.Status;
                    ctx.SaveChanges();
                }
                else
                {
                    objPracticeSpecialInstruction = new Practice_Special_Instruction_Answers();
                    // long pracNotesid = Convert.ToInt64(ctx.SP_TableIdGenerator("Practice_Notes_Id").FirstOrDefault().ToString());
                    objPracticeSpecialInstruction = model;
                    objPracticeSpecialInstruction.Created_By = uId.ToString();
                    objPracticeSpecialInstruction.Created_Date = DateTime.Now;
                    objPracticeSpecialInstruction.Deleted = false;
                    ctx.Practice_Special_Instruction_Answers.Add(objPracticeSpecialInstruction);
                    ctx.SaveChanges();
                }
                if (objPracticeSpecialInstruction != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = objPracticeSpecialInstruction.Question_Id;
                }
                else
                {
                    objResponse.Status = "Error";
                }
            }
            return objResponse;
        }
        [HttpGet]
        public ResponseModel DeleteSpecialInstruction(long QuestionId, long PracticeId)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                SpecialInstructionViewModel specialInstructionModel = new SpecialInstructionViewModel();
                using (var ctx = new NPMDBEntities())
                {
                    Practice_Special_Instruction_Answers objPracticeSpecialInstruction = ctx.Practice_Special_Instruction_Answers.SingleOrDefault(p => p.Practice_Code == PracticeId && p.Question_Id == QuestionId);
                    if (objPracticeSpecialInstruction != null)
                    {
                        objPracticeSpecialInstruction.Deleted = true;
                        objPracticeSpecialInstruction.Modify_By = "1";
                        objPracticeSpecialInstruction.Modify_Date = DateTime.Now;
                        ctx.SaveChanges();
                    }
                    specialInstructionModel.specialInstructionModel = ctx.Practice_Special_Instruction_Answers.Where(c => c.Practice_Code == PracticeId && (c.Deleted == null || c.Deleted == false)).ToList();
                    specialInstructionModel.CategoryList = ctx.Practice_Special_Instruction_Category.Where(p => p.Deleted == null || p.Deleted == false).Select(svm => new SelectListViewModel { Id = svm.Category_Id, Name = svm.Category_Desc }).ToList();
                    if (specialInstructionModel != null)
                    {
                        objResponse.Status = "Sucess";
                        objResponse.Response = specialInstructionModel;
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
        #endregion PracticeSpecialInstruction
        #region ProvderNotes
        public ResponseModel GetProviderNotesList(long ProviderId)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                List<Provider_Notes> ProviderNotesList = null;
                using (var ctx = new NPMDBEntities())
                {
                    ProviderNotesList = ctx.Provider_Notes.Where(c => c.Provider_Code == ProviderId && (c.Deleted == null || c.Deleted == false)).ToList();
                }
                if (ProviderNotesList != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = ProviderNotesList;
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
        public ResponseModel GetProviderNote(long ProviderNotesId)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                Provider_Notes ProviderNote = null;
                using (var ctx = new NPMDBEntities())
                {
                    ProviderNote = ctx.Provider_Notes.SingleOrDefault(c => c.Provider_Notes_Id == ProviderNotesId && (c.Deleted == null || c.Deleted == false));
                }
                if (ProviderNote != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = ProviderNote;
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
        [HttpPost]
        public ResponseModel SaveProviderNote([FromBody] Provider_Notes ProviderNote)
        {
            ResponseModel objResponse = new ResponseModel();
            Provider_Notes objNewProviderNote = null;
            using (var ctx = new NPMDBEntities())
            {
                if (ProviderNote.Provider_Notes_Id != 0)
                {
                    objNewProviderNote = ctx.Provider_Notes.SingleOrDefault(p => p.Provider_Notes_Id == ProviderNote.Provider_Notes_Id);
                    if (objNewProviderNote != null)
                    {
                        objNewProviderNote.Note_Content = ProviderNote.Note_Content;
                        objNewProviderNote.Modified_Date = DateTime.Now;
                        ctx.SaveChanges();
                    }
                }
                else
                {
                    objNewProviderNote = new Provider_Notes();
                    long provNotesid = Convert.ToInt64(ctx.SP_TableIdGenerator("Provider_Notes_Id").FirstOrDefault().ToString());//ctx.Provider_Notes.Max(p => p.Provider_Notes_Id);
                    objNewProviderNote = ProviderNote;
                    objNewProviderNote.Provider_Notes_Id = provNotesid;
                    ctx.Provider_Notes.Add(objNewProviderNote);
                    ctx.SaveChanges();
                }
                if (ProviderNote != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = ProviderNote.Provider_Notes_Id;
                }
                else
                {
                    objResponse.Status = "Error";
                }
            }
            return objResponse;
        }
        [HttpGet]
        public ResponseModel DeleteProviderNote(long ProviderId, long ProviderNotesId)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                List<Provider_Notes> ProviderNotesList = null;
                using (var ctx = new NPMDBEntities())
                {
                    Provider_Notes objPatientNote = ctx.Provider_Notes.SingleOrDefault(c => c.Provider_Notes_Id == ProviderNotesId && (c.Deleted == null || c.Deleted == false));
                    if (objPatientNote != null)
                    {
                        objPatientNote.Deleted = true;
                        ctx.SaveChanges();
                    }
                    ProviderNotesList = ctx.Provider_Notes.Where(c => c.Provider_Code == ProviderId && (c.Deleted == null || c.Deleted == false)).ToList();
                    if (ProviderNotesList != null)
                    {
                        objResponse.Status = "Sucess";
                        objResponse.Response = ProviderNotesList;
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
        #endregion ProviderNotes
        #region ProviderWorkingHours
        public ResponseModel GetProviderWorkingHours(long ProviderId, long LocationId)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                List<Provider_Working_Days_Time> ProviderWorkingHoursList = null;
                using (var ctx = new NPMDBEntities())
                {
                    ProviderWorkingHoursList = ctx.Provider_Working_Days_Time.Where(c => c.Provider_Code == ProviderId && c.Location_Code == LocationId).ToList();
                }
                if (ProviderWorkingHoursList != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = ProviderWorkingHoursList;
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
        #endregion ProviderWorkingHours
        #region ProviderResources
        public ResponseModel GetProviderResources(long ProviderId)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                List<Provider_Resources> ProviderResourcesList = null;
                using (var ctx = new NPMDBEntities())
                {
                    ProviderResourcesList = ctx.Provider_Resources.Where(c => c.Provider_Code == ProviderId && (c.Deleted == null || c.Deleted == false)).ToList();
                }
                if (ProviderResourcesList != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = ProviderResourcesList;
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
        #endregion ProviderResources
        #region ProviderPayers
        public ResponseModel GetProviderPayers(long ProviderId, long LocationId)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                //List<Provider_Payers> ProviderPayersList = null;
                List<SP_PROVIDERPAYERSEARCH_Result> ProviderPayersList = null;
                using (var ctx = new NPMDBEntities())
                {
                    ProviderPayersList = ctx.SP_PROVIDERPAYERSEARCH(ProviderId, LocationId).ToList();
                    //ProviderPayersList = ctx.Provider_Payers.Where(c => c.Provider_Code == ProviderId && c.Location_Code == LocationId &&  (c.Deleted == null || c.Deleted == false)).ToList();
                }
                if (ProviderPayersList != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = ProviderPayersList;
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
        #endregion ProviderPayers

        //payer mapping - pir ubaid 
        #region PracticePayers
        public ResponseModel GetPracticePayers(long PracticeId )
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                //List<Provider_Payers> ProviderPayersList = null;
                List<SP_PRACTICEPROVIDERPAYERSEARCH_Result> PracticePayersList = null;
                using (var ctx = new NPMDBEntities())
                {
                    PracticePayersList = ctx.SP_PRACTICEPROVIDERPAYERSEARCH(PracticeId).ToList();
                }
                if (PracticePayersList != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = PracticePayersList;
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
        #endregion PracticePayers
        [HttpGet]
        public ResponseModel DeletePracticePayers(long PracticeId, string ProviderPayerId)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    Provider_Payers objPracticePay = ctx.Provider_Payers
                        .SingleOrDefault(c => c.Practice_code == PracticeId
                                              && c.Provider_Payer_Id == ProviderPayerId
                                              && (c.Deleted == null || c.Deleted == false));

                    if (objPracticePay != null)
                    {
                         objPracticePay.Deleted = true;
                        ctx.SaveChanges();
                        var PracticePayersList = ctx.SP_PRACTICEPROVIDERPAYERSEARCH(PracticeId).ToList();
                        objResponse.Status = "Success";
                        objResponse.Response = PracticePayersList;
                    }
                    else
                    {
                        objResponse.Status = "Error";
                        objResponse.Response = "No matching record found or already deleted.";
                    }
                }
            }
            catch (Exception ex)
            {
                objResponse.Status = "Error";
                objResponse.Response = $"An error occurred: {ex.Message}";
            }

            return objResponse;
        }

        //
        //public ResponseModel MapPayer(InsuranceModel model, long userId)
        //{
        //    ResponseModel responseModel = new ResponseModel();
        //    try
        //    {
        //        using (var ctx = new NPMDBEntities())
        //        {
        //            int affectedRows = ctx.USP_NPM_PAYER_MAPPING(model.PracticeId, model.Insurance_Id, userId);

        //            if (affectedRows > 0)
        //            {
        //                responseModel.Status = "Success";
        //                responseModel.Response = "Payer has been mapped successfully.";
        //            }
        //            else
        //            {
        //                responseModel.Status = "Failure";
        //                responseModel.Response = "This PAYER has already been mapped with the selected practice.";
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        responseModel.Status = "Error";
        //        responseModel.Response = "An error occurred while mapping the payer.";
        //    }

        //    return responseModel;
        //}

        public ResponseModel MapPayer(InsuranceModel model, long userId)
   {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var practiceCodeParam = new SqlParameter("@PracticeId", model.PracticeId);
                    var inspayerIdParam = new SqlParameter("@InsuranceId", model.Insurance_Id);
                    var createdByParam = new SqlParameter("@UserId", userId);
                    var result = ctx.Database.SqlQuery<int>(
                        "EXEC USP_NPM_PAYER_MAPPING @PracticeId, @InsuranceId, @UserId",
                        practiceCodeParam,
                        inspayerIdParam,
                        createdByParam
                    ).FirstOrDefault();

                    if (result == 1)
                    {
                        responseModel.Status = "Success";
                        responseModel.Response = "Insurance is mapped with practice successfully.";
                    }
                    else if (result == 0)
                    {
                        responseModel.Status = "Failure";
                        responseModel.Response = "Insurance is already mapped for this practice.";
                    }
                    else if (result == 2)
                    {
                        responseModel.Status = "Reactivated";
                        responseModel.Response = "Insurance is mapped with practice successfully.";
                    }
                    else 
                    {
                        responseModel.Status = "Error";
                        responseModel.Response = "Unexpected result from the stored procedure.";
                    }
                }
            }
              catch (Exception ex)
            {
                responseModel.Status = "Error";
                responseModel.Response = $"An error occurred while mapping the payer: {ex.Message}";
            }

            return responseModel;
        }

        #region Provider
        public ResponseModel GetProviders(long PracticeId)
        {
            ResponseModel objResponse = new ResponseModel();
            List<Provider> ProviderList = null;
            using (var ctx = new NPMDBEntities())
            {
                ProviderList = ctx.Providers.Where(p => p.Practice_Code == PracticeId && (p.Deleted == null || p.Deleted == false)).ToList();
            }
            if (ProviderList != null)
            {
                objResponse.Status = "Sucess";
                objResponse.Response = ProviderList;
            }
            else
            {
                objResponse.Status = "No Data Found";
            }
            return objResponse;
        }
        public ResponseModel GetProvider(long providerId, long PracticeId)
        {
            ResponseModel objResponse = new ResponseModel();
            Provider ObjProvider = null;
            using (var ctx = new NPMDBEntities())
            {
                ObjProvider = ctx.Providers.FirstOrDefault(p => p.Provider_Code == providerId && p.Practice_Code == PracticeId && (p.Deleted == null || p.Deleted == false));
                if (ObjProvider != null && !string.IsNullOrEmpty(ObjProvider.Taxonomy_Code))
                {
                    var specialityCategory = ctx.Specialty_Category.Where(c => c.TAXONOMY_CODE == ObjProvider.Taxonomy_Code).FirstOrDefault();
                    if (specialityCategory != null)
                    {
                        var group = ctx.Specialty_Groups.Where(t => t.GROUP_NO == specialityCategory.GROUP_NO).FirstOrDefault();
                        if (group != null)
                        {
                            ObjProvider.GroupNo = group.GROUP_NO;
                        }
                    }
                }
            }
            if (ObjProvider != null)
            {
                objResponse.Status = "Sucess";
                objResponse.Response = ObjProvider;
            }
            else
            {
                objResponse.Status = "No Data Found";
            }
            return objResponse;
        }
        public ResponseModel ActivateInActiveProvider(long providerId, long PracticeId, bool isActive)
        {
            ResponseModel objResponse = new ResponseModel();
            Provider ObjProvider = null;
            List<Provider> Providerlist = null;
            using (var ctx = new NPMDBEntities())
            {
                ObjProvider = ctx.Providers.Where(s => s.Practice_Code == PracticeId && s.Provider_Code == providerId).FirstOrDefault<Provider>();
                if (ObjProvider != null)
                {
                    ObjProvider.Is_Active = isActive;
                    ctx.SaveChanges();
                }
                Providerlist = ctx.Providers.Where(p => p.Practice_Code == PracticeId).ToList();
            }
            if (Providerlist != null)
            {
                objResponse.Status = "Sucess";
                objResponse.Response = Providerlist;
            }
            else
            {
                objResponse.Status = "No Data Found";
            }
            return objResponse;
        }
        public ResponseModel SaveProvider(Provider model, long userId)
        {
            long result = 0;
            var syncedPractice = _deltaSyncRepository.GetSyncedPractice(model.Practice_Code);
            try
            {
                ResponseModel objResponse = new ResponseModel();
                if (model.Provider_Code == 0)
                {
                    using (var ctx = new NPMDBEntities())
                    {
                        long ProvdierID = Convert.ToInt64(ctx.SP_TableIdGenerator("Provider_Code").FirstOrDefault().ToString());
                        model.Provider_Code = ProvdierID;
                        model.Created_Date = DateTime.Now;
                        var entity = ctx.Providers.Add(model);
                        ctx.SaveChanges();
                        result = entity == null ? 0 : entity.Provider_Code;
                        // mapped the new provider - pir ubaid
              ctx.Database.ExecuteSqlCommand("EXEC [USP_NPM_PAYER_MAPPING_PROVIDER] @PROVIDER_CODE, @CREATED_BY", 
                    new SqlParameter("@PROVIDER_CODE", ProvdierID),
                    new SqlParameter("@CREATED_BY", userId));
                        if (syncedPractice != null)
                        {
                            PracticeSynchronization practiceSynchronization = ctx.PracticeSynchronizations.Where(x => x.PracticeId == syncedPractice.Practice_Code).FirstOrDefault();
                            practiceSynchronization.UpdatedDate = DateTime.Now;
                            ctx.SaveChanges();
                        }
                    }
                }
                else
                {
                    using (var ctx = new NPMDBEntities())
                    {
                        Provider practice = ctx.Providers.Where(s => s.Provider_Code == model.Provider_Code).FirstOrDefault<Provider>();
                        if (practice != null)
                        {
                            practice.Is_Active = model.Is_Active;
                            practice.Provid_FName = model.Provid_FName;
                            practice.Provid_MName = model.Provid_MName;
                            practice.Provid_LName = model.Provid_LName;
                            practice.Provid_Middle_Name = model.Provid_Middle_Name;
                            practice.Provider_Is_Attending_Physician = model.Provider_Is_Attending_Physician;
                            practice.Is_Billing_Physician = model.Is_Billing_Physician;
                            practice.Provider_Title = model.Provider_Title;
                            practice.Date_Of_Birth = model.Date_Of_Birth;
                            practice.SSN = model.SSN;
                            practice.ADDRESS = model.ADDRESS;
                            practice.Address_Line2 = model.Address_Line2;
                            practice.ZIP = model.ZIP;
                            practice.CITY = model.CITY;
                            practice.STATE = model.STATE;
                            practice.Phone_One = model.Phone_One;
                            practice.Phone_Two = model.Phone_Two;
                            practice.Phone_Three = model.Phone_Three;
                            practice.Email_Address = model.Email_Address;
                            practice.License_No = model.License_No;
                            // practice.SPI = model.SPI;
                            practice.NPI = model.NPI;
                            practice.Provid_UPIN = model.Provid_UPIN;
                            practice.SPECIALIZATION_CODE = model.SPECIALIZATION_CODE;
                            //practice.np = model.Provid_FName;             Missing Group NPI
                            practice.DEA_Expiry_Date = model.DEA_Expiry_Date;
                            practice.DEA_No = model.DEA_No;
                            practice.Taxonomy_Code = model.Taxonomy_Code;
                            practice.federal_taxid = model.federal_taxid;
                            practice.Taxonomy_Code = model.Taxonomy_Code;
                            //practice. = model.Taxonomy_Code;                      Missing Taxonomy Fields category one and category two 
                            //practice.Taxonomy_Code = model.Taxonomy_Code;
                            practice.WCB_Rating_Code = model.WCB_Rating_Code;
                            practice.WCB_Authorization_No = model.WCB_Authorization_No;
                            practice.group_npi = model.group_npi;
                            practice.grp_taxonomy_id = model.grp_taxonomy_id;
                            ctx.SaveChanges();
                            var syncedProvider = ctx.SyncedProviders.Where(x => x.Provider_Code == model.Provider_Code)
                                                           .FirstOrDefault<SyncedProvider>();
                            if (syncedProvider != null)
                            {
                                syncedProvider.UpdatedDate = DateTime.Now;
                                ctx.SaveChanges();
                                //Update the Provider/Doctor in Inbox Health if the Provider is Synced 
                                UpdateSyncedProvider(model.Provider_Code, syncedProvider.GeneratedId);
                            }
                            result = model.Provider_Code;
                        }
                    }
                }
                if (result != 0)
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

        private void UpdateSyncedProvider(long provider_Code, long generatedId)
        {
            ProviderUpdateRequest providerUpdateRequest;
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    providerUpdateRequest = (from p in ctx.Providers
                                             join s in ctx.Specializations on p.SPECIALIZATION_CODE equals s.SPECIALIZATION_CODE
                                             where p.Provider_Code == provider_Code
                                             select new ProviderUpdateRequest()
                                             {
                                                 id = generatedId,
                                                 doctor = new Doctor()
                                                 {
                                                     first_name = p.Provid_FName,
                                                     middle_name = p.Provid_Middle_Name,
                                                     last_name = p.Provid_LName,
                                                     phone = p.Phone_One,
                                                     specialty = s.SPECIALIZATION_NAME,
                                                     email = p.Email_Address,
                                                     npi = p.NPI,
                                                     status = "floating",
                                                     provider_code = p.Provider_Code.ToString()
                                                 }
                                             }).FirstOrDefault();
                    var update = _deltaSyncRepository.UpdateProvider(providerUpdateRequest);
                    var prac_code = (from p in ctx.Providers where p.Provider_Code == provider_Code select p.Practice_Code).FirstOrDefault();
                    var practiceSyncId = ctx.PracticeSynchronizations.Where(x => x.PracticeId == prac_code).FirstOrDefault().PracticeSyncId;
                    if (update.IsSuccessful)
                    {
                        ctx.PracticeSynchronizationLogs.Add(new PracticeSynchronizationLog()
                        {
                            PracticeSyncId = practiceSyncId,
                            LogMessage = "Provider with Id " + generatedId + " has been successfully updated.",
                            LogTime = DateTime.Now
                        });
                        ctx.SaveChanges();
                    }
                    else
                    {
                        ctx.PracticeSynchronizationLogs.Add(new PracticeSynchronizationLog()
                        {
                            PracticeSyncId = practiceSyncId,
                            LogMessage = "Provider Location with Id " + generatedId + " has failed to updated.",
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
        #endregion Provider
        #region PracticeLocation
        public ResponseModel GetPracticeLocationList(long PracticeId)
        {
            ResponseModel objResponse = new ResponseModel();
            List<Practice_Locations> PracticeLocationsList = null;
            using (var ctx = new NPMDBEntities())
            {
                PracticeLocationsList = ctx.Practice_Locations.Where(p => p.Practice_Code == PracticeId && (p.Deleted == null || p.Deleted == false)).ToList();
            }
            if (PracticeLocationsList != null)
            {
                objResponse.Status = "Sucess";
                objResponse.Response = PracticeLocationsList;
            }
            else
            {
                objResponse.Status = "No Data Found";
            }
            return objResponse;
        }
        public ResponseModel GetPracticeLocation(long PracticeId, long PracticeLocationId)
        {
            ResponseModel objResponse = new ResponseModel();
            Practice_Locations ObjPracticeLoaction = null;
            using (var ctx = new NPMDBEntities())
            {
                ObjPracticeLoaction = ctx.Practice_Locations.FirstOrDefault(p => p.Location_Code == PracticeLocationId && p.Practice_Code == PracticeId && (p.Deleted == null || p.Deleted == false));
            }
            if (ObjPracticeLoaction != null)
            {
                objResponse.Status = "Sucess";
                objResponse.Response = ObjPracticeLoaction;
            }
            else
            {
                objResponse.Status = "No Data Found";
            }
            return objResponse;
        }
        public ResponseModel SavePracticeLocation([FromBody] Practice_Locations model)
        {
            long result = 0;
            var syncedPractice = _deltaSyncRepository.GetSyncedPractice(model.Practice_Code);
            try
            {
                ResponseModel objResponse = new ResponseModel();
                if (model.Location_Code == 0)
                {
                    using (var ctx = new NPMDBEntities())
                    {
                        long locationCode = Convert.ToInt64(ctx.SP_TableIdGenerator("Location_Code").FirstOrDefault().ToString());
                        model.Location_Code = locationCode;
                        model.Created_Date = DateTime.Now;
                        var entity = ctx.Practice_Locations.Add(model);
                        ctx.SaveChanges();
                        result = entity == null ? 0 : entity.Location_Code;
                        if (syncedPractice != null)
                        {
                            PracticeSynchronization practiceSynchronization = ctx.PracticeSynchronizations.Where(x => x.PracticeId == syncedPractice.Practice_Code).FirstOrDefault();
                            practiceSynchronization.UpdatedDate = DateTime.Now;
                            ctx.SaveChanges();
                        }
                    }
                }
                else
                {
                    using (var ctx = new NPMDBEntities())
                    {
                        Practice_Locations PracticeLocation = ctx.Practice_Locations.Where(s => s.Location_Code == model.Location_Code).FirstOrDefault<Practice_Locations>();
                        if (PracticeLocation != null)
                        {
                            PracticeLocation.Location_Name = model.Location_Name;
                            PracticeLocation.Clia_Expiry_Date = model.Clia_Expiry_Date;
                            PracticeLocation.Clia_Number = model.Clia_Number;
                            PracticeLocation.Created_Date = DateTime.Now;
                            PracticeLocation.Location_Address = model.Location_Address;
                            PracticeLocation.Location_Short = model.Location_Short;
                            PracticeLocation.Location_City = model.Location_City;
                            //PracticeLocation.Location_Code = model.Location_Code,
                            PracticeLocation.Location_ID = model.Location_ID;
                            PracticeLocation.Location_State = model.Location_State;
                            PracticeLocation.Location_Zip = model.Location_Zip;
                            PracticeLocation.Phone_one = model.Phone_one;
                            PracticeLocation.Phone_two = model.Phone_two;
                            PracticeLocation.is_default = model.is_default;
                            PracticeLocation.NPI = model.NPI;
                            // Tax Id and Address Two Fields are missing in db
                            ctx.SaveChanges();
                            //Check if the pratice was synced with InboxHealth
                            var syncedPracticeLocation = ctx.SyncedPracticeLocations.Where(x => x.Location_Code == model.Location_Code)
                                                           .FirstOrDefault<SyncedPracticeLocation>();
                            if (syncedPracticeLocation != null)
                            {
                                syncedPracticeLocation.UpdatedDate = DateTime.Now;
                                ctx.SaveChanges();
                                //Update the Practice(practice_attribute) in Inbox Health if the Practice Location is Synced 
                                UpdateSyncedPracticeLocation(model.Location_Code, syncedPracticeLocation.GeneratedId);
                            }
                            result = model.Location_Code;
                        }
                    }
                }
                if (result != 0)
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

        private void UpdateSyncedPracticeLocation(long location_Code, long generatedId)
        {
            PracticeLocationUpdateRequest practiceLocationUpdateRequest;
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var location = ctx.Practice_Locations.Where(s => s.Location_Code == location_Code).FirstOrDefault();
                    practiceLocationUpdateRequest = new PracticeLocationUpdateRequest()
                    {
                        id = generatedId,
                        practice = new PracticesAttribute()
                        {
                            name = location.Location_Name,
                            city = location.Location_City,
                            state = location.Location_State,
                            zip = location.Location_Zip,
                            address_line_1 = location.Location_Name,
                            time_zone = "Eastern Time (US & Canada)"
                        }
                    };
                    var update = _deltaSyncRepository.UpdatePracticeLocation(practiceLocationUpdateRequest);
                    var practiceSyncId = ctx.PracticeSynchronizations.Where(x => x.PracticeId == location.Practice_Code).FirstOrDefault().PracticeSyncId;
                    if (update.IsSuccessful)
                    {
                        ctx.PracticeSynchronizationLogs.Add(new PracticeSynchronizationLog()
                        {
                            PracticeSyncId = practiceSyncId,
                            LogMessage = "Practice Location with Id " + generatedId + " has been successfully updated.",
                            LogTime = DateTime.Now
                        });
                        ctx.SaveChanges();
                    }
                    else
                    {
                        ctx.PracticeSynchronizationLogs.Add(new PracticeSynchronizationLog()
                        {
                            PracticeSyncId = practiceSyncId,
                            LogMessage = "Practice Location with Id " + generatedId + " has failed to updated.",
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

        [HttpGet]
        public ResponseModel DeletePracticeLocation(long PracticeId, long PracticeLocationId)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                List<Practice_Locations> PracticeLocationsList = null;
                using (var ctx = new NPMDBEntities())
                {
                    Practice_Locations objPatientNote = ctx.Practice_Locations.SingleOrDefault(c => c.Location_Code == PracticeLocationId && c.Practice_Code == PracticeId && (c.Deleted == null || c.Deleted == false));
                    if (objPatientNote != null)
                    {
                        objPatientNote.Deleted = true;
                        ctx.SaveChanges();
                    }
                    PracticeLocationsList = ctx.Practice_Locations.Where(c => c.Practice_Code == PracticeId && (c.Deleted == null || c.Deleted == false)).ToList();
                    if (PracticeLocationsList != null)
                    {
                        objResponse.Status = "Sucess";
                        objResponse.Response = PracticeLocationsList;
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
        #endregion PracticeLocation
        #region PracticeFacility
        public ResponseModel GetPracticeFacilityList(long PracticeId)
        {
            ResponseModel objResponse = new ResponseModel();
            List<PracticeFacilityViewModel> PracticeFacilitiesList = null;
            using (var ctx = new NPMDBEntities())
            {
                if (ctx.PRACTICE_FACILITY.Where(p => p.Practice_Code == PracticeId && (p.Deleted == null || p.Deleted == false)).ToList().Count > 0)
                {
                    //PracticeFacilitiesList = ctx.PRACTICE_FACILITY.Where(p => p.Practice_Code == PracticeId && (p.Deleted == null || p.Deleted == false)).Select(new PracticeFacilityViewModel()).ToList();
                    PracticeFacilitiesList = ctx.PRACTICE_FACILITY.Where(p => p.Practice_Code == PracticeId && (p.Deleted == null || p.Deleted == false)).Select(svm => new PracticeFacilityViewModel { FacilityCode = svm.Facility_Code, PracticeCode = svm.Practice_Code, isDefault = svm.IsDefault, POS = ctx.Place_Of_Services.FirstOrDefault(ps => ps.POS_Code == svm.POS.Value.ToString()).POS_Name, City = ctx.Facilities.FirstOrDefault().Facility_City, Facility_Address = ctx.Facilities.FirstOrDefault().Facility_Address, Name = ctx.Facilities.FirstOrDefault().Facility_Name, NPI = ctx.Facilities.FirstOrDefault().NPI, Phone = ctx.Facilities.FirstOrDefault().Facility_Phone }).ToList();// ctx.Claim_Payments.Where(ci => ci.Claim_No == ClaimNo && ci.claim_payments_id == svm.claim_payments_id).FirstOrDefault() }).ToList();
                }
            }
            if (PracticeFacilitiesList != null)
            {
                objResponse.Status = "Sucess";
                objResponse.Response = PracticeFacilitiesList;
            }
            else
            {
                objResponse.Status = "No Data Found";
            }
            return objResponse;
        }
        public ResponseModel GetPracticeFacility(long PracticeId, long PracticeFacilityId)
        {
            ResponseModel objResponse = new ResponseModel();
            PracticeFacilityEditViewModel ObjPracticeFacility = new PracticeFacilityEditViewModel();
            using (var ctx = new NPMDBEntities())
            {
                ObjPracticeFacility.objPracticeFaclity = ctx.PRACTICE_FACILITY.FirstOrDefault(p => p.Facility_Code == PracticeFacilityId && p.Practice_Code == PracticeId && (p.Deleted == null || p.Deleted == false));
                ObjPracticeFacility.POSList = ctx.Place_Of_Services.Where(p => p.Deleted == null || p.Deleted == false).AsEnumerable().Select(svm => new SelectListViewModel { Id = ConvertId(svm.POS_Code), Name = svm.POS_Name }).ToList();
            }
            if (ObjPracticeFacility != null)
            {
                objResponse.Status = "Sucess";
                objResponse.Response = ObjPracticeFacility;
            }
            else
            {
                objResponse.Status = "No Data Found";
            }
            return objResponse;
        }
        public Int32 ConvertId(String id)
        {
            int temp = 0;
            Int32.TryParse(id, out temp);
            return temp;
        }
        [HttpPost]
        public ResponseModel SavePracticeFacility(PRACTICE_FACILITY model)
        {
            ResponseModel objResponse = new ResponseModel();
            if (model.Practice_Code != 0)
            {
                using (var ctx = new NPMDBEntities())
                {
                    PRACTICE_FACILITY PracticeFacilityIsExist = ctx.PRACTICE_FACILITY.Where(s => s.Facility_Code == model.Facility_Code && s.Practice_Code == model.Practice_Code && (s.Deleted == null || s.Deleted == false)).FirstOrDefault<PRACTICE_FACILITY>();
                    if (PracticeFacilityIsExist == null)
                    {
                        PRACTICE_FACILITY PracticeFacility = new PRACTICE_FACILITY();
                        PracticeFacility.Facility_Code = model.Facility_Code;
                        PracticeFacility.Created_Date = DateTime.Now;
                        PracticeFacility.Practice_Code = model.Practice_Code;
                        PracticeFacility.IsDefault = model.IsDefault;
                        PracticeFacility.POS = model.POS;
                        //  PracticeFacility.                                       Missing isDefault field and POS as well
                        ctx.PRACTICE_FACILITY.Add(PracticeFacility);
                        ctx.SaveChanges();
                        //Facility_Code = PracticeFacility.Facility_Code;
                    }
                }
            }
            else
            {
                using (var ctx = new NPMDBEntities())
                {
                    PRACTICE_FACILITY PracticeFacility = ctx.PRACTICE_FACILITY.Where(s => s.Facility_Code == model.Facility_Code).FirstOrDefault<PRACTICE_FACILITY>();
                    if (PracticeFacility != null)
                    {
                        //PracticeFacility. = model.HL7_Facility_Code;
                        //PracticeFacility.HL7_Facility_Code = model.HL7_Facility_Code;
                        //PracticeFacility.HL7_Facility_Code = model.HL7_Facility_Code;
                        //PracticeFacility.HL7_Facility_Code = model.HL7_Facility_Code;
                        //PracticeFacility.HL7_Facility_Code = model.HL7_Facility_Code;
                        //PracticeFacility.HL7_Facility_Code = model.HL7_Facility_Code;
                        //PracticeFacility.HL7_Facility_Code = model.HL7_Facility_Code;
                        //PracticeFacility.HL7_Facility_Code = model.HL7_Facility_Code;
                        //PracticeFacility.HL7_Facility_Code = model.HL7_Facility_Code;
                        //PracticeFacility.HL7_Facility_Code = model.HL7_Facility_Code;
                        //ctx.SaveChanges();
                    }
                }
            }
            long result = model.Facility_Code;
            if (result != 0)
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
        [HttpGet]
        public ResponseModel DeletePracticeFacility(long PracticeId, long PracticeFacilityId)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                List<PracticeFacilityViewModel> PracticeFacilitiesList = null;
                using (var ctx = new NPMDBEntities())
                {
                    if (ctx.PRACTICE_FACILITY.Where(p => p.Practice_Code == PracticeId && (p.Deleted == null || p.Deleted == false)).ToList().Count > 0)
                    {
                        PRACTICE_FACILITY objPracFacility = ctx.PRACTICE_FACILITY.SingleOrDefault(c => c.Facility_Code == PracticeFacilityId && c.Practice_Code == PracticeId && (c.Deleted == null || c.Deleted == false));
                        if (objPracFacility != null)
                        {
                            //ctx.PRACTICE_FACILITY.Remove(objPracFacility);
                            objPracFacility.Deleted = true;
                            ctx.SaveChanges();
                        }
                        PracticeFacilitiesList = ctx.PRACTICE_FACILITY.Where(p => p.Practice_Code == PracticeId && (p.Deleted == null || p.Deleted == false)).Select(svm => new PracticeFacilityViewModel { FacilityCode = svm.Facility_Code, PracticeCode = svm.Practice_Code, City = ctx.Facilities.FirstOrDefault().Facility_City, Facility_Address = ctx.Facilities.FirstOrDefault().Facility_Address, Name = ctx.Facilities.FirstOrDefault().Facility_Name, NPI = ctx.Facilities.FirstOrDefault().NPI, Phone = ctx.Facilities.FirstOrDefault().Facility_Phone }).ToList();
                        if (PracticeFacilitiesList != null)
                        {
                            objResponse.Status = "Sucess";
                            objResponse.Response = PracticeFacilitiesList;
                        }
                    }
                    //else
                    //{
                    //    objResponse.Status = "Error";
                    //}
                }
            }
            catch (Exception ex)
            {
                objResponse.Response = ex;
                objResponse.Status = "Error";
            }
            return objResponse;
        }
        #endregion PracticeFacility

        public async Task<ResponseModel> GetWcbRatings()
        {
            using (var ctx = new NPMDBEntities())
            {
                return new ResponseModel
                {
                    Response = await ctx.WCBRatings.ToListAsync(),
                    Status = "success"
                };
            }
        }
        public async Task<ResponseModel> GetSpecializations()
        {
            using (var ctx = new NPMDBEntities())
            {
                return new ResponseModel
                {
                    Response = await ctx.Specializations.ToListAsync(),
                    Status = "success"
                };
            }
        }
        public async Task<ResponseModel> GetSpecialityGroups()
        {
            using (var ctx = new NPMDBEntities())
            {
                return new ResponseModel
                {
                    Response = await ctx.Specialty_Groups.ToListAsync(),
                    Status = "success"
                };
            }
        }
        public ResponseModel GetPracticeSpecialityCategoryOne(long GroupNo)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                List<Specialty_Category> SpecialityCategory = null;
                using (var ctx = new NPMDBEntities())
                {
                    SpecialityCategory = ctx.Specialty_Category.Where(c => c.GROUP_NO == GroupNo && c.PRECEEDING_CAT == 0 && c.CAT_LEVEL == 1).ToList();
                }
                if (SpecialityCategory != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = SpecialityCategory;
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
        public ResponseModel GetPracticeSpecialityCategoryTwo(long GroupNo, int CatLevel)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                List<Specialty_Category> SpecialityCategory = null;
                using (var ctx = new NPMDBEntities())
                {
                    SpecialityCategory = ctx.Specialty_Category.Where(c => c.GROUP_NO == GroupNo && c.PRECEEDING_CAT == CatLevel).ToList();
                }
                if (SpecialityCategory != null)
                {
                    objResponse.Status = "Sucess";
                    objResponse.Response = SpecialityCategory;
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
        public ResponseModel AddPracticeSynchronization(PracticeSynchronizationModel model, long userId)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    PracticeSynchronization objSynchronization = ctx.PracticeSynchronizations.Where(c => c.PracticeId == model.PracticeId).FirstOrDefault();
                    if (objSynchronization == null)
                    {
                        ctx.PracticeSynchronizations.Add(new PracticeSynchronization
                        {
                            CompletedAt = null,
                            IsDeleted = false,
                            IsFailed = false,
                            Notes = null,
                            PracticeId = model.PracticeId,
                            StartedAt = null,
                            SynchronizedBy = userId
                        });
                        ctx.PracticeVendors.Add(new PracticeVendor
                        {
                            CreatedBy = userId,
                            CreatedDate = DateTime.Now,
                            ModifiedBy = null,
                            ModifiedDate = null,
                            PracticeId = model.PracticeId,
                            VendorId = model.VendorId
                        });
                        ctx.SaveChanges();
                        objResponse.Status = "Sucess";
                    }
                    else
                    {
                        objResponse.Status = "Practice Already Synchronized";
                    }
                }
            }
            catch (Exception)
            {
                objResponse.Status = "Error";
            }
            return objResponse;
        }
        public ResponseModel GetPracticeSynchronization(long practiceid)
        {
            ResponseModel objresponse = new ResponseModel();
            try
            {
                PracticeSynchronization _practiceSynchronization = null;
                using (var ctx = new NPMDBEntities())
                {
                    _practiceSynchronization = ctx.PracticeSynchronizations.Where(v => v.PracticeId == practiceid && v.IsDeleted == false).OrderByDescending(e => e.StartedAt).First();
                }
                if (_practiceSynchronization != null)
                {
                    objresponse.Status = "Sucess";
                    objresponse.Response = _practiceSynchronization;
                }
                else
                {
                    objresponse.Status = "Error";
                    objresponse.Response = _practiceSynchronization;
                }
            }
            catch (Exception)
            {
                objresponse.Status = "Error";
            }
            return objresponse;
        }

        public ResponseModel GetActiveUsersForPractice(long practiceCode)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var activeUsers = ctx.GetPracticeActiveUsers(practiceCode).ToList();
                    if (activeUsers != null && activeUsers.Any())
                    {
                        responseModel.Status = "Success";
                        responseModel.Response = activeUsers;
                    }
                    else
                    {
                        responseModel.Status = "No active users found for the practice";
                    }
                }
            }
            catch (Exception ex)
            {
                responseModel.Status = ex.ToString();
            }
            return responseModel;
        }

        public ResponseModel GetDivisionForUser()
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var userDivision = ctx.Divisions.ToList();
                    if (userDivision != null && userDivision.Any())
                    {
                        responseModel.Status = "Success";
                        responseModel.Response = userDivision;
                    }
                    else
                    {
                        responseModel.Status = "No division  data for the user";
                    }
                }
            }
            catch (Exception ex)
            {
                responseModel.Status = ex.ToString();
            }
            return responseModel;
        }

        public ResponseModel SaveReportingDetails(Practice_Reporting model, long userId)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    if (model != null)

                    {
                        Practice_Reporting reporting = ctx.Practice_Reporting.Where(x => x.ID == model.ID).FirstOrDefault();

                        if (reporting != null)
                        {
                            reporting.ID = model.ID;
                            reporting.Practice_Code = model.Practice_Code;
                            reporting.Practice_Alias = model.Practice_Alias;
                            reporting.Manager_ID = model.Manager_ID;
                            reporting.TeamLead_ID = model.TeamLead_ID;
                            reporting.BillingAssociate_ID = model.BillingAssociate_ID;
                            reporting.Shift = model.Shift;
                            reporting.Division_ID = model.Division_ID;
                            reporting.PHD_POCid = model.PHD_POCid;
                            reporting.User_ID = userId;
                            reporting.Modified_Date = DateTime.Now;
                            reporting.Modified_By = userId;
                            reporting.Created_By = userId;
                            reporting.Deleted = false;

                        }

                        else
                        {

                            ctx.Practice_Reporting.Add(new Practice_Reporting()
                            {
                                ID = Convert.ToInt64(ctx.SP_TableIdGenerator("ID").FirstOrDefault()),
                                Practice_Code = model.Practice_Code,
                                Practice_Alias = model.Practice_Alias,
                                Manager_ID = model.Manager_ID,
                                TeamLead_ID = model.TeamLead_ID,
                                BillingAssociate_ID = model.BillingAssociate_ID,
                                Shift = model.Shift,
                                Division_ID = model.Division_ID,
                                User_ID = userId,
                                Created_Date = DateTime.Now,
                                Created_By = userId,
                                Deleted = false,


                            });
                        }
                        ctx.SaveChanges();
                        objResponse.Status = "Success";
                    }
                }
            }
            catch (Exception ex)
            {
                objResponse.Status = ex.ToString();
            }
            return objResponse;
        }

        public ResponseModel GetReportingDetailOfPracticeUser(long userID, long practiceCode)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {

                using (var ctx = new NPMDBEntities())
                {
                    var reportingDetaill = ctx.GetPracticeUserReportingDetail(userID, practiceCode).ToList();
                    if (reportingDetaill != null && reportingDetaill.Any())
                    {
                        responseModel.Status = "Success";
                        responseModel.Response = reportingDetaill;
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

        public ResponseModel IsAutoFillEnableForPractice(long practiceCode)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    // Fetch the practice with the specified practice code
                    var practice = ctx.Practices
                        .Where(p => p.Practice_Code == practiceCode)
                        .Select(p => new { p.isAutofillClaimDetails }) // Select only the IsAutoFillClaimDetails field
                        .FirstOrDefault();

                    if (practice != null)
                    {
                        // Check if IsAutoFillClaimDetails is true or false
                        responseModel.Status = "Success";
                        responseModel.Response = practice.isAutofillClaimDetails;

                    }
                    else
                    {
                        responseModel.Status = "No data found";
                    }
                }
            }
            catch (Exception ex)
            {
                responseModel.Status = "Error";
                responseModel.Response = ex.ToString();
            }
            return responseModel;
        }

        public ResponseModel GetRecentClaimDetails( string patientAccount, string practiceCode, string claimType)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    // Fetch the practice with the specified practice code
                    var ClaimDetails = ctx.GetRecentClaimDetailsWithDiagnosis( patientAccount, practiceCode, claimType).FirstOrDefault();

                    if (ClaimDetails != null)
                    {
                        // Check if IsAutoFillClaimDetails is true or false
                        responseModel.Status = "Success";
                        responseModel.Response = ClaimDetails;

                    }
                    else
                    {
                        responseModel.Status = "No data found";
                    }
                }
            }
            catch (Exception ex)
            {
                responseModel.Status = "Error";
                responseModel.Response = ex.ToString();
            }
            return responseModel;

        }

        //added by Pir Ubaid - For Taxonomy Code - Payer mapping
        public ResponseModel TaxonomyCode(long PracticeId)
        {
            ResponseModel objResponse = new ResponseModel();

            using (var ctx = new NPMDBEntities())
            {
                var taxonomyCode = ctx.Practices
                                      .Where(s => s.Practice_Code == PracticeId)
                                      .Select(s => s.TAXONOMY_CODE)
                                      .FirstOrDefault();

                if (taxonomyCode != null)
                {
                    objResponse.Status = "Success";
                    objResponse.Response = taxonomyCode;
                }
                else
                {
                    objResponse.Status = "No Data Found";
                }
            }

            return objResponse;
        }

        //

        //added by Pir Ubaid - For NPI Code - Payer mapping
        public ResponseModel NPICode(long PracticeId)
        {
            ResponseModel objResponse = new ResponseModel();

            using (var ctx = new NPMDBEntities())
            {
                var npiCode = ctx.Practices
                                      .Where(s => s.Practice_Code == PracticeId)
                                      .Select(s => s.NPI)
                                      .FirstOrDefault();

                if (npiCode != null)
                {
                    objResponse.Status = "Success";
                    objResponse.Response = npiCode;
                }
                else
                {
                    objResponse.Status = "No Data Found";
                }
            }

            return objResponse;
        }

        //
    }
}

