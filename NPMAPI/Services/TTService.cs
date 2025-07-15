using NPMAPI.Models;
using NPMAPI.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NPMAPI.Services
{
    public class TTService : ITicketTrackerRrepo
    {
        public ResponseModel GetClaimInfo(string claimno)
        {
            using (var ctx = new NPMDBEntities())
            {
                try
                {
                    long claimnumber=  Convert.ToInt64(claimno);
                    var claim = ctx.Claims
                        .FirstOrDefault(p => p.Claim_No == claimnumber);

                    if (claim == null)
                    {
                        return new ResponseModel
                        {
                            Status = "Error",
                            Response = "Claim not found"
                        };
                    }

                    // Step 2: Fetch related insurance records
                    var insurances = (
                           from ci in ctx.Claim_Insurance
                           join ins in ctx.Insurances on ci.Insurance_Id equals ins.Insurance_Id
                           join payer in ctx.Insurance_Payers on ins.InsPayer_Id equals payer.Inspayer_Id
                           where ci.Claim_No == claimnumber
                           select new
                           {
                               ci.Pri_Sec_Oth_Type,
                               ci.Policy_Number,
                               payer.Inspayer_Id,
                               payer.Inspayer_Description
                           }).ToList();


                    // Step 3: Package the response
                    var responseData = new
                    {
                        Claim = new
                        {
                            claim.Claim_No,
                            claim.Amt_Due,
                            claim.DOS,
                            claim.Claim_Total,
                            claim.Attending_Physician,
                            claim.Billing_Physician,

                        },
                        Insurances = insurances
                    };

                    return new ResponseModel
                    {
                        Status = "Success",
                        Response = responseData
                    };
                }
                catch (Exception ex)
                {
                    return new ResponseModel
                    {
                        Status = "Error",
                        Response = ex.Message
                    };
                }
            }

        }

        public ResponseModel GetPatientAccounts(long practicecode)
        {
            using (var ctx = new NPMDBEntities())
            {
                try
                {
                    var Pracinfo = (from p in ctx.Patients
                                    where p.Practice_Code == practicecode && p.Deleted != true
                                    select new
                                    {
                                        p.Patient_Account,
                                    }).ToList();

                    return new ResponseModel
                    {
                        Status = "Success",
                        Response = Pracinfo
                    };
                }
                catch (Exception ex)
                {
                    return new ResponseModel
                    {
                        Status = "Error",
                        Response = ex.Message
                    };
                }
            }
        }

        public ResponseModel GetPatientDueAmt(string patientacc)
        {
            var response = new ResponseModel();

            try
            {
                long accountNumber = Convert.ToInt64(patientacc);
                using (var ctx = new NPMDBEntities())
                {
                    // Call the stored procedure (assuming it's imported correctly in the EDMX)
                    var result = ctx.SP_CLAIMSUMMARYAMOUNTS(accountNumber).FirstOrDefault();

                    if (result != null)
                    {
                        response.Status = "Success";
                        response.Response = result; // You can also project specific fields if needed
                    }
                    else
                    {
                        response.Status = "NotFound";
                        response.Response = null;
                    }
                }
            }
            catch (Exception ex)
            {
                response.Status = "Error";
                response.Response = ex.Message;
            }

            return response;
        }


        public ResponseModel GetPatientInfo(string patientacc)
        {
            using (var ctx = new NPMDBEntities())
            {
                try
                {
                    long accountNumber = Convert.ToInt64(patientacc);
                    var patient = ctx.Patients
                        .FirstOrDefault(p => p.Patient_Account == accountNumber);

                    if (patient == null)
                    {
                        return new ResponseModel
                        {
                            Status = "Error",
                            Response = "Patient not found"
                        };
                    }
                    var claims = ctx.Claims
                        .Where(c => c.Patient_Account == accountNumber)
                        .Select(c => new
                        {
                            c.Claim_No,
                        })
                        .ToList();

                    var responseData = new
                    {
                        Patient = new
                        {
                            patient.First_Name,
                            patient.MI,
                            patient.Last_Name,
                            patient.Cell_Phone,
                            patient.Home_Phone,
                            patient.Date_Of_Birth,
                            patient.SSN,
                        },
                        Claims = claims
                    };

                    return new ResponseModel
                    {
                        Status = "Success",
                        Response = responseData
                    };
                }
                catch (Exception ex)
                {
                    return new ResponseModel
                    {
                        Status = "Error",
                        Response = ex.Message
                    };
                }
            }
        }


        public ResponseModel GetPracticeInfo(long practicecode)
        {
            using (var ctx = new NPMDBEntities())
            {
                try
                {
                    var Pracinfo = (from p in ctx.Practices
                                    where p.Practice_Code == practicecode && p.Deleted != true
                                    join pr in ctx.Practice_Reporting on p.Practice_Code equals pr.Practice_Code into prJoin
                                    from pr in prJoin.DefaultIfEmpty()
                                    join d in ctx.Divisions on pr.Division_ID equals d.Division_ID into dJoin
                                    from d in dJoin.DefaultIfEmpty()
                                    select new
                                    {
                                        p.Practice_Code,
                                        p.Prac_Address,
                                        p.Prac_Phone,
                                        p.Prac_Alternate_Phone,
                                        p.NPI,
                                        p.Prac_Tax_Id,
                                        p.TAXONOMY_CODE,
                                        p.Prac_Soft,
                                        DivisionId = pr.Division_ID,
                                        DivisionName = d.Division_Name
                                    }).ToList();

                    return new ResponseModel
                    {
                        Status = "Success",
                        Response = Pracinfo
                    };
                }
                catch (Exception ex)
                {
                    return new ResponseModel
                    {
                        Status = "Error",
                        Response = ex.Message
                    };
                }
            }
        }

        public ResponseModel GetRenderingProvider(long providercode)
        {
            using (var ctx = new NPMDBEntities())
            {
                try
                {
                    var renderingprovider = ctx.Providers
                    .Where(p => p.Provider_Code == providercode)
                    .ToList()
                    .Select(p => new Provider
                    {
                        Provid_FName = p.Provid_FName,
                        Provid_LName = p.Provid_LName,
                        ADDRESS = p.ADDRESS,
                        NPI = p.NPI,
                        group_npi = p.group_npi,
                        SSN = p.SSN,
                        Taxonomy_Code = p.Taxonomy_Code,
                        Phone_One = p.Phone_One
                    }).ToList();

                    return new ResponseModel
                    {
                        Status = "Success",
                        Response = renderingprovider
                    };
                }
                catch (Exception ex)
                {
                    return new ResponseModel
                    {
                        Status = "Error",
                        Response = ex.Message
                    };
                }
            }
        }

        public ResponseModel GetTTDepartmentsList()
        {
            using (var ctx = new NPMDBEntities())
            {
                try
                {
                    var departments = ctx.Departments
                        .Where(d => d.EnableTicket == true && (d.Deleted != true || d.Deleted == null))
                        .Select(d => new DepartmentViewModel
                        {
                            DepartmentId = d.DepartmentId,
                            DepartmentName = d.DepartmentName,
                            OfficeId = d.OfficeId,
                            CompanyId = d.CompanyId
                        }).ToList();

                    return new ResponseModel
                    {
                        Status = "Success",
                        Response = departments
                    };
                }
                catch (Exception ex)
                {
                    return new ResponseModel
                    {
                        Status = "Error",
                        Response = ex.Message
                    };
                }
            }
        }

        public ResponseModel SaveTicket(Tickettracker model)
        {
            var response = new ResponseModel();

            using (var ctx = new NPMDBEntities())
            using (var transaction = ctx.Database.BeginTransaction())
            {
                try
                {
                    if (model.Ticket_Id != 0) 
                    {
                        var existingTicket = ctx.Tickets.FirstOrDefault(t => t.Ticket_Id == model.Ticket_Id && t.Deleted != true);
                        if (existingTicket != null)
                        {
                            existingTicket.Department_Id = model.Department_Id;
                            existingTicket.Assigned_User = Convert.ToInt64(model.Assigned_User);
                            existingTicket.Ticket_Type = model.Ticket_Type;
                            existingTicket.Ticket_Reason = model.Ticket_Reason;
                            existingTicket.Ticket_Priority = model.Ticket_Priority;
                            existingTicket.Ticket_Title = model.Ticket_Title;
                            existingTicket.Ticket_Status = model.Ticket_Status;
                            existingTicket.Ticket_Aging = model.Ticket_Aging;
                            existingTicket.Closing_Remarks = model.Closing_Remarks;
                            existingTicket.Practice_Code = Convert.ToInt64(model.Practice_Code);
                            existingTicket.RProvider_Id = model.RProvider_Id;
                            existingTicket.BProvider_Id = model.BProvider_Id;
                            existingTicket.Modified_By = Convert.ToInt64(model.Created_By);
                            existingTicket.Modified_Date = DateTime.Now;
                            existingTicket.Deleted = false;
                            ctx.SaveChanges();
                        }
                        var existingclaim = ctx.Ticket_Patient_Claims_Info.FirstOrDefault(c => c.Ticket_Id == model.Ticket_Id && c.Deleted != true);
                        if (existingclaim!=null)
                        {

                            existingclaim.Ticket_Id = model.Ticket_Id;
                            existingclaim.Patient_Account = model.Patient_Account;
                            existingclaim.Claim_No =model.Claim_No;
                            existingclaim.First_Name = model.First_Name;
                            existingclaim.Last_Name = model.Last_Name;
                            existingclaim.MI = model.MI;
                            existingclaim.PatientCell_Phone = model.PatientCell_Phone;
                            existingclaim.PatientHome_Phone = model.PatientHome_Phone;
                            existingclaim.Date_Of_Birth = model.Date_Of_Birth;
                            existingclaim.SSN = model.SSN;
                            existingclaim.DOS = model.DOS;
                            existingclaim.Total_Billed = model.Total_Billed;
                            existingclaim.Patient_Due = model.Patient_Due;
                            existingclaim.Claim_Due = model.Claim_Due;
                            existingclaim.Ins_Mode = model.Ins_Mode;
                            existingclaim.Payer_Name = model.Payer_Name;
                            existingclaim.Payer_Id = model.Payer_Id;
                            existingclaim.Policy_No = model.Policy_No;
                            existingclaim.Created_By = null;
                            existingclaim.Created_Date = null ;
                            existingclaim.Modified_By = Convert.ToInt64(model.Created_By);
                            existingclaim.Modified_Date = DateTime.Now;
                            existingclaim.Deleted = false;
                            ctx.SaveChanges();

                        }
                        var existingtracker = ctx.Ticket_Tracking.FirstOrDefault(t => t.Ticket_Id == model.Ticket_Id && t.Deleted != true);

                        if (existingtracker != null)
                        {
                            existingtracker.Ticket_Id = model.Ticket_Id;
                            existingtracker.Ticket_Status = model.Ticket_Status;
                            existingtracker.Ticket_Message = model.Ticket_Message;
                            existingtracker.Department_Id = model.Department_Id;
                            existingtracker.Assigned_User = Convert.ToInt64(model.Assigned_User);
                            existingtracker.Created_By = Convert.ToInt64(model.Created_By);
                            existingtracker.Created_Date = DateTime.Now;
                            existingtracker.Modified_By = null;
                            existingtracker.Modified_Date = null;
                            existingtracker.Deleted = false;

                            ctx.SaveChanges();
                        }

                        response.Status = "Success";
                        response.Response = "Ticket Updated successfully.";

                    }
                    else
                    {
                        // 1. Insert into Ticket
                        var ticket = new Ticket
                        {
                            Ticket_Id = Convert.ToInt64(ctx.SP_TableIdGenerator("Ticket_Id").FirstOrDefault().ToString()),
                            Department_Id = model.Department_Id,
                            Ticket_Type = model.Ticket_Type,
                            Ticket_Reason = model.Ticket_Reason,
                            Ticket_Priority = model.Ticket_Priority,
                            Ticket_Title = model.Ticket_Title,
                            Ticket_Status = "New",
                            Ticket_Aging = model.Ticket_Aging,
                            Closing_Remarks = model.Closing_Remarks,
                            Practice_Code = Convert.ToInt64(model.Practice_Code),
                            RProvider_Id = model.RProvider_Id,
                            BProvider_Id = model.BProvider_Id,
                            Assigned_User = Convert.ToInt64(model.Assigned_User),
                            Created_By = Convert.ToInt64(model.Created_By),
                            Created_Date = DateTime.Now,
                            Modified_By = null,
                            Modified_Date = null,
                            Deleted = false
                        };

                        ctx.Tickets.Add(ticket);
                        ctx.SaveChanges();

                        // 2. Insert into Ticket_Patient_Claims_Info
                        var claim = new Ticket_Patient_Claims_Info
                        {
                            Ticket_Id = ticket.Ticket_Id,
                            Patient_Account =model.Patient_Account,
                            Claim_No = model.Claim_No,
                            First_Name = model.First_Name,
                            Last_Name = model.Last_Name,
                            MI = model.MI,
                            PatientCell_Phone = model.PatientCell_Phone,
                            PatientHome_Phone = model.PatientHome_Phone,
                            Date_Of_Birth = model.Date_Of_Birth,
                            SSN = model.SSN,
                            DOS = model.DOS,
                            Total_Billed = model.Total_Billed,
                            Patient_Due = model.Patient_Due,
                            Claim_Due = model.Claim_Due,
                            Ins_Mode = model.Ins_Mode,
                            Payer_Name = model.Payer_Name,
                            Payer_Id = model.Payer_Id,
                            Policy_No = model.Policy_No,
                            Created_By = Convert.ToInt64(model.Created_By),
                            Created_Date = DateTime.Now,
                            Modified_By = null,
                            Modified_Date = null,
                            Deleted = false
                        };

                        ctx.Ticket_Patient_Claims_Info.Add(claim);
                        ctx.SaveChanges();

                        // 3. Insert into Ticket_Tracking
                        var track = new Ticket_Tracking
                        {
                            Ticket_Id = ticket.Ticket_Id,
                            Ticket_Status = "New",
                            Ticket_Message = model.Ticket_Message,
                            Department_Id = model.Department_Id,
                            Assigned_User = Convert.ToInt64(model.Assigned_User),
                            Created_By = Convert.ToInt64(model.Created_By),
                            Created_Date = DateTime.Now,
                            Modified_By = null,
                            Modified_Date = null,
                            Deleted = false
                        };

                        ctx.Ticket_Tracking.Add(track);
                        ctx.SaveChanges();
                        response.Status = "Success";
                        response.Response = "Ticket Created Successfully.";
                    }
                    transaction.Commit();

                    
                    return response;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();

                    response.Status = "Error";
                    response.Response = ex.Message;
                    return response;
                    throw ex;
                }
            }
        }

        public ResponseModel SearchTicket(TicketSearchModel model)
        {
            try
            {
        
                using (var ctx = new NPMDBEntities())
                {
                    var searchresult = ctx.usp_GetTicketsWithFilters(
                        model.Ticket_Id == 0 ? null : model.Ticket_Id,
                        model.Ticket_Type,
                        model.Ticket_Reason,
                        model.Ticket_Priority,
                        model.Practice_Code == 0 ? null : model.Practice_Code,
                        model.Ticket_Status,
                        model.Department_Id == 0 ? null : model.Department_Id,
                        model.Created_By == 0 ? null : model.Created_By,
                        model.Assigned_User == 0 ? null : model.Assigned_User,
                        model.Payer_Name,
                        model.Claim_No,
                        model.DateFrom,
                        model.DateTo
                    ).ToList();

                    return new ResponseModel
                    {
                        Status = "Success",
                        Response = searchresult
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel
                {
                    Status = "Error",
                    Response = ex.Message
                };
            }
        }

        public ResponseModel TicketAssignment(TicketAssignmentRequest model)
        {
            using (var db = new NPMDBEntities())
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var ticketIds = model.TicketIds;
                    var userId = model.UserId;
                    var currentuser = model.CurrentUser;

                    // Update AssignedUserId in Tickets table
                    var tickets = db.Tickets.Where(t => ticketIds.Contains((int)t.Ticket_Id)).ToList();
                    foreach (var ticket in tickets)
                    {
                        ticket.Assigned_User = userId;
                        ticket.Modified_By = currentuser;
                        ticket.Modified_Date = DateTime.Now;
                    }

                    // Update AssignedUserId in Ticket_Tracking table
                    var ticketTrails = db.Ticket_Tracking.Where(tt => ticketIds.Contains((int)tt.Ticket_Id)).ToList();
                    foreach (var trail in ticketTrails)
                    {
                        trail.Assigned_User = userId;
                        trail.Modified_By = currentuser;
                        trail.Modified_Date = DateTime.Now;
                    }

                    db.SaveChanges();
                    transaction.Commit();

                    return new ResponseModel
                    {
                        Status = "Success",
                        Response = "Tickets successfully assigned"
                    };
                }
                catch (Exception ex)
                {
                    transaction.Rollback();

                    return new ResponseModel
                    {
                        Status = "Error",
                        Response = $"Assignment failed: {ex.Message}"
                    };
                }
            }
        }

        public ResponseModel GetTicketById(long ticketid)
        {
            using (var db = new NPMDBEntities())
            {
                var ticketId = ticketid;

                // Get from Tickets
                var ticket = db.Tickets.FirstOrDefault(t => t.Ticket_Id == ticketId);
                if (ticket == null)
                {
                    return new ResponseModel
                    {
                        Status = "Error",
                        Response = "Ticket not found"
                    };
                }
                var checkSoft = db.Practices.Where(p => p.Practice_Code ==ticket.Practice_Code ).Select(p => p.Prac_Soft).FirstOrDefault();
        

                // Get latest entry from Ticket_Tracking
                var tracking = db.Ticket_Tracking
                                 .Where(t => t.Ticket_Id == ticketId)
                                 .OrderByDescending(t => t.Modified_Date)
                                 .FirstOrDefault();

                // Get from Ticket_Patient_Claims_Info
                var claimInfo = db.Ticket_Patient_Claims_Info
                                  .FirstOrDefault(p => p.Ticket_Id == ticketId);

                var response = new Tickettracker
                {
                    Ticket_Id = (int)ticket.Ticket_Id,
                    Department_Id = (int)ticket.Department_Id, // Assuming non-nullable int
                    Ticket_Type = ticket.Ticket_Type ?? "",
                    Ticket_Reason = ticket.Ticket_Reason ?? "",
                    Ticket_Priority = ticket.Ticket_Priority ?? "",
                    Ticket_Title = ticket.Ticket_Title ?? "",
                    Ticket_Status = ticket.Ticket_Status ?? "",
                    Ticket_Aging = ticket.Ticket_Aging ?? 0,
                    Closing_Remarks = ticket.Closing_Remarks ?? "",
                    Practice_Code = ticket.Practice_Code?.ToString() ?? "",
                    RProvider_Id = ticket.RProvider_Id,
                    BProvider_Id = ticket.BProvider_Id,
                    Assigned_User = ticket.Assigned_User?.ToString() ?? "",
                    Created_By = ticket.Created_By?.ToString() ?? "",

                    // Patient Claims Info
                    Patient_Account = claimInfo?.Patient_Account?.ToString() ?? "",
                    Claim_No = claimInfo?.Claim_No?.ToString() ?? "",
                    First_Name = claimInfo?.First_Name ?? "",
                    Last_Name = claimInfo?.Last_Name ?? "",
                    MI = claimInfo?.MI ?? "",
                    PatientCell_Phone = claimInfo?.PatientCell_Phone ?? "",
                    PatientHome_Phone = claimInfo?.PatientHome_Phone ?? "",
                    Date_Of_Birth = claimInfo?.Date_Of_Birth,
                    SSN = claimInfo?.SSN ?? "",
                    DOS = claimInfo?.DOS,
                    Total_Billed = claimInfo?.Total_Billed ?? 0,
                    Patient_Due = claimInfo?.Patient_Due ?? 0,
                    Claim_Due = claimInfo?.Claim_Due ?? 0,
                    Ins_Mode = claimInfo?.Ins_Mode ?? "",
                    Payer_Name = claimInfo?.Payer_Name ?? "",
                    Payer_Id = claimInfo?.Payer_Id ?? "",
                    Policy_No = claimInfo?.Policy_No ?? "",
                    

                    // Ticket Tracking (latest message)
                    Ticket_Message = tracking?.Ticket_Message ?? ""
                };
                if (checkSoft == "OtherSoft")
                {
                    response.Soft = true;
                }
                return new ResponseModel
                {
                    Status = "Success",
                    Response = response
                };
            }
        }

        public ResponseModel GetProviderList(long practicecode)
        {
            using (var db = new NPMDBEntities())
            {
                try
                {
                    var providers = db.Providers.Where(p => p.Practice_Code == practicecode && p.Deleted != true).ToList();
                    return new ResponseModel
                    {
                        Status = "Success",
                        Response = providers
                    };
                }
                catch (Exception ex)
                {
                    return new ResponseModel
                    {
                        Status = "Error",
                        Response = ex.Message
                    };
        
                }
            }
        }

        public ResponseModel GetTicketTrackById(long ticketid)
        {
            using (var db = new NPMDBEntities())
            {
                var ticket = db.Tickets.FirstOrDefault(t => t.Ticket_Id == ticketid);
                if (ticket == null)
                {
                    return new ResponseModel
                    {
                        Status = "Error",
                        Response = "Ticket not found"
                    };
                }

                var checkSoft = db.Practices
                    .Where(p => p.Practice_Code == ticket.Practice_Code)
                    .Select(p => p.Prac_Soft)
                    .FirstOrDefault();

                // 🔹 Get related data
                var departmentName = db.Departments
                    .Where(d => d.DepartmentId == ticket.Department_Id)
                    .Select(d => d.DepartmentName)
                    .FirstOrDefault();

                var practiceName = db.Practices
                    .Where(p => p.Practice_Code == ticket.Practice_Code)
                    .Select(p => p.Prac_Name)
                    .FirstOrDefault();

                var createdByName = db.Users
                    .Where(u => u.UserId == ticket.Created_By)
                    .Select(u => u.LastName + u.FirstName)
                    .FirstOrDefault();

                var assignedUserName = db.Users
                    .Where(u => u.UserId == ticket.Assigned_User)
                    .Select(u => u.LastName + ", " + u.FirstName)
                    .FirstOrDefault();

                //var tracking = db.Ticket_Tracking
                //    .Where(t => t.Ticket_Id == ticketid)
                //    .OrderByDescending(t => t.Modified_Date)
                //    .FirstOrDefault();
                var trackingList = db.Ticket_Tracking
               .Where(t => t.Ticket_Id == ticketid && (t.Deleted == false || t.Deleted == null))
               .OrderByDescending(t => t.Created_Date)
               .ToList();

                var messageList = trackingList.Select(t => new TicketMessageDetail
                {
                    Trail_id = t.Ticket_Trail_Id,
                    Ticket_Message = t.Ticket_Message ?? "",
                    Ticket_Status = t.Ticket_Status ?? "",
                    Created_Date = t.Created_Date,
                    Created_By_Name = db.Users
                    .Where(u => u.UserId == t.Created_By)
                    .Select(u => u.FirstName + " " + u.LastName)
                    .FirstOrDefault() ?? ""
                }).ToList();

                var claimInfo = db.Ticket_Patient_Claims_Info
                    .FirstOrDefault(p => p.Ticket_Id == ticketid);

                var response = new GetTicketTrackDetail
                {
                    Ticket_Id = (int)ticket.Ticket_Id,
                    Department_Id = (int)ticket.Department_Id,
                    Ticket_Type = ticket.Ticket_Type ?? "",
                    Ticket_Reason = ticket.Ticket_Reason ?? "",
                    Ticket_Priority = ticket.Ticket_Priority ?? "",
                    Ticket_Title = ticket.Ticket_Title ?? "",
                    Ticket_Status = ticket.Ticket_Status ?? "",
                    Ticket_Aging = ticket.Ticket_Aging ?? 0,
                    Closing_Remarks = ticket.Closing_Remarks ?? "",
                    Practice_Code = ticket.Practice_Code?.ToString() ?? "",
                    RProvider_Id = ticket.RProvider_Id,
                    BProvider_Id = ticket.BProvider_Id,
                    Assigned_User = ticket.Assigned_User?.ToString() ?? "",
                    Created_By = ticket.Created_By?.ToString() ?? "",

                    // Added mapped names
                    DepartmentName = departmentName ?? "",
                    Practice_Name = practiceName ?? "",
                    CreatedByName = createdByName ?? "",
                    AssignedUserName = assignedUserName ?? "",

                    // Patient Claims Info
                    Patient_Account = claimInfo?.Patient_Account?.ToString() ?? "",
                    Claim_No = claimInfo?.Claim_No?.ToString() ?? "",
                    First_Name = claimInfo?.First_Name ?? "",
                    Last_Name = claimInfo?.Last_Name ?? "",
                    MI = claimInfo?.MI ?? "",
                    PatientCell_Phone = claimInfo?.PatientCell_Phone ?? "",
                    PatientHome_Phone = claimInfo?.PatientHome_Phone ?? "",
                    Date_Of_Birth = claimInfo?.Date_Of_Birth,
                    SSN = claimInfo?.SSN ?? "",
                    DOS = claimInfo?.DOS,
                    Total_Billed = claimInfo?.Total_Billed ?? 0,
                    Patient_Due = claimInfo?.Patient_Due ?? 0,
                    Claim_Due = claimInfo?.Claim_Due ?? 0,
                    Ins_Mode = claimInfo?.Ins_Mode ?? "",
                    Payer_Name = claimInfo?.Payer_Name ?? "",
                    Payer_Id = claimInfo?.Payer_Id ?? "",
                    Policy_No = claimInfo?.Policy_No ?? "",

                    // Ticket Tracking (latest message)
                    TicketMessages = messageList
                };

                // Soft flag
                if (checkSoft == "OtherSoft")
                {
                    response.Soft = true;
                }

                return new ResponseModel
                {
                    Status = "Success",
                    Response = response
                };
            }
        }

        public ResponseModel SaveTrackDetails(SaveTrackDetails model)
        {
            var response = new ResponseModel();

            using (var ctx = new NPMDBEntities())
            using (var transaction = ctx.Database.BeginTransaction())
            {
                try
                {
                    // 1. Find the existing ticket to update
                    var ticket = ctx.Tickets.FirstOrDefault(t => t.Ticket_Id == model.Ticket_Id);

                    if (ticket == null)
                    {
                        response.Status = "Error";
                        response.Response = "Ticket not found.";
                        return response;
                    }

                    // 2. Update Ticket
                    ticket.Department_Id = model.Department_Id;
                    ticket.Ticket_Status = model.Ticket_Status;
                    ticket.Assigned_User = Convert.ToInt64(model.Assigned_User);
                    ticket.Closing_Remarks = model.Closing_Remarks;

                    // 3. Insert into Ticket_Tracking
                    var track = new Ticket_Tracking
                    {
                        Ticket_Id = model.Ticket_Id,
                        Ticket_Status = model.Ticket_Status,
                        Ticket_Message = model.Ticket_Message,
                        Department_Id = model.Department_Id,
                        Assigned_User = Convert.ToInt64(model.Assigned_User),
                        Created_By = Convert.ToInt64(model.Created_By),
                        Created_Date = DateTime.Now,
                        Modified_By = null,
                        Modified_Date = null,
                        Deleted = false
                    };

                    ctx.Ticket_Tracking.Add(track);

                    // 4. Save both updates
                    ctx.SaveChanges();

                    // 5. Commit transaction
                    transaction.Commit();

                    response.Status = "Success";
                    response.Response = "Ticket Response Added Successfully.";
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    response.Status = "Error";
                    response.Response = ex.Message;
                }
            }

            return response;
        }

        public ResponseModel EditTrackDetails(TicketMessageDetail model)
        {
            var response = new ResponseModel();

            using (var ctx = new NPMDBEntities())
            using (var transaction = ctx.Database.BeginTransaction())
            {
                try
                {
                    var trailid = ctx.Ticket_Tracking.FirstOrDefault(t => t.Ticket_Trail_Id == model.Trail_id && t.Deleted != true);
                    if (trailid != null)
                    {
                        trailid.Ticket_Message = model.Ticket_Message;
                        trailid.Ticket_Status = model.Ticket_Status;
                        trailid.Assigned_User = Convert.ToInt64( model.AssignedUser);
                        trailid.Department_Id = Convert.ToInt64(model.AssignedDep);
                        trailid.Created_By = Convert.ToInt64(model.Created_By);
                        trailid.Created_Date = DateTime.Now;
                        ctx.SaveChanges();
                    }
                    var ticket = ctx.Tickets.FirstOrDefault(t => t.Ticket_Id == model.Ticket_Id && t.Deleted != true);
                    if (ticket !=null)
                    {
                        ticket.Ticket_Status = model.Ticket_Status;
                        ticket.Department_Id = Convert.ToInt64(model.AssignedDep);
                        ticket.Assigned_User = Convert.ToInt64(model.AssignedUser);
                        ticket.Closing_Remarks = model.Closing_Remarks;
                        ctx.SaveChanges();
                    }
                    // 5. Commit transaction
                    transaction.Commit();

                    response.Status = "Success";
                    response.Response = "Ticket Response Updated Successfully.";
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    response.Status = "Error";
                    response.Response = ex.Message;
                }
            }

            return response;
        }
        public ResponseModel DeleteTrackDetails(TicketMessageDetail model)
        {
            var response = new ResponseModel();

            using (var ctx = new NPMDBEntities())
            {
                try
                {
                    var track = ctx.Ticket_Tracking.FirstOrDefault(t => t.Ticket_Trail_Id == model.Trail_id && t.Deleted == false); // Replace 'Id' with your actual PK

                    if (track != null)
                    {
                        ctx.Ticket_Tracking.Remove(track); // For hard delete

                        // Optional: for soft delete instead
                        // track.Deleted = true;

                        ctx.SaveChanges();

                        response.Status = "Success";
                        response.Response = "Ticket response deleted successfully.";
                    }
                    else
                    {
                        response.Status = "Error";
                        response.Response = "Ticket response not found.";
                    }
                }
                catch (Exception ex)
                {
                    response.Status = "Error";
                    response.Response = ex.Message;
                }
            }

            return response;
        }

        public ResponseModel GetAssignedUser(long practicecode)
        {
            using (var ctx = new NPMDBEntities())
            {
                try
                {
                    
                    var assigneduser = ctx.Practice_Reporting
                        .FirstOrDefault(c => c.Practice_Code == practicecode && c.Deleted != true);
 
    
                    return new ResponseModel
                    {
                        Status = "Success",
                        Response = assigneduser
                    };
                }
                catch (Exception ex)
                {
                    return new ResponseModel
                    {
                        Status = "Error",
                        Response = ex.Message
                    };
                }
            }
        }
    }
}

