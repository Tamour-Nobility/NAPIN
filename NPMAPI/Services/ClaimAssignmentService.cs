using System;
using System.Collections.Generic;
using System.Linq;
using NPMAPI.Models;
using NPMAPI.Repositories;

namespace NPMAPI.Services
{
    public partial class ClaimAssignmentService : IClaimAssignmentRepository
    {
        public ResponseModel GetSelectedUserList(long practicecode)
        {
            ResponseModel responseModel = new ResponseModel();
            List<AssigneeusersList> usersList = null;
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    // usersList = ctx.SP_SelectAllPracticeUsers(practicecode).ToList();
                    usersList = (from up in ctx.Users_Practice_Provider
                                 join u in ctx.Users on up.User_Id equals u.UserId
                                 join p in ctx.Practices on up.Practice_Code equals p.Practice_Code
                                 join r in ctx.Roles on u.RoleId equals r.RoleId into uc
                                 from r in uc.DefaultIfEmpty()
                                 where (up.Practice_Code == practicecode && (p.Is_Active ?? true) == true
                                 && (p.Deleted ?? false) == false && (u.IsActive ?? true) == true && (u.IsDeleted ?? false) == false)
                                 select new AssigneeusersList()
                                 {
                                     Id = u.UserId,
                                     UserName = u.UserName,
                                     Name = u.UserId + "-" + u.LastName + ", " + u.FirstName + "-" + u.UserName,
                                     FullName = u.LastName + " " + u.FirstName
                                 }).Distinct().ToList();
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

        public ResponseModel PostAllAssignedClaimsforUser(claimassigneeModel model, long userid)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var claimAssignee_ID = Convert.ToInt64(ctx.SP_TableIdGenerator("Claim_AssigneeID").FirstOrDefault().ToString());
                    ctx.ClaimAssignee_CL.Add(new ClaimAssignee_CL()
                    {
                        Claim_AssigneeID = claimAssignee_ID,
                        Priority = model.Priority,
                        Status = model.Status,
                        Start_Date = model.Start_Date,
                        Due_Date = model.Due_Date,
                        Assignedto_UserId = model.Assignedto_UserId,
                        Assignedto_UserName = model.Assignedto_UserName,
                        Assignedto_FullName = model.Assignedto_FullName,
                        AssignedBy_UserId = model.AssignedBy_UserId,
                        AssignedBy_UserName = model.AssignedBy_UserName,
                        AssignedBy_FullName = model.AssignedBy_FullName,
                        PracticeCode = model.PracticeCode,
                        ClaimNo = model.ClaimNo,
                        Claim_AmtDue = model.Claim_AmtDue,
                        Claim_AmtPaid = model.Claim_AmtPaid,
                        Claim_Claimtotal = model.Claim_Claimtotal,
                        Claim_DOS = model.Claim_DOS,
                        Claim_AttendingPhysician = model.Claim_AttendingPhysician,
                        Claim_BillingPhysician = model.Claim_BillingPhysician,
                        ProviderFullName = model.ProviderFullName,
                        PatientAccount = model.PatientAccount,
                        PatientFullName = model.PatientFullName,
                        countentries = model.countentries,
                        Created_By = userid,
                        Modified_By = null,
                        Modified_Date = null,
                        Created_Date = DateTime.Now,
                        modification_allowed = null,
                    });
                    if (ctx.SaveChanges() > 0)
                    {
                        ctx.ClaimAssignee_Notes.Add(new ClaimAssignee_Notes()
                        {
                            Notes_ID = Convert.ToInt64(ctx.SP_TableIdGenerator("Notes_ID").FirstOrDefault().ToString()),
                            ClaimAssignee_notes_ID = claimAssignee_ID,
                            Claim_notes = model.Claim_notes,
                            Name = model.AssignedBy_FullName,
                            Created_By = userid,
                            Modified_By = null,
                            Modified_Date = null,
                            Created_Date = DateTime.Now,
                            modification_allowed = false
                        });
                        if (ctx.SaveChanges() > 0)
                        {
                            responseModel.Status = "Success";
                        }
                        else
                        {
                            responseModel.Status = "Notes Error";
                        }
                    }
                    else
                    {
                        responseModel.Status = "Error";
                    };

                }

            }
            catch (Exception ex)
            {
                responseModel.Status = ex.ToString();
            }


            return responseModel;
        }

        public ResponseModel GetAssignedClaimDataforUser(long claimnumber)
        {
            ResponseModel responseModel = new ResponseModel();
            List<SP_GetSpecificClaiminfo_Result> usersList = null;
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    usersList = ctx.SP_GetSpecificClaiminfo(claimnumber).ToList();
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

        public ResponseModel EditAssignedClaimsforUser(editassignedclaimModel model, long userid)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                ClaimAssignee_CL _CL;
                using (var ctx = new NPMDBEntities())
                {
                    _CL = ctx.ClaimAssignee_CL.FirstOrDefault(u => u.Claim_AssigneeID == model.Claim_AssigneeID);
                    if (_CL != null)
                    {
                        _CL.Priority = model.Priority;
                        _CL.Status = model.Status;
                        _CL.Start_Date = model.Start_Date;
                        _CL.Due_Date = model.Due_Date;
                        _CL.Assignedto_UserId = model.Assignedto_UserId;
                        _CL.Assignedto_UserName = model.Assignedto_UserName;
                        _CL.Assignedto_FullName = model.Assignedto_FullName;
                        _CL.AssignedBy_UserId = model.AssignedBy_UserId;
                        _CL.AssignedBy_UserName = model.AssignedBy_UserName;
                        _CL.AssignedBy_FullName = model.AssignedBy_FullName;
                        _CL.PracticeCode = model.PracticeCode;
                        _CL.ClaimNo = model.ClaimNo;
                        _CL.Claim_AmtDue = model.Claim_AmtDue;
                        _CL.Claim_AmtPaid = model.Claim_AmtPaid;
                        _CL.Claim_Claimtotal = model.Claim_Claimtotal;
                        _CL.Claim_DOS = model.Claim_DOS;
                        _CL.Claim_AttendingPhysician = model.Claim_AttendingPhysician;
                        _CL.Claim_BillingPhysician = model.Claim_BillingPhysician;
                        _CL.ProviderFullName = model.ProviderFullName;
                        _CL.PatientAccount = model.PatientAccount;
                        _CL.PatientFullName = model.PatientFullName;
                        _CL.countentries = model.countentries;
                        _CL.Created_By = userid;
                        _CL.Modified_By = null;
                        _CL.Modified_Date = null;
                        _CL.Created_Date = DateTime.Now;
                        _CL.modification_allowed = null;

                        ctx.Entry(_CL).State = System.Data.Entity.EntityState.Modified;
                        if (ctx.SaveChanges() > 0)
                        {
                            ctx.ClaimAssignee_Notes.Add(new ClaimAssignee_Notes()
                            {
                                Notes_ID = Convert.ToInt64(ctx.SP_TableIdGenerator("Notes_ID").FirstOrDefault().ToString()),
                                ClaimAssignee_notes_ID = model.Claim_AssigneeID,
                                Claim_notes = model.Claim_notes,
                                Name = model.AssignedBy_FullName,
                                Created_By = model.AssignedBy_UserId,
                                Modified_By = null,
                                Modified_Date = null,
                                Created_Date = DateTime.Now,
                                modification_allowed = false
                            });
                            if (ctx.SaveChanges() > 0)
                            {
                                responseModel.Status = "Success";
                            }
                            else
                            {
                                responseModel.Status = "Notes Error";
                            }
                        }
                        else
                        {
                            responseModel.Status = "Error";
                        }

                    }

                    //        }

                }
            }
            catch (Exception ex)
            {
                responseModel.Status = ex.ToString();
            }


            return responseModel;
        }

        public ResponseModel GetAllAssignedClaimsforspecificuser(long practice_code, long userid)
        {
            ResponseModel responseModel = new ResponseModel();
            List<ClaimAssignee_CL> usersList = null;
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    usersList = ctx.ClaimAssignee_CL.Where(e => e.PracticeCode == practice_code && e.Assignedto_UserId == userid).ToList();
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

        public ResponseModel GetAllAssignedClaimsForPracticeuser(long practice_code)
        {
            ResponseModel responseModel = new ResponseModel();
            List<ClaimAssignee_CL> usersList = null;
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    usersList = ctx.ClaimAssignee_CL.Where(e => e.PracticeCode == practice_code).ToList();
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

        public ResponseModel GetAllAssignedClaimsNotificationsforuser(long practice_code, long userid)
        {
            ResponseModel responseModel = new ResponseModel();
            List<SP_CountAssignedclaims_Result> usersList = null;
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    usersList = ctx.SP_CountAssignedclaims(practice_code, userid).ToList();
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

        public ResponseModel GetAllAssignedAccountsNotificationsforuser(long practice_code, long userid)
        {
            ResponseModel responseModel = new ResponseModel();
            List<SP_CountAssignedAccounts_Result> usersList = null;
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    usersList = ctx.SP_CountAssignedAccounts(practice_code, userid).ToList();
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

        public ResponseModel GetSpecificAssignedClaimDataforuser(long claimassignee_id)
        {
            ResponseModel responseModel = new ResponseModel();
            List<ClaimAssignee_CL> usersList = null;
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    usersList = ctx.ClaimAssignee_CL.Where(e => e.Claim_AssigneeID == claimassignee_id).ToList();
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


        public ResponseModel GetSpecificAssignedClaimNotesforuser(long ClaimAssignee_notes_ID)
        {
            ResponseModel responseModel = new ResponseModel();
            List<ClaimAssignee_Notes> usersList = null;
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    usersList = ctx.ClaimAssignee_Notes.Where(e => e.ClaimAssignee_notes_ID == ClaimAssignee_notes_ID).ToList();
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








        //Account  Level ***********************************************************
        public ResponseModel PostAllAssignedAccountforUser(accountassigneeModel model, long userid)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var account_AssigneeID = Convert.ToInt64(ctx.SP_TableIdGenerator("Account_AssigneeID").FirstOrDefault().ToString());
                    ctx.AccountAssignee_AL.Add(new AccountAssignee_AL()
                    {
                        Account_AssigneeID = account_AssigneeID,
                        Priority = model.Priority,
                        Status = model.Status,
                        Start_Date = model.Start_Date,
                        Due_Date = model.Due_Date,
                        Assignedto_UserId = model.Assignedto_UserId,
                        Assignedto_UserName = model.Assignedto_UserName,
                        Assignedto_FullName = model.Assignedto_FullName,
                        AssignedBy_UserId = model.AssignedBy_UserId,
                        AssignedBy_UserName = model.AssignedBy_UserName,
                        AssignedBy_FullName = model.AssignedBy_FullName,
                        PracticeCode = model.PracticeCode,
                        PatientAccount = model.PatientAccount,
                        PatientFullName = model.PatientFullName,
                        Created_By = userid,
                        Modified_By = null,
                        Modified_Date = null,
                        Created_Date = DateTime.UtcNow,
                        modification_allowed = null,
                    });
                    if (ctx.SaveChanges() > 0)
                    {
                        ctx.AccountAssignee_Notes.Add(new AccountAssignee_Notes()
                        {
                            Notes_ID_AL = Convert.ToInt64(ctx.SP_TableIdGenerator("Notes_ID_AL").FirstOrDefault().ToString()),
                            AccountAssignee_notes_ID = account_AssigneeID,
                            Account_notes = model.Account_notes,
                            Name = model.AssignedBy_FullName,
                            Created_By = userid,
                            Modified_By = null,
                            Modified_Date = null,
                            Created_Date = DateTime.UtcNow,
                            modification_allowed = false
                        });
                        if (ctx.SaveChanges() > 0)
                        {
                            responseModel.Status = "Success";
                        }
                        else
                        {
                            responseModel.Status = "Notes Error";
                        }
                    }
                    else
                    {
                        responseModel.Status = "Error";
                    };

                }

            }
            catch (Exception ex)
            {
                responseModel.Status = ex.ToString();
            }


            return responseModel;
        }


        public ResponseModel GetAllAssignedAccountsforspecificuser(long practice_code, long userid)
        {
            ResponseModel responseModel = new ResponseModel();
            List<AccountAssignee_AL> usersList = null;
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    usersList = ctx.AccountAssignee_AL.Where(e => e.PracticeCode == practice_code && e.Assignedto_UserId == userid).ToList();
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



        public ResponseModel GetAllAssignedAccountsForPracticeuser(long practice_code)
        {
            ResponseModel responseModel = new ResponseModel();
            List<AccountAssignee_AL> usersList = null;
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    usersList = ctx.AccountAssignee_AL.Where(e => e.PracticeCode == practice_code).ToList();
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


        public ResponseModel GetSpecificAssignedAccountNotesforuser(long AccountAssignee_notes_ID)
        {
            ResponseModel responseModel = new ResponseModel();
            List<AccountAssignee_Notes> usersList = null;
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    usersList = ctx.AccountAssignee_Notes.Where(e => e.AccountAssignee_notes_ID == AccountAssignee_notes_ID).ToList();
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


        public ResponseModel GetSpecificAssignedAccountDataforuser(long accountassignee_id)
        {
            ResponseModel responseModel = new ResponseModel();
            List<AccountAssignee_AL> usersList = null;
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    usersList = ctx.AccountAssignee_AL.Where(e => e.Account_AssigneeID == accountassignee_id).ToList();
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


        public ResponseModel EditAssignedAccountforUser(editassignedaccountModel model, long userid)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                AccountAssignee_AL _AL;
                using (var ctx = new NPMDBEntities())
                {
                    _AL = ctx.AccountAssignee_AL.FirstOrDefault(u => u.Account_AssigneeID == model.Account_AssigneeID);
                    if (_AL != null)
                    {
                        _AL.Priority = model.Priority;
                        _AL.Status = model.Status;
                        _AL.Start_Date = model.Start_Date;
                        _AL.Due_Date = model.Due_Date;
                        _AL.Assignedto_UserId = model.Assignedto_UserId;
                        _AL.Assignedto_UserName = model.Assignedto_UserName;
                        _AL.Assignedto_FullName = model.Assignedto_FullName;
                        _AL.AssignedBy_UserId = model.AssignedBy_UserId;
                        _AL.AssignedBy_UserName = model.AssignedBy_UserName;
                        _AL.AssignedBy_FullName = model.AssignedBy_FullName;
                        _AL.PracticeCode = model.PracticeCode;
                        _AL.PatientAccount = model.PatientAccount;
                        _AL.PatientFullName = model.PatientFullName;
                        _AL.Created_By = userid;
                        _AL.Modified_By = null;
                        _AL.Modified_Date = null;
                        _AL.Created_Date = DateTime.UtcNow;
                        _AL.modification_allowed = null;

                        ctx.Entry(_AL).State = System.Data.Entity.EntityState.Modified;
                        if (ctx.SaveChanges() > 0)
                        {
                            ctx.AccountAssignee_Notes.Add(new AccountAssignee_Notes()
                            {
                                Notes_ID_AL = Convert.ToInt64(ctx.SP_TableIdGenerator("Notes_ID_AL").FirstOrDefault().ToString()),
                                AccountAssignee_notes_ID = model.Account_AssigneeID,
                                Account_notes = model.Account_notes,
                                Name = model.AssignedBy_FullName,
                                Created_By = model.AssignedBy_UserId,
                                Modified_By = null,
                                Modified_Date = null,
                                Created_Date = DateTime.UtcNow,
                                modification_allowed = false
                            });
                            if (ctx.SaveChanges() > 0)
                            {
                                responseModel.Status = "Success";
                            }
                            else
                            {
                                responseModel.Status = "Notes Error";
                            }
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



    }
}