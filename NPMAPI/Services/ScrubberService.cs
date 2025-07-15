using NPMAPI.Models;
using NPMAPI.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Dapper;
using NPMAPI.Models.ViewModels;

namespace NPMAPI.Services
{
    public class ScrubberService : IScrubberRepository
    {
        List<spGetFailedClaimsFromScrubber_Result> violatedClaims = new List<spGetFailedClaimsFromScrubber_Result>();
        List<spGetCleanClaims_Result> cleanClaims = new List<spGetCleanClaims_Result>();


        public ResponseModel GetAllViolated(string practiceCode)
        {
            ResponseModel res = new ResponseModel();


            using (var ctx = new NPMDBEntities())
            {

                var violatedClaims = ctx.spGetFailedClaimsFromScrubber(Convert.ToInt64(practiceCode)).ToList();

                if (violatedClaims.Count() >= 0)
                {
                    res.Status = "Success";
                    res.Response = violatedClaims;

                }
                else
                {
                    res.Status = "Error";
                    res.Response = null;
                }
                return res;
            }

        }

        public ResponseModel GetAllCleanClaims(string practiceCode)
        {


            ResponseModel res = new ResponseModel();

            using (var ctx = new NPMDBEntities())
            {
                var cleanClaims = ctx.spGetCleanClaims(Convert.ToInt64(practiceCode)).ToList();

                if (cleanClaims.Count() >= 0)
                {
                    res.Status = "success";
                    res.Response = cleanClaims;
                }
                else
                {
                    res.Status = "Error";
                    res.Response = null;
                }

                return res;
            }
        }

        public ResponseModel AddToScrubberQueue(ClaimsViewModel claimModel)
        {
            ResponseModel res = new ResponseModel();
            using (var ctx = new NPMDBEntities())
            {
                ScrubberQueue searchedClaim = null;
                searchedClaim = ctx.ScrubberQueues.Where(r => r.ClaimNo == claimModel.ClaimModel.Claim_No).FirstOrDefault();
                if (searchedClaim != null)
                {

                    ctx.sp_deleteFromScrubberQueue(claimModel.ClaimModel.Claim_No);
                    ctx.insertInScrubberQueue(claimModel.ClaimModel.Claim_No, claimModel.ClaimModel.Patient_Account, DateTime.Now, claimModel.ClaimModel.Billing_Physician, null, null, null, null);
                    res.Status = "Success: Claim readded to Scrubber";
                    res.Response = claimModel;
                    return res;
                }
                else
                {
                    try
                    {
                        ctx.insertInScrubberQueue(claimModel.ClaimModel.Claim_No, claimModel.ClaimModel.Patient_Account, DateTime.Now, claimModel.ClaimModel.Billing_Physician, null, null, null, null);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    res.Status = "Success: Claim added to scrubber for first time";
                    res.Response = claimModel;
                    return res;
                }
                res.Status = "Claim already present in the scrubber queue";
                res.Response = null;
                return res;
            }
        }

        //Added By Hamza Ikhlaq For Custom_Edits
        #region CustomEdits
        public List<GetColumn_List> GetColumnList_FrontEnd()
        {
            using (var ctx = new NPMDBEntities())
            {
                var result = ctx.GetColumn_List.ToList();
                return result;
            }
        }
        public ResponseModel GetColumnList(GetColumnList GC_L)
        {
            ResponseModel res = new ResponseModel();
            using (var ctx = new NPMDBEntities())
            {
                if (GC_L.TableName != null)
                {
                    var result = ctx.GetCustomEditColums((GC_L.TableName)).ToList();
                    if (result.Count() > 0)
                    {
                        res.Status = "Column List";
                        res.Response = result;
                    }
                    else
                    {
                        res.Status = "No Column Exist Against this Table";
                        res.Response = null;
                    }

                }
                return res;
            }

        }
        public ResponseModel GetTableList()
        {
            ResponseModel res = new ResponseModel();
            using (var ctx = new NPMDBEntities())
            {
                var result = ctx.USP_GetCustomEditTable().ToList();
                res.Response = result;
            }
            return res;
        }
        public ResponseModel AddCustom_Edits_Rules(CustomEdits_ColumnsList ce, string userName)
        {
            ResponseModel res = new ResponseModel();
            try
            {
                ce.Status = true;
                if (ce.customedits.Count() > 0)

                {
                    int index = ce.customedits.Count() - 1;
                    ce.customedits[index].EditConditon = "";
                }
                using (var ctx = new NPMDBEntities())
                {
                    Custom_Scrubber_Rules csr = new Custom_Scrubber_Rules();
                    if (ce.Gcc_id == 0 || ce.Gcc_id == null)
                    {
                        long CustomEditRule_Id = Convert.ToInt64(ctx.SP_TableIdGenerator("Gc_id").FirstOrDefault().ToString());
                        csr.Gc_id = CustomEditRule_Id;
                        csr.Practice_Code = ce.Practice_Code;

                        if (ce.customedits != null)
                        {
                            foreach (var item in ce.customedits)
                            {
                                csr.Entity1 = item.Entity1;
                                csr.Field1 = item.Field1;
                                csr.Operator = item.Operator;
                                csr.Value = item.Value ?? "";
                                csr.Entity2 = item.Entity2 ?? "";
                                csr.Field2 = item.Field2 ?? "";
                                csr.Condition = item.EditConditon;
                                if (item.EditConditon == "AND")
                                {
                                    csr.Status = ce.Status;
                                    ctx.Custom_Scrubber_Rules.Add(csr);
                                    ctx.SaveChanges();
                                }
                                else
                                {
                                    csr.Edit_Name = ce.EditName ?? "";
                                    csr.Edit_Description = ce.EditDescirption ?? "";
                                    csr.ErrorMessage = ce.EditErrorMassage ?? "";
                                    csr.Created_Date = DateTime.Now.Date;
                                    csr.Created_By = userName;
                                    if (ce.EditErrorMassage != null)
                                    {
                                        csr.Status = ce.Status;
                                    }
                                    ctx.Custom_Scrubber_Rules.Add(csr);
                                    ctx.SaveChanges();
                                }

                            }
                            res.Status = "Successfully Added";
                        }
                    }

                    else if (ce.Gcc_id > 0)
                    {

                        if (ce.customedits != null)
                        {
                            for (int index = 0; index < ce.customedits.Count(); index++)
                            {
                                var editid = ce.customedits[index].Edit_id;
                                var editdata = ce.customedits[index];
                                var existingRule = ctx.Custom_Scrubber_Rules.Where(r => r.Edit_id == editid).ToList();
                                if (existingRule != null && existingRule.Count > 0)
                                {
                                    existingRule[0].Entity1 = editdata.Entity1;
                                    existingRule[0].Field1 = editdata.Field1;
                                    existingRule[0].Operator = editdata.Operator;
                                    existingRule[0].Value = editdata.Value ?? "";
                                    existingRule[0].Entity2 = editdata.Entity2 ?? "";
                                    existingRule[0].Field2 = editdata.Field2 ?? "";
                                    existingRule[0].Condition = editdata.EditConditon;
                                    if (editdata.EditConditon == "AND")
                                    {
                                        existingRule[0].Status = ce.Status;
                                        ctx.SaveChanges();
                                    }
                                    else
                                    {
                                        existingRule[0].Edit_Name = ce.EditName ?? "";
                                        existingRule[0].Edit_Description = ce.EditDescirption ?? "";
                                        existingRule[0].ErrorMessage = ce.EditErrorMassage ?? "";
                                        existingRule[0].Modified_Date = DateTime.Now.Date;
                                        existingRule[0].Modified_By = userName;
                                        if (ce.EditErrorMassage != null)
                                        {
                                            existingRule[0].Status = ce.Status;
                                        }
                                        ctx.SaveChanges();


                                    }

                                }
                                else
                                {
                                    var getdata = ctx.Custom_Scrubber_Rules.Where(r => r.Gc_id == ce.Gcc_id).ToList();
                                    foreach (var item in getdata)
                                    {
                                        item.ErrorMessage = null;
                                        item.Edit_Name = null;
                                        item.Edit_Description = null;
                                        item.Status = null;

                                    }
                                    csr.Gc_id = ce.Gcc_id;
                                    csr.Practice_Code = ce.Practice_Code;
                                    csr.Entity1 = editdata.Entity1;
                                    csr.Field1 = editdata.Field1;
                                    csr.Operator = editdata.Operator;
                                    csr.Value = editdata.Value ?? "";
                                    csr.Entity2 = editdata.Entity2 ?? "";
                                    csr.Field2 = editdata.Field2 ?? "";
                                    csr.Condition = editdata.EditConditon;
                                    csr.Status = ce.Status;
                                    if (editdata.EditConditon == "AND")
                                    {
                                        ctx.Custom_Scrubber_Rules.Add(csr);
                                        ctx.SaveChanges();
                                    }
                                    else
                                    {
                                        csr.Edit_Name = ce.EditName ?? "";
                                        csr.Edit_Description = ce.EditDescirption ?? "";
                                        csr.ErrorMessage = ce.EditErrorMassage ?? "";
                                        csr.Modified_Date = DateTime.Now.Date;
                                        csr.Modified_By = userName;
                                        if (ce.EditErrorMassage != null)
                                        {
                                            csr.Status = ce.Status;
                                        }
                                        ctx.Custom_Scrubber_Rules.Add(csr);
                                        ctx.SaveChanges();
                                    }
                                }

                            }
                            res.Status = "Successfully Updated";
                        }
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
            }

            return res;
        }

        public List<Custom_Scrubber_Rules> GetAllCustomEdits(long Practice_Code)
        {

            using (var ctx = new NPMDBEntities())
            {

                var result = ctx.Custom_Scrubber_Rules.Where(e => e.Practice_Code == Practice_Code).ToList();
                return result;

            }
        }
        public ResponseModel GetCustomRuleById(int id)
        {
            ResponseModel res = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    if (id != null)
                    {
                        var result = ctx.Custom_Scrubber_Rules.Where(e => e.Gc_id == id).ToList();
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

        public ResponseModel CustomEditsStatus(int id)
        {

            ResponseModel res = new ResponseModel();
            Custom_Scrubber_Rules csr = new Custom_Scrubber_Rules();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    if (id != null)
                    {
                        var result = ctx.Custom_Scrubber_Rules.Where(e => e.Gc_id == id).ToList();
                        foreach (var r in result)
                        {

                            if (r.Status == true)
                            {
                                r.Status = false;
                            }
                            else if (r.Status == null)
                            {
                                r.Status = null;
                            }
                            else
                            {
                                r.Status = true;
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


        #endregion


        //Shehzad Khan Scrubber rejection code 

        public ResponseModel GetScrubberRejection(string practiceCode, DateTime Date_From, DateTime Date_To)
        {
            ResponseModel res = new ResponseModel();

            using (var ctx = new NPMDBEntities())
            {
                var violatedClaims = ctx.sp_GetScrubberRejectionReports(
                    Convert.ToInt64(practiceCode),
                    Date_From,
                    Date_To
                ).OrderByDescending(c => c.RejectionDate).ToList();
                if (violatedClaims.Count > 0)
                {
                    var groupedClaims = violatedClaims.GroupBy(c => c.ClaimNumber);
                    foreach (var group in groupedClaims)
                    {
                        if (group.Count() == 1)
                        {
                            var singleRecord = group.First();
                            singleRecord.ActionTaken = (singleRecord.ActionTaken == "Yes") ? "Yes" : "NO";
                        }
                        else if (group.Count() == 2)
                        {
                            var firstRecord = group.First();
                            var secondRecord = group.Skip(1).First();

                            if (firstRecord.ActionTaken == "NO" && secondRecord.ActionTaken == "NO")
                            {
                                firstRecord.ActionTaken = "Yes";
                                secondRecord.ActionTaken = "NO";
                            }
                            else
                            {
                                if (firstRecord.ActionTaken == "Yes" && secondRecord.ActionTaken == "Yes")
                                {
                                    firstRecord.ActionTaken = "Yes";
                                    secondRecord.ActionTaken = "Yes";
                                }
                                if (firstRecord.ActionTaken == "NO" && secondRecord.ActionTaken == "Yes")
                                {
                                    firstRecord.ActionTaken = "Yes";
                                    secondRecord.ActionTaken = "Yes";
                                }
                                else if (firstRecord.ActionTaken == "NO" && secondRecord.ActionTaken == "NO")
                                {
                                    firstRecord.ActionTaken = "Yes";
                                    secondRecord.ActionTaken = "NO";
                                }
                            }
                        }
                        else if (group.Count() > 2 && group.Count() < 4)
                        {
                            var thirdRecord = group.Skip(2).First();

                            if (thirdRecord.ActionTaken == "NO")
                            {
                                foreach (var record in group.Take(2))
                                {
                                    record.ActionTaken = "Yes";
                                }
                                thirdRecord.ActionTaken = "NO";
                            }
                            else
                            {
                                if (thirdRecord.ActionTaken == "Yes")
                                {
                                    foreach (var record in group.Take(2))
                                    {
                                        record.ActionTaken = "Yes";
                                    }
                                    thirdRecord.ActionTaken = "Yes";
                                }
                            }
                        }
                        else if (group.Count() > 3 && group.Count() < 5)
                        {
                            var fourthRecord = group.Skip(3).First();

                            if (fourthRecord.ActionTaken == "NO")
                            {
                                foreach (var record in group.Take(3))
                                {
                                    record.ActionTaken = "Yes";
                                }
                                fourthRecord.ActionTaken = "NO";
                            }
                            else
                            {
                                if (fourthRecord.ActionTaken == "Yes")
                                {
                                    foreach (var record in group.Take(3))
                                    {
                                        record.ActionTaken = "Yes";
                                    }
                                    fourthRecord.ActionTaken = "Yes";
                                }
                            }
                        }
                        else if (group.Count() > 4 && group.Count() < 6)

                        {
                            var FifthRecord = group.Skip(4).First();

                            if (FifthRecord.ActionTaken == "NO")
                            {
                                foreach (var record in group.Take(5))
                                {
                                    record.ActionTaken = "Yes";
                                }
                                FifthRecord.ActionTaken = "NO";
                            }
                            else
                            {
                                if (FifthRecord.ActionTaken == "Yes")
                                {
                                    foreach (var record in group.Take(5))
                                    {
                                        record.ActionTaken = "Yes";
                                    }
                                    FifthRecord.ActionTaken = "Yes";
                                }
                            }
                        }
                        else if (group.Count() > 5 && group.Count() < 7)
                        {
                            var sixthRecord = group.Skip(4).First();

                            if (sixthRecord.ActionTaken == "NO")
                            {
                                foreach (var record in group.Take(6))
                                {
                                    record.ActionTaken = "Yes";
                                }
                                sixthRecord.ActionTaken = "NO";
                            }
                            else
                            {
                                if (sixthRecord.ActionTaken == "Yes")
                                {
                                    foreach (var record in group.Take(6))
                                    {
                                        record.ActionTaken = "Yes";
                                    }
                                    sixthRecord.ActionTaken = "Yes";
                                }
                            }
                        }
                        else if (group.Count() > 6 && group.Count() < 8)
                        {
                            var sevenththRecord = group.Skip(7).First();

                            if (sevenththRecord.ActionTaken == "NO")
                            {
                                foreach (var record in group.Take(7))
                                {
                                    record.ActionTaken = "Yes";
                                }
                                sevenththRecord.ActionTaken = "NO";
                            }
                            else
                            {
                                if (sevenththRecord.ActionTaken == "Yes")
                                {
                                    foreach (var record in group.Take(7))
                                    {
                                        record.ActionTaken = "Yes";
                                    }
                                    sevenththRecord.ActionTaken = "Yes";
                                }
                            }
                        }
                        else if (group.Count() > 7 && group.Count() < 9)
                        {
                            var eightRecord = group.Skip(7).First();

                            if (eightRecord.ActionTaken == "NO")
                            {
                                foreach (var record in group.Take(8))
                                {
                                    record.ActionTaken = "Yes";
                                }
                                eightRecord.ActionTaken = "NO";
                            }
                            else
                            {
                                if (eightRecord.ActionTaken == "Yes")
                                {
                                    foreach (var record in group.Take(8))
                                    {
                                        record.ActionTaken = "Yes";
                                    }
                                    eightRecord.ActionTaken = "Yes";
                                }
                            }
                        }
                        else if (group.Count() > 8 && group.Count() < 10)
                        {
                            var ninthRecord = group.Skip(8).First();

                            if (ninthRecord.ActionTaken == "NO")
                            {
                                foreach (var record in group.Take(9))
                                {
                                    record.ActionTaken = "Yes";
                                }
                                ninthRecord.ActionTaken = "NO";
                            }
                            else
                            {
                                if (ninthRecord.ActionTaken == "Yes")
                                {
                                    foreach (var record in group.Take(9))
                                    {
                                        record.ActionTaken = "Yes";
                                    }
                                    ninthRecord.ActionTaken = "Yes";
                                }
                            }
                        }
                        else if (group.Count() > 9 && group.Count() < 11)
                        {
                            var tenthRecord = group.Skip(9).First();

                            if (tenthRecord.ActionTaken == "NO")
                            {
                                foreach (var record in group.Take(10))
                                {
                                    record.ActionTaken = "Yes";
                                }
                                tenthRecord.ActionTaken = "NO";
                            }
                            else
                            {
                                if (tenthRecord.ActionTaken == "Yes")
                                {
                                    foreach (var record in group.Take(10))
                                    {
                                        record.ActionTaken = "Yes";
                                    }
                                    tenthRecord.ActionTaken = "Yes";
                                }
                            }
                        }
                        else if (group.Count() > 10 && group.Count() < 12)
                        {
                            var elevenRecord = group.Skip(10).First();

                            if (elevenRecord.ActionTaken == "NO")
                            {
                                foreach (var record in group.Take(11))
                                {
                                    record.ActionTaken = "Yes";
                                }
                                elevenRecord.ActionTaken = "NO";
                            }
                            else
                            {
                                if (elevenRecord.ActionTaken == "Yes")
                                {
                                    foreach (var record in group.Take(11))
                                    {
                                        record.ActionTaken = "Yes";
                                    }
                                    elevenRecord.ActionTaken = "Yes";
                                }
                            }
                        }
                        else if (group.Count() > 11)
                        {
                            var elevenRecord = group.Skip(11).First();

                            if (elevenRecord.ActionTaken == "NO")
                            {
                                foreach (var record in group.Take(11))
                                {
                                    record.ActionTaken = "Yes";
                                }
                                elevenRecord.ActionTaken = "NO";
                            }
                            else
                            {
                                if (elevenRecord.ActionTaken == "Yes")
                                {
                                    foreach (var record in group.Take(11))
                                    {
                                        record.ActionTaken = "Yes";
                                    }
                                    elevenRecord.ActionTaken = "Yes";
                                }
                            }
                        }
                    }
                }

                if (violatedClaims.Count > 0)
                {
                    var responseList = violatedClaims.Select(c => new ScrubberReportResponse
                    {
                        Practice_Code = c.PracticeCode,
                        Practice_Name = c.PracticeName,
                        Rejection_Date = c.RejectionDate,
                        Account_Number = c.AccountNumber,
                        Patient_Name = c.PatientName,
                        Claim_Number = c.ClaimNumber,
                        Date_Of_Service = c.DateOfService,
                        Charge_amount = c.Chargeamount,
                        Rejection_Reason = c.RejectionReason,
                        Insurance_Name = c.InsuranceName,
                        Action_Taken = c.ActionTaken,
                    }).ToList();
                    res.Status = "Success";
                    res.Response = responseList;
                }
                else
                {
                    res.Status = "Error";
                    res.Response = null;
                }

                return res;
            }
        }
        //added by Sami Ullah
        public ResponseModel GetScrubberRejectionPagination(string practiceCode, DateTime Date_From, DateTime Date_To,int page,int size)
        {
            ResponseModel res = new ResponseModel();
            PagingResponse pagingResponse = new PagingResponse();

            using (var ctx = new NPMDBEntities())
            {
                var violatedClaims      = ctx.sp_GetScrubberRejectionReports(
                                            Convert.ToInt64(practiceCode),
                                            Date_From,
                                            Date_To
                                            ).OrderByDescending(c => c.RejectionDate)
                                                                    .Skip((page - 1) * size).Take(size).ToList();
               pagingResponse.TotalRecords = ctx.sp_GetScrubberRejectionReports(
                                            Convert.ToInt64(practiceCode),
                                            Date_From,
                                            Date_To
                                            ).Count();
                pagingResponse.FilteredRecords = violatedClaims.Count(); // Count after pagination
                pagingResponse.CurrentPage = page; ;
             
                if (violatedClaims.Count > 0)
                {
                    var groupedClaims = violatedClaims.GroupBy(c => c.ClaimNumber);
                    foreach (var group in groupedClaims)
                    {
                        if (group.Count() == 1)
                        {
                            var singleRecord = group.First();
                            singleRecord.ActionTaken = (singleRecord.ActionTaken == "Yes") ? "Yes" : "NO";
                        }
                        else if (group.Count() == 2)
                        {
                            var firstRecord = group.First();
                            var secondRecord = group.Skip(1).First();

                            if (firstRecord.ActionTaken == "NO" && secondRecord.ActionTaken == "NO")
                            {
                                firstRecord.ActionTaken = "Yes";
                                secondRecord.ActionTaken = "NO";
                            }
                            else
                            {
                                if (firstRecord.ActionTaken == "Yes" && secondRecord.ActionTaken == "Yes")
                                {
                                    firstRecord.ActionTaken = "Yes";
                                    secondRecord.ActionTaken = "Yes";
                                }
                                if (firstRecord.ActionTaken == "NO" && secondRecord.ActionTaken == "Yes")
                                {
                                    firstRecord.ActionTaken = "Yes";
                                    secondRecord.ActionTaken = "Yes";
                                }
                                else if (firstRecord.ActionTaken == "NO" && secondRecord.ActionTaken == "NO")
                                {
                                    firstRecord.ActionTaken = "Yes";
                                    secondRecord.ActionTaken = "NO";
                                }
                            }
                        }
                        else if (group.Count() > 2 && group.Count() < 4)
                        {
                            var thirdRecord = group.Skip(2).First();

                            if (thirdRecord.ActionTaken == "NO")
                            {
                                foreach (var record in group.Take(2))
                                {
                                    record.ActionTaken = "Yes";
                                }
                                thirdRecord.ActionTaken = "NO";
                            }
                            else
                            {
                                if (thirdRecord.ActionTaken == "Yes")
                                {
                                    foreach (var record in group.Take(2))
                                    {
                                        record.ActionTaken = "Yes";
                                    }
                                    thirdRecord.ActionTaken = "Yes";
                                }
                            }
                        }
                        else if (group.Count() > 3 && group.Count() < 5)
                        {
                            var fourthRecord = group.Skip(3).First();

                            if (fourthRecord.ActionTaken == "NO")
                            {
                                foreach (var record in group.Take(3))
                                {
                                    record.ActionTaken = "Yes";
                                }
                                fourthRecord.ActionTaken = "NO";
                            }
                            else
                            {
                                if (fourthRecord.ActionTaken == "Yes")
                                {
                                    foreach (var record in group.Take(3))
                                    {
                                        record.ActionTaken = "Yes";
                                    }
                                    fourthRecord.ActionTaken = "Yes";
                                }
                            }
                        }
                        else if (group.Count() > 4 && group.Count() < 6)

                        {
                            var FifthRecord = group.Skip(4).First();

                            if (FifthRecord.ActionTaken == "NO")
                            {
                                foreach (var record in group.Take(5))
                                {
                                    record.ActionTaken = "Yes";
                                }
                                FifthRecord.ActionTaken = "NO";
                            }
                            else
                            {
                                if (FifthRecord.ActionTaken == "Yes")
                                {
                                    foreach (var record in group.Take(5))
                                    {
                                        record.ActionTaken = "Yes";
                                    }
                                    FifthRecord.ActionTaken = "Yes";
                                }
                            }
                        }
                        else if (group.Count() > 5 && group.Count() < 7)
                        {
                            var sixthRecord = group.Skip(4).First();

                            if (sixthRecord.ActionTaken == "NO")
                            {
                                foreach (var record in group.Take(6))
                                {
                                    record.ActionTaken = "Yes";
                                }
                                sixthRecord.ActionTaken = "NO";
                            }
                            else
                            {
                                if (sixthRecord.ActionTaken == "Yes")
                                {
                                    foreach (var record in group.Take(6))
                                    {
                                        record.ActionTaken = "Yes";
                                    }
                                    sixthRecord.ActionTaken = "Yes";
                                }
                            }
                        }
                        else if (group.Count() > 6 && group.Count() < 8)
                        {
                            var sevenththRecord = group.Skip(7).First();

                            if (sevenththRecord.ActionTaken == "NO")
                            {
                                foreach (var record in group.Take(7))
                                {
                                    record.ActionTaken = "Yes";
                                }
                                sevenththRecord.ActionTaken = "NO";
                            }
                            else
                            {
                                if (sevenththRecord.ActionTaken == "Yes")
                                {
                                    foreach (var record in group.Take(7))
                                    {
                                        record.ActionTaken = "Yes";
                                    }
                                    sevenththRecord.ActionTaken = "Yes";
                                }
                            }
                        }
                        else if (group.Count() > 7 && group.Count() < 9)
                        {
                            var eightRecord = group.Skip(7).First();

                            if (eightRecord.ActionTaken == "NO")
                            {
                                foreach (var record in group.Take(8))
                                {
                                    record.ActionTaken = "Yes";
                                }
                                eightRecord.ActionTaken = "NO";
                            }
                            else
                            {
                                if (eightRecord.ActionTaken == "Yes")
                                {
                                    foreach (var record in group.Take(8))
                                    {
                                        record.ActionTaken = "Yes";
                                    }
                                    eightRecord.ActionTaken = "Yes";
                                }
                            }
                        }
                        else if (group.Count() > 8 && group.Count() < 10)
                        {
                            var ninthRecord = group.Skip(8).First();

                            if (ninthRecord.ActionTaken == "NO")
                            {
                                foreach (var record in group.Take(9))
                                {
                                    record.ActionTaken = "Yes";
                                }
                                ninthRecord.ActionTaken = "NO";
                            }
                            else
                            {
                                if (ninthRecord.ActionTaken == "Yes")
                                {
                                    foreach (var record in group.Take(9))
                                    {
                                        record.ActionTaken = "Yes";
                                    }
                                    ninthRecord.ActionTaken = "Yes";
                                }
                            }
                        }
                        else if (group.Count() > 9 && group.Count() < 11)
                        {
                            var tenthRecord = group.Skip(9).First();

                            if (tenthRecord.ActionTaken == "NO")
                            {
                                foreach (var record in group.Take(10))
                                {
                                    record.ActionTaken = "Yes";
                                }
                                tenthRecord.ActionTaken = "NO";
                            }
                            else
                            {
                                if (tenthRecord.ActionTaken == "Yes")
                                {
                                    foreach (var record in group.Take(10))
                                    {
                                        record.ActionTaken = "Yes";
                                    }
                                    tenthRecord.ActionTaken = "Yes";
                                }
                            }
                        }
                        else if (group.Count() > 10 && group.Count() < 12)
                        {
                            var elevenRecord = group.Skip(10).First();

                            if (elevenRecord.ActionTaken == "NO")
                            {
                                foreach (var record in group.Take(11))
                                {
                                    record.ActionTaken = "Yes";
                                }
                                elevenRecord.ActionTaken = "NO";
                            }
                            else
                            {
                                if (elevenRecord.ActionTaken == "Yes")
                                {
                                    foreach (var record in group.Take(11))
                                    {
                                        record.ActionTaken = "Yes";
                                    }
                                    elevenRecord.ActionTaken = "Yes";
                                }
                            }
                        }
                        else if (group.Count() > 11)
                        {
                            var elevenRecord = group.Skip(11).First();

                            if (elevenRecord.ActionTaken == "NO")
                            {
                                foreach (var record in group.Take(11))
                                {
                                    record.ActionTaken = "Yes";
                                }
                                elevenRecord.ActionTaken = "NO";
                            }
                            else
                            {
                                if (elevenRecord.ActionTaken == "Yes")
                                {
                                    foreach (var record in group.Take(11))
                                    {
                                        record.ActionTaken = "Yes";
                                    }
                                    elevenRecord.ActionTaken = "Yes";
                                }
                            }
                        }
                    }
                }

                if (violatedClaims.Count > 0)
                {
                    var responseList = violatedClaims.Select(c => new ScrubberReportResponse
                    {
                        Practice_Code = c.PracticeCode,
                        Practice_Name = c.PracticeName,
                        Rejection_Date = c.RejectionDate,
                        Account_Number = c.AccountNumber,
                        Patient_Name = c.PatientName,
                        Claim_Number = c.ClaimNumber,
                        Date_Of_Service = c.DateOfService,
                        Charge_amount = c.Chargeamount,
                        Rejection_Reason = c.RejectionReason,
                        Insurance_Name = c.InsuranceName,
                        Action_Taken = c.ActionTaken,
                    }).ToList();
                    pagingResponse.data = responseList;
                    res.Status = "Success";
                    res.Response = pagingResponse;
                }
                else
                {
                    res.Status = "Error";
                    res.Response = null;
                }

                return res;
            }
        }
        public ResponseModel GetScrubberRejectionDetail(string practiceCode, DateTime Date_From, DateTime Date_To)
        {
            ResponseModel res = new ResponseModel();

            using (var ctx = new NPMDBEntities())
            {
                var violatedClaims = ctx.sp_GetScrubberRejectionReportDetailes(
                    Convert.ToInt64(practiceCode),
                    Date_From,
                    Date_To
                ).ToList();

                if (violatedClaims.Count > 0)
                {
                    res.Status = "Success";
                    res.Response = violatedClaims;
                }
                else
                {
                    res.Status = "Error";
                    res.Response = null;
                }

                return res;
            }
        }
        #region ClaimSubmissionReport
        public ResponseModelForClaimSubmission GetClaimSubmissionReport(long practiceCode, DateTime Date_From, DateTime Date_To, string Status)
        {
            ResponseModelForClaimSubmission objResponse = new ResponseModelForClaimSubmission();
            string storedProcedureName = "SP_Global277CAReport";
            using (var ctx = new NPMDBEntities())
            {
                List<ClaimSubmission> obj = new List<ClaimSubmission>();
                var adoConnection = ctx.Database.Connection as SqlConnection;
                if (adoConnection != null)
                {
                    using (IDbConnection connection = adoConnection)
                    {
                        var parameters = new DynamicParameters();
                        parameters.Add("@practiceCode", practiceCode);
                        parameters.Add("@Date_From", Date_From);
                        parameters.Add("@Date_To", Date_To);
                        parameters.Add("@Status", Status);
                        connection.Open();
                        var command = connection.CreateCommand();
                        command.CommandTimeout = 120;
                        objResponse.obj = connection.Query<ClaimSubmission>(storedProcedureName, parameters, commandTimeout: 120, commandType: CommandType.StoredProcedure).OrderByDescending(c => c.DOS).ToList();
                        if (Status == "PayerRejected")
                        {
                            objResponse.obj = objResponse.obj.Where(i => i.statusLevel == "Payer" && i.Status == "Rejected").ToList();
                        }
                        if (Status == "PayerAccepted")
                        {
                            objResponse.obj = objResponse.obj.Where(i => i.statusLevel == "Payer" && i.Status == "Accepted").ToList();
                        }
                        var responseList = objResponse.obj.Select(c => new ClaimSubmissionResponse
                        {
                           Practice_Code = c.Practice_Code,
                           Practice_Name =c.PracticeName,
                           Claim_No=c.ClaimNo,
                           Account_No=c.AccountNo,
                           Patient_Name=c.PatientName,
                           DOS=c.DOS,
                           Status_Date=c.StatusDate,
                           Submission_Date=c.SubmitDate,
                           Charge_amount=c.Chargeamount,
                           Insurance_Name=c.InsuranceName,
                           response_Level=c.statusLevel,
                           Status=c.Status,
                           Code=c.RejectionCode,
                           Rejection_reason=c.message,
                           File_name=c.File277CA,
                        }).ToList();
                        objResponse.Response = responseList;
                        objResponse.Status = "Success";
                    }
                }
                return objResponse;
            }
        }
        //added by Sami Ullah
        public ResponseModelForClaimSubmission GetClaimSubmissionReportPagination(long practiceCode, DateTime Date_From, DateTime Date_To, string Status,int page,int size)
        {
            ResponseModelForClaimSubmission objResponse = new ResponseModelForClaimSubmission();
            PagingResponse pagingResponse = new PagingResponse();
            string storedProcedureName = "SP_Global277CAReport";
            using (var ctx = new NPMDBEntities())
            {
                List<ClaimSubmission> obj = new List<ClaimSubmission>();
                var adoConnection = ctx.Database.Connection as SqlConnection;
                if (adoConnection != null)
                {
                    using (IDbConnection connection = adoConnection)
                    {
                        var parameters = new DynamicParameters();
                        parameters.Add("@practiceCode", practiceCode);
                        parameters.Add("@Date_From", Date_From);
                        parameters.Add("@Date_To", Date_To);
                        parameters.Add("@Status", Status);
                        connection.Open();
                        var command = connection.CreateCommand();
                        command.CommandTimeout = 120;
                        objResponse.obj = connection.Query<ClaimSubmission>(storedProcedureName, parameters, commandTimeout: 120, commandType: CommandType.StoredProcedure).
                            OrderByDescending(c => c.DOS)
                                                                    .Skip((page - 1) * size).Take(size).ToList();
                        pagingResponse.TotalRecords = connection.Query<ClaimSubmission>(storedProcedureName, parameters, commandTimeout: 120, commandType: CommandType.StoredProcedure).Count();
                        pagingResponse.FilteredRecords = objResponse.obj.Count(); // Count after pagination
                        pagingResponse.CurrentPage = page;


                        if (Status == "PayerRejected")
                        {
                            objResponse.obj = objResponse.obj.Where(i => i.statusLevel == "Payer" && i.Status == "Rejected").ToList();
                        }
                        if (Status == "PayerAccepted")
                        {
                            objResponse.obj = objResponse.obj.Where(i => i.statusLevel == "Payer" && i.Status == "Accepted").ToList();
                        }
                        var responseList = objResponse.obj.Select(c => new ClaimSubmissionResponse
                        {
                            Practice_Code = c.Practice_Code,
                            Practice_Name = c.PracticeName,
                            Claim_No = c.ClaimNo,
                            Account_No = c.AccountNo,
                            Patient_Name = c.PatientName,
                            DOS = c.DOS,
                            Status_Date = c.StatusDate,
                            Submission_Date = c.SubmitDate,
                            Charge_amount = c.Chargeamount,
                            Insurance_Name = c.InsuranceName,
                            response_Level = c.statusLevel,
                            Status = c.Status,
                            Code = c.RejectionCode,
                            Rejection_reason = c.message,
                            File_name = c.File277CA,
                        }).ToList();
                        pagingResponse.data= responseList;
                        objResponse.Response = pagingResponse;
                        objResponse.Status = "Success";
                    }
                }
                return objResponse;
            }
        }
        public ResponseModelForEdiHistory GetEdiHistory(long ClaimNo, string Status, string statusLevel, string File277CA)
        {
            if (statusLevel == "Clearing house")
            {
                statusLevel = "AY";
            }
            else if (statusLevel == "Payer")
            {
                statusLevel = "PR";
            }
            ResponseModelForEdiHistory objResponse = new ResponseModelForEdiHistory();
            string storedProcedureName = "[SP_EDIHistory]";
            using (var ctx = new NPMDBEntities())
            {
                List<EdiHistoryModel> obj = new List<EdiHistoryModel>();
                var adoConnection = ctx.Database.Connection as SqlConnection;
                if (adoConnection != null)
                {
                    using (IDbConnection connection = adoConnection)
                    {
                        var parameters = new DynamicParameters();
                        parameters.Add("@claimNo", ClaimNo);
                        parameters.Add("@Status", Status);
                        parameters.Add("@File277CA", File277CA);
                        parameters.Add("@statusLevel", statusLevel);
                        connection.Open();
                        var command = connection.CreateCommand();
                        command.CommandTimeout = 60;
                        objResponse.obj = connection.Query<EdiHistoryModel>(storedProcedureName, parameters, commandType: System.Data.CommandType.StoredProcedure).ToList();
                        objResponse.Status = "Success";
                    }
                }
                return objResponse;
            }
        }
        public ResponseModelforClaimRejection GetClaimRejections(long practiceCode)
        {
            ResponseModelforClaimRejection objResponse = new ResponseModelforClaimRejection();
            string storedProcedureName = "Sp_ClaimRejection";

            using (var ctx = new NPMDBEntities())
            {
                List<ClaimRejectionModel> obj = new List<ClaimRejectionModel>();
                var adoConnection = ctx.Database.Connection as SqlConnection;

                if (adoConnection != null)
                {
                    using (IDbConnection connection = adoConnection)
                    {
                        var parameters = new DynamicParameters();
                        parameters.Add("@practiceCode", practiceCode);
                        var command = connection.CreateCommand();
                        connection.Open();
                        command.CommandTimeout = 120;
                        obj = connection.Query<ClaimRejectionModel>(storedProcedureName, parameters, commandType: System.Data.CommandType.StoredProcedure).ToList();
                        obj = obj.Where(item => item.date_created < item.StatusDate).ToList();
                        objResponse.obj = obj;
                        objResponse.Status = "Success";
                    }
                }

                return objResponse;
            }
        }
        #endregion
    }
}
