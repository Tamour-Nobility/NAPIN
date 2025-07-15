using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Dynamic;
using System.Linq;
using NPMAPI.Models;
using NPMAPI.Models.ViewModels;
using NPMAPI.Repositories;

namespace NPMAPI.Services
{
    public class SchedulerService : ISchedulerRepository
    {
        private readonly IDemographicRepository _iDemographicRepository;
        public SchedulerService(IDemographicRepository demographicRepository)
        {
            _iDemographicRepository = demographicRepository;
        }

        public SchedulerService()
        {
        }

        public ResponseModel GetAppointments(AppointmentsSearchViewModel model)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var results = ctx.GetAppointmentEvents(model.practiceCode, string.Join(",", model.providers.Select(m => m.Id)), model.sDate, model.eDate, string.Join(",", model.locations.Select(m => m.Id))).ToList();
                    responseModel.Response = new ExpandoObject();
                    responseModel.Response.results = results;
                    responseModel.Status = "Success";

                }
            }
            catch (Exception)
            {
                throw;
            }
            return responseModel;
        }

        public ResponseModel GetExistingAppointments(AppointmentCreateModel model)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var results = ctx.GetExistingAppointmentData(model.PracticeCode, model.PatientAccount, model.AppointmentDateTime ).ToList();
                    responseModel.Response = new ExpandoObject();
                    responseModel.Response.results = results;
                    responseModel.Status = "Success";
                }
            }
            catch (Exception)
            {
                throw;
            }
            return responseModel;
        }

        public ResponseModel GetProviderAppointmentSettings(long providerCode)
        {
            ResponseModel responseModel = new ResponseModel();                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              
            try
            {
                responseModel.Status = "Success";
            }
            catch (Exception ex)
            {
                responseModel.Status = ex.ToString();
            }
            return responseModel;
        }

        public ResponseModel GetAppointmentModel(long practiceCode, long? id)
        {
            ResponseModel responseModel = new ResponseModel();
            AppointmentViewModel model = new AppointmentViewModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var appointment = ctx.Appointments.Where(ap => ap.Appointment_Id == id && !(ap.Deleted ?? false)).FirstOrDefault();
                    if (appointment != null)
                    {
                        model = (from a in ctx.Appointments
                                 join p in ctx.Patients on a.Patient_Account equals p.Patient_Account
                                 join ar in ctx.appointment_reasons on a.Reason_Id equals ar.Reason_Id
                                 join prov in ctx.Providers on a.Provider_Code equals prov.Provider_Code
                                 join loc in ctx.Practice_Locations on a.Location_Id equals loc.Location_Code
                                 where a.Appointment_Id == id && !(a.Deleted ?? false)
                                 select new AppointmentViewModel()
                                 {
                                     AppointmentDateTime = a.Appointment_Date_Time,
                                     AppointmentId = a.Appointment_Id,
                                     Duration = a.Duration,
                                     Patient = new SelectListViewModel()
                                     {
                                         Id = p.Patient_Account,
                                         IdStr = p.Patient_Account + "",
                                         Name = p.Patient_Account + "|" + p.Last_Name + ", " + p.First_Name
                                     },
                                     PatientAccount = a.Patient_Account,
                                     Reason = new SelectListViewModel()
                                     {
                                         Id = ar.Reason_Id,
                                         IdStr = ar.Reason_Id + "",
                                         Name = ar.Reason_Description
                                     },
                                     ReasonId = a.Reason_Id,
                                     Statuses = ctx.Appointment_Status.Select(s => new SelectListViewModel()
                                     {
                                         Id = s.Appointment_Status_Id,
                                         IdStr = s.Appointment_Status_Id + "",
                                         Name = s.Appointment_Status_Description
                                     }).ToList(),
                                     StatusId = a.Appointment_Status_Id,
                                     TimeFrom = a.Time_From,
                                     AttendingPhysician = a.Attending_Physician,
                                     LocationCode = a.Location_Id,
                                     Location = new SelectListViewModel()
                                     {
                                         Id = loc.Location_Code,
                                         IdStr = loc.Location_Code + "",
                                         Name = loc.Location_Name
                                     },
                                     ProviderCode = a.Provider_Code,
                                     Provider = new SelectListViewModel()
                                     {
                                         Id = prov.Provider_Code,
                                         IdStr = prov.Provider_Code + "",
                                         Name = prov.Provid_LName + ", " + prov.Provid_FName
                                     },
                                     Notes = a.Notes
                                 }).FirstOrDefault();
                        if (model.ProviderCode != null && model.ProviderCode > 0 && model.LocationCode != null && model.LocationCode > 0 && model.AppointmentId != null && model.AppointmentId > 0)
                            model.Reasons = GetPracAppointmentReasons(practiceCode, (long)model.ProviderCode, (long)model.LocationCode).Response;
                    }
                    else
                    {
                        model.Statuses = ((List<PracticeAppointmentStatusViewModel>)GetPracticeAppointmentStatuses(practiceCode, 0).Response).Select(s => new SelectListViewModel()
                        {
                            Id = s.AppStatusID,
                            IdStr = s.AppStatusID + "",
                            Name = s.Appointment_Status_Description
                        }).ToList();
                    }

                    responseModel.Status = "Success";
                    responseModel.Response = model;
                }
            }
            catch (Exception ex)
            {
                responseModel.Status = ex.ToString();
            }
            return responseModel;
        }

        public ResponseModel GetReasonsSelectList(string searchText)
        {
            ResponseModel responseModel = new ResponseModel();
            List<SelectListViewModel> list;
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    list = ctx.appointment_reasons.Where(ar => ((ar.Deleted ?? false) == false) && (ar.Reason_Description.Contains(searchText)
                     || ar.Reason_Color.Contains(searchText))).Select(ar => new SelectListViewModel()
                     {
                         Id = ar.Reason_Id,
                         Name = ar.Reason_Description,
                         IdStr = ar.Reason_Id + ""
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
            catch (Exception ex)
            {
                responseModel.Status = ex.ToString();
            }
            return responseModel;
        }

        //public ResponseModel Save(AppointmentCreateModel model, long userId)
        //{
        //    ResponseModel responseModel = new ResponseModel();
        //    Appointment appointment;
        //    try
        //    {
        //        using (var ctx = new NPMDBEntities())
        //        {
        //            //var slotAvailability = SlotAvailability(new SlotAvailabilityRequestVM()
        //            //{
        //            //    appointmentId = model.AppointmentId,
        //            //    date = model.AppointmentDateTime,
        //            //    duration = model.Duration,
        //            //    locationCode = model.LocationCode,
        //            //    practiceCode = model.PracticeCode,
        //            //    providerCode = model.ProviderCode,
        //            //    timeFrom = model.TimeFrom
        //            //}, userId).Response;
        //            if (model.AppointmentId > 0)
        //            {
        //                long previousProviderCode = model.ProviderCode;
        //                long previousLocationCode = model.LocationCode;
        //                appointment = ctx.Appointments.Where(a => a.Appointment_Id == model.AppointmentId && !(a.Deleted ?? false)).FirstOrDefault();
        //                if (appointment != null)
        //                {
        //                    appointment.Notes = model.Notes;
        //                    appointment.Patient_Account = model.PatientAccount;
        //                    appointment.Appointment_Date_Time = model.AppointmentDateTime;
        //                    appointment.Time_From = model.TimeFrom;
        //                    appointment.Duration = model.Duration;
        //                    appointment.Reason_Id = model.ReasonId;
        //                    appointment.Appointment_Status_Id = model.StatusId;
        //                    appointment.Location_Id = model.LocationCode;
        //                    appointment.Modified_By = userId;
        //                    appointment.Modified_Date = DateTime.Now;
        //                    appointment.Provider_Code = model.ProviderCode;
        //                    //appointment.Practice_Code = model.PracticeCode;
        //                    ctx.Entry(appointment).State = System.Data.Entity.EntityState.Modified;
        //                }
        //                else
        //                    responseModel.Status = "No appointment found.";

        //            }
        //            else
        //            {
        //                appointment = new Appointment()
        //                {
        //                    Appointment_Id = Convert.ToInt64(ctx.SP_TableIdGenerator("Appointment_Id").FirstOrDefault().ToString()),
        //                    Notes = model.Notes,
        //                    Patient_Account = model.PatientAccount,
        //                    Appointment_Date_Time = model.AppointmentDateTime,
        //                    Time_From = model.TimeFrom,
        //                    Duration = model.Duration,
        //                    Reason_Id = model.ReasonId,
        //                    Appointment_Status_Id = model.StatusId,
        //                    Location_Id = model.LocationCode,
        //                    Created_Date = DateTime.Now,
        //                    Created_By = userId,
        //                    Provider_Code = model.ProviderCode,
        //                    Deleted = false,
        //                    Practice_Code = model.PracticeCode
        //                };
        //                ctx.Appointments.Add(appointment);
        //            }
        //            if (ctx.SaveChanges() > 0)
        //            {
        //                responseModel.Status = "Success";
        //                responseModel.Response = new ExpandoObject();
        //                responseModel.Response.Location = ctx.Practice_Locations.Where(p => p.Location_Code == appointment.Location_Id).Select(p => new SelectListViewModel()
        //                {
        //                    Id = p.Location_Code,
        //                    Name = p.Location_Code + "|" + p.Location_Name
        //                }).FirstOrDefault();
        //                responseModel.Response.Provider = ctx.Providers.Where(p => p.Provider_Code == appointment.Provider_Code).Select(p => new SelectListViewModel()
        //                {
        //                    Id = p.Provider_Code,
        //                    Name = p.Provider_Code + "|" + p.Provid_LName + ", " + p.Provid_FName
        //                }).FirstOrDefault();
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        responseModel.Status = ex.ToString();
        //    }
        //    return responseModel;
        //}

        //adding changes for duplicate appointment
        public ResponseModel Save(AppointmentCreateModel model, long userId)
        {
            ResponseModel responseModel = new ResponseModel();
            Appointment appointment;
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    // Check for existing appointments with the same patient, provider, location, and date
                    var existingAppointment = ctx.Appointments
                        .Where(a => a.Patient_Account == model.PatientAccount &&
                                    a.Provider_Code == model.ProviderCode &&
                                    a.Location_Id == model.LocationCode &&
                                    DbFunctions.TruncateTime(a.Appointment_Date_Time) == DbFunctions.TruncateTime(model.AppointmentDateTime) &&
                                    !(a.Deleted ?? false) &&
                                    a.Appointment_Id != model.AppointmentId) // Exclude current appointment if updating
                        .FirstOrDefault();


                    if (existingAppointment != null)
                    {

                        var provider = ctx.Providers
                            .Where(p => p.Provider_Code == model.ProviderCode)
                            .Select(p => p.Provid_LName + "," + p.Provid_FName)
                            .FirstOrDefault();

                        var location = ctx.Practice_Locations
                            .Where(l => l.Location_Code == model.LocationCode)
                            .Select(l => l.Location_Name)
                            .FirstOrDefault();

                        string errorMessage = $"Another appointment is already scheduled with  {provider} and {location}.";

                        //responseModel.Status = "Error";
                        responseModel.Status = errorMessage;
                        return responseModel;
                    }

                    // Proceed with saving the appointment
                    if (model.AppointmentId > 0)
                    {
                        // Update existing appointment
                        appointment = ctx.Appointments
                            .Where(a => a.Appointment_Id == model.AppointmentId && !(a.Deleted ?? false))
                            .FirstOrDefault();

                        //if (appointment != null)
                        //{
                            if (appointment != null)
                            {
                                // Perform the overlap check again for updates
                                var conflictingAppointment = ctx.Appointments
                                    .Where(a => a.Patient_Account == model.PatientAccount &&
                                                a.Provider_Code == model.ProviderCode &&
                                                a.Location_Id == model.LocationCode &&
                                                DbFunctions.TruncateTime(a.Appointment_Date_Time) == DbFunctions.TruncateTime(model.AppointmentDateTime) &&
                                                !(a.Deleted ?? false) &&
                                                a.Appointment_Id != model.AppointmentId) // Exclude current appointment
                                    .FirstOrDefault();

                                if (conflictingAppointment != null)
                                {
                                    var provider = ctx.Providers
                                        .Where(p => p.Provider_Code == model.ProviderCode)
                                        .Select(p => p.Provid_LName + "," + p.Provid_FName)
                                        .FirstOrDefault();

                                    var location = ctx.Practice_Locations
                                        .Where(l => l.Location_Code == model.LocationCode)
                                        .Select(l => l.Location_Name)
                                        .FirstOrDefault();

                                    string errorMessage = $"Another appointment is already scheduled with {provider} and {location}.";
                                    responseModel.Status = errorMessage;
                                    return responseModel;
                                }

                                appointment.Notes = model.Notes;
                                appointment.Patient_Account = model.PatientAccount;
                                appointment.Appointment_Date_Time = model.AppointmentDateTime;
                                appointment.Time_From = model.TimeFrom;
                                appointment.Duration = model.Duration;
                                appointment.Reason_Id = model.ReasonId;
                                appointment.Appointment_Status_Id = model.StatusId;
                                appointment.Location_Id = model.LocationCode;
                                appointment.Modified_By = userId;
                                appointment.Modified_Date = DateTime.Now;
                                appointment.Provider_Code = model.ProviderCode;
                                ctx.Entry(appointment).State = System.Data.Entity.EntityState.Modified;
                            }
                            else
                            {
                                responseModel.Status = "No appointment found.";
                            }
                        }
                        else
                        {
                            // Create a new appointment
                            appointment = new Appointment()
                            {
                                Appointment_Id = Convert.ToInt64(ctx.SP_TableIdGenerator("Appointment_Id").FirstOrDefault().ToString()),
                                Notes = model.Notes,
                                Patient_Account = model.PatientAccount,
                                Appointment_Date_Time = model.AppointmentDateTime,
                                Time_From = model.TimeFrom,
                                Duration = model.Duration,
                                Reason_Id = model.ReasonId,
                                Appointment_Status_Id = model.StatusId,
                                Location_Id = model.LocationCode,
                                Created_Date = DateTime.Now,
                                Created_By = userId,
                                Provider_Code = model.ProviderCode,
                                Deleted = false,
                                Practice_Code = model.PracticeCode
                            };
                            ctx.Appointments.Add(appointment);
                        }

                        // Save changes to the database
                        if (ctx.SaveChanges() > 0)
                        {
                            responseModel.Status = "Success";
                            responseModel.Response = new ExpandoObject();
                            responseModel.Response.Location = ctx.Practice_Locations
                                .Where(p => p.Location_Code == appointment.Location_Id)
                                .Select(p => new SelectListViewModel()
                                {
                                    Id = p.Location_Code,
                                    Name = p.Location_Code + "|" + p.Location_Name
                                }).FirstOrDefault();

                            responseModel.Response.Provider = ctx.Providers
                                .Where(p => p.Provider_Code == appointment.Provider_Code)
                                .Select(p => new SelectListViewModel()
                                {
                                    Id = p.Provider_Code,
                                    Name = p.Provider_Code + "|" + p.Provid_LName + ", " + p.Provid_FName
                                }).FirstOrDefault();
                        }
                    }
                
                }
            catch (Exception ex)
            {
                responseModel.Status = ex.ToString();
            }
            return responseModel;
        }

        //


        public ResponseModel Delete(long id, long userId)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var appointment = ctx.Appointments.FirstOrDefault(a => a.Appointment_Id == id);
                    if (appointment != null)
                    {
                        appointment.Deleted = true;
                        appointment.Modified_By = userId;
                        appointment.Modified_Date = DateTime.Now;
                        if (ctx.SaveChanges() > 0)
                        {
                            responseModel.Status = "Success";
                            responseModel.Response = appointment.Appointment_Id;
                            return responseModel;
                        }
                        responseModel.Status = "Failed to delete the appointment.";
                    }
                }
            }
            catch (Exception ex)
            {
                responseModel.Status = ex.ToString();
            }
            return responseModel;
        }

        public ResponseModel Timings(TimingSearchViewModel model, long userId)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var timings = ctx.sp_provider_working_times(practice_code: model.practiceCode, provider_code: model.providerCode, location_code: model.locationCode, dateFrom: model.dateFrom, dateTo: model.dateTo).ToList();
                    responseModel.Response = new ExpandoObject();
                    responseModel.Response.timings = timings;
                    responseModel.Status = "Success";
                }
            }
            catch (Exception)
            {
                throw;
            }
            return responseModel;
        }

        public ResponseModel Rules(ProviderAppointmentRuleViewModel model, long userId)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var rules = ctx.Provider_Appointment_Rules.Where(r => r.Practice_Code == model.Practice_Code
                    && r.Provider_code == model.Provider_code
                    && r.Location_code == model.Location_code
                    && r.No_Appointments_Start_Time == model.No_Appointments_Start_Time
                    && r.No_Appointment_End_Time == model.No_Appointment_End_Time
                    )
                        .Select(rs => new
                        {
                            No_Appointments_Start_Date = rs.No_Appointments_Start_Time.ToString().Substring(0, 11),
                            No_Appointment_End_Date = rs.No_Appointment_End_Time.ToString().Substring(0, 11),
                            No_Appointments_Start_Time = rs.No_Appointments_Start_Time.ToString().Substring(12, rs.No_Appointments_Start_Time.ToString().Length),
                            No_Appointment_End_Time = rs.No_Appointment_End_Time.ToString().Substring(12, rs.No_Appointments_Start_Time.ToString().Length)
                        }).ToList();
                    responseModel.Response = rules;
                    responseModel.Status = "Success";
                }
            }
            catch (Exception)
            {
                throw;
            }
            return responseModel;
        }

        public ResponseModel GetProviderAppointmentRules(ProviderAppointmentRuleViewModel model, long userId)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var rules = ctx.Provider_Appointment_Rules.Where(r => r.Practice_Code == model.Practice_Code && r.Provider_code == model.Provider_code && r.Location_code == model.Location_code).Select(r => new
                    {
                        No_Appointments_Start_Date = r.No_Appointments_Start_Time.ToString(),
                        No_Appointment_End_Date = r.No_Appointment_End_Time.ToString(),
                        No_Appointments_Start_Time = r.No_Appointments_Start_Time.ToString(),
                        No_Appointment_End_Time = r.No_Appointment_End_Time.ToString()

                    }).ToList();
                    responseModel.Response = rules;
                    responseModel.Status = "Success";
                }
            }
            catch (Exception)
            {
                throw;
            }
            return responseModel;
        }

        public ResponseModel SaveOfficeTimings(OfficeTimingCreateViewModel model, long userId)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    foreach (var timing in model.timings)
                    {
                        Provider_Working_Days_Time pwdt = ctx.Provider_Working_Days_Time.FirstOrDefault(p => p.Practice_Code == model.schedule.PracticeCode &&
                         p.Provider_Code == model.schedule.ProviderCode &&
                         p.Location_Code == model.schedule.LocationCode &&
                         p.Date_From == model.schedule.DateFrom &&
                         p.Date_To == model.schedule.DateTo &&
                         p.Weekday_Id == timing.weekday_id);
                        if (pwdt == null)
                        {
                            pwdt = new Provider_Working_Days_Time();
                            pwdt.Provider_Working_Days_Time_Id = Convert.ToInt64(ctx.SP_TableIdGenerator("Provider_Working_Days_Time_Id").FirstOrDefault().ToString());
                            pwdt.Provider_Code = model.schedule.ProviderCode;
                            pwdt.Practice_Code = model.schedule.PracticeCode;
                            pwdt.Location_Code = model.schedule.LocationCode;
                            pwdt.Weekday_Id = timing.weekday_id;
                            pwdt.Time_From = DateTime.Now.Date + timing.Time_From;
                            pwdt.Time_To = DateTime.Now.Date + timing.Time_To;
                            pwdt.Break_Time_From = DateTime.Now.Date + timing.Break_time_From;
                            pwdt.Break_Time_To = DateTime.Now.Date + timing.Break_Time_To;
                            pwdt.Enable_Break = true;
                            pwdt.Date_From = model.schedule.DateFrom;
                            pwdt.Created_By = userId;
                            pwdt.Created_Date = DateTime.Now;
                            pwdt.Day_On = timing.Day_on;
                            pwdt.Date_To = model.schedule.DateTo;
                            pwdt.Time_slot_size = timing.time_slot_size;
                            ctx.Provider_Working_Days_Time.Add(pwdt);
                            ctx.Entry(pwdt).State = System.Data.Entity.EntityState.Added;
                        }
                        else
                        {
                            pwdt.Provider_Code = model.schedule.ProviderCode;
                            pwdt.Practice_Code = model.schedule.PracticeCode;
                            pwdt.Location_Code = model.schedule.LocationCode;
                            pwdt.Weekday_Id = timing.weekday_id;
                            pwdt.Time_From = DateTime.Now.Date + timing.Time_From;
                            pwdt.Time_To = DateTime.Now.Date + timing.Time_To;
                            pwdt.Break_Time_From = DateTime.Now.Date + timing.Break_time_From;
                            pwdt.Break_Time_To = DateTime.Now.Date + timing.Break_Time_To;
                            pwdt.Enable_Break = true;
                            pwdt.Date_From = model.schedule.DateFrom;
                            pwdt.Modified_By = userId;
                            pwdt.Modified_Date = DateTime.Now;
                            pwdt.Day_On = timing.Day_on;
                            pwdt.Date_To = model.schedule.DateTo;
                            pwdt.Time_slot_size = timing.time_slot_size;
                            ctx.Entry(pwdt).State = System.Data.Entity.EntityState.Modified;
                        }
                        ctx.SaveChanges();
                    }
                    responseModel.Status = "Success";
                }
            }
            catch (Exception)
            {
                throw;
            }
            return responseModel;
        }

        public ResponseModel SaveAppointmentRules(ProviderAppointmentRuleViewModel rule, long userId)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    Provider_Appointment_Rules par = ctx.Provider_Appointment_Rules.FirstOrDefault(p => p.Rule_id == rule.Rule_Id);
                    if (par == null)
                    {
                        var cs = canSave(rule);
                        if (cs.Status == "NF")
                        {
                            par = new Provider_Appointment_Rules();
                            par.Rule_id = Convert.ToInt64(ctx.SP_TableIdGenerator("rule_id").FirstOrDefault().ToString());
                            par.Provider_code = rule.Provider_code;
                            par.Practice_Code = rule.Practice_Code;
                            par.Location_code = rule.Location_code;
                            par.No_Appointments_Start_Time = rule.No_Appointments_Start_Time;
                            par.No_Appointment_End_Time = rule.No_Appointment_End_Time;
                            ctx.Provider_Appointment_Rules.Add(par);
                            ctx.Entry(par).State = System.Data.Entity.EntityState.Added;
                        }
                        else
                        {
                            responseModel.Status = "Failed";
                            responseModel.Response = cs.Response;
                        }
                    }
                    else
                    {
                        par.No_Appointments_Start_Time = rule.No_Appointments_Start_Time;
                        par.No_Appointment_End_Time = rule.No_Appointment_End_Time;
                        ctx.Entry(par).State = System.Data.Entity.EntityState.Modified;
                    }
                    var r = ctx.SaveChanges();
                    if (r > 0)
                    {
                        responseModel.Status = "Success";
                        responseModel.Response = new ProviderAppointmentRuleViewModel
                        {
                            Rule_Id = par.Rule_id,
                            Practice_Code = (long)par.Practice_Code,
                            Provider_code = par.Provider_code,
                            Location_code = (long)par.Location_code,
                            No_Appointments_Start_Time = (DateTime)par.No_Appointments_Start_Time,
                            No_Appointment_End_Time = (DateTime)par.No_Appointment_End_Time
                        };
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return responseModel;
        }

        private ResponseModel canSave(ProviderAppointmentRuleViewModel rule)
        {
            ResponseModel res = new ResponseModel();
            using (var ctx = new NPMDBEntities())
            {
                var cot = ctx.Check_Office_Timing(rule.Practice_Code, rule.Provider_code, rule.Location_code, rule.No_Appointments_Start_Time, rule.No_Appointment_End_Time).ToList();
                if (cot.Count > 0)
                {
                    var par = ctx.Check_Duplicate_Blocking_Rule(rule.Practice_Code, rule.Provider_code, rule.Location_code, rule.No_Appointments_Start_Time, rule.No_Appointment_End_Time).ToList();
                    if (par.Count > 0)
                    {
                        res.Status = "F";
                        res.Response = "Rule Already Exists Within Your Given Time Range.";
                        return res;
                    }
                    else
                    {
                        res.Status = "NF";
                        return res;
                    }
                }
                else
                {
                    res.Status = "NOffT";
                    res.Response = "No Office Timing Exists Within Your Given Time Range.";
                    return res;
                }
            }
        }

        public ResponseModel GetProviderSchedules(long practiceCode, long providerCode, long locationCode)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var scheduleGroups = ctx.Provider_Working_Days_Time
                        .Where(pwdt => pwdt.Practice_Code == practiceCode && pwdt.Provider_Code == providerCode && pwdt.Location_Code == locationCode).GroupBy(pwdt => new { pwdt.Date_From, pwdt.Date_To }).ToList();
                    List<ProviderSchedulesViewModel> providerSchedulesViewModels = new List<ProviderSchedulesViewModel>();
                    var serialNo = 0;
                    scheduleGroups.ForEach((group) =>
                    {
                        serialNo++;
                        var schedule = group.FirstOrDefault();
                        providerSchedulesViewModels.Add(new ProviderSchedulesViewModel()
                        {
                            DateFrom = (DateTime)schedule.Date_From,
                            DateTo = (DateTime)schedule.Date_To,
                            LocationCode = locationCode,
                            ProviderCode = providerCode,
                            SrNo = serialNo,
                            PracticeCode = practiceCode
                        });
                    });
                    responseModel.Status = "Success";
                    responseModel.Response = providerSchedulesViewModels.OrderByDescending(d => d.DateTo);
                }
            }
            catch (Exception)
            {
                throw;
            }
            return responseModel;
        }
        public ResponseModel GetProviderSchedulesWithLocation(long practiceCode, List<long> providerCodes, List<long> locationCodes)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var scheduleGroups = ctx.Provider_Working_Days_Time
                        .Where(pwdt =>
                            pwdt.Practice_Code == practiceCode &&
                            providerCodes.Contains(pwdt.Provider_Code) &&
                            locationCodes.Contains((long)pwdt.Location_Code))
                        .GroupBy(pwdt => new { pwdt.Date_From, pwdt.Date_To })
                        .ToList();

                    List<ProviderSchedulesViewModel> providerSchedulesViewModels = new List<ProviderSchedulesViewModel>();
                    var serialNo = 0;

                    scheduleGroups.ForEach((group) =>
                    {
                        serialNo++;
                        var schedule = group.FirstOrDefault();
                        providerSchedulesViewModels.Add(new ProviderSchedulesViewModel()
                        {
                            DateFrom = (DateTime)schedule.Date_From,
                            DateTo = (DateTime)schedule.Date_To,
                            LocationCode = (long)schedule.Location_Code,
                            ProviderCode = schedule.Provider_Code,
                            SrNo = serialNo,
                            PracticeCode = practiceCode
                        });
                    });

                    responseModel.Status = "Success";
                    responseModel.Response = providerSchedulesViewModels.OrderByDescending(d => d.DateTo);
                }
            }
            catch (Exception)
            {
                throw;
            }
            return responseModel;
        }
        public ResponseModel GetOfficeTimingForProvidersAndLocations(long practiceCode,string providerCodes, string locationCodes,DateTime dateFrom, DateTime dateTo)
        {
            ResponseModel responseModel = new ResponseModel();
            List<USP_GetOfficeTimeForProvidersAndLocations_Result> officeTime = new List<USP_GetOfficeTimeForProvidersAndLocations_Result>();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    officeTime = ctx.USP_GetOfficeTimeForProvidersAndLocations(practiceCode,providerCodes,locationCodes,dateFrom,dateTo). ToList();
                    if (officeTime.Count > 0)
                    {
                       
                        responseModel.Status = "Success";
                        responseModel.Response = officeTime;
                    }
                    else
                    {
                        responseModel.Status = "Error";
                        responseModel.Response = "No Rules Found";
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return responseModel;
        }
        public ResponseModel GetProviderBlockedSchedules(long practiceCode, long providerCode, long locationCode)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var blockedScheduleGroups = ctx.Provider_Appointment_Rules
                        .Where(pwdt => pwdt.Practice_Code == practiceCode
                        && pwdt.Provider_code == providerCode
                        && pwdt.Location_code == locationCode)
                        .GroupBy(pwdt => new { pwdt.No_Appointments_Start_Time, pwdt.No_Appointment_End_Time })
                        .ToList();
                    if (blockedScheduleGroups.Count > 0)
                    {
                        List<ProviderAppointmentRuleViewModel> providerAppRuleViewModels = new List<ProviderAppointmentRuleViewModel>();

                        blockedScheduleGroups.ForEach((group) =>
                        {
                            var blockedSchedule = group.FirstOrDefault();
                            providerAppRuleViewModels.Add(new ProviderAppointmentRuleViewModel()
                            {
                                Rule_Id = blockedSchedule.Rule_id,
                                Practice_Code = practiceCode,
                                Provider_code = providerCode,
                                Location_code = locationCode,
                                No_Appointments_Start_Time = Convert.ToDateTime(blockedSchedule.No_Appointments_Start_Time),
                                No_Appointment_End_Time = Convert.ToDateTime(blockedSchedule.No_Appointment_End_Time),
                            });
                        });
                        responseModel.Status = "Success";
                        responseModel.Response = providerAppRuleViewModels;
                    }
                    else
                    {
                        responseModel.Status = "Error";
                        responseModel.Response = "No Rules Found";
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return responseModel;
        }

        public ResponseModel GetMatchingSchedules(ProviderSchedulesViewModel model, long userId)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var groups = ctx.Provider_Working_Days_Time.Where(
                        p => p.Practice_Code == model.PracticeCode
                        && p.Provider_Code == model.ProviderCode
                        && p.Location_Code == model.LocationCode
                        && ((p.Date_From >= model.DateFrom && p.Date_From <= model.DateTo)
                        || (p.Date_To >= model.DateFrom && p.Date_To <= model.DateTo)
                        || (model.DateFrom >= p.Date_From && model.DateFrom <= p.Date_From)
                        || (model.DateTo >= p.Date_From && model.DateTo <= p.Date_To))
                        ).GroupBy(p => new { p.Date_From, p.Date_To }).ToList();
                    List<ProviderSchedulesViewModel> providerSchedulesViewModels = new List<ProviderSchedulesViewModel>();
                    groups.ForEach((group) =>
                    {
                        var schedule = group.FirstOrDefault();
                        providerSchedulesViewModels.Add(new ProviderSchedulesViewModel()
                        {
                            DateFrom = (DateTime)schedule.Date_From,
                            DateTo = (DateTime)schedule.Date_To,
                            LocationCode = model.LocationCode,
                            ProviderCode = model.ProviderCode,
                            PracticeCode = model.PracticeCode
                        });
                    });
                    responseModel.Status = "Success";
                    responseModel.Response = providerSchedulesViewModels.OrderByDescending(p => p.DateTo);
                }
            }
            catch (Exception)
            {
                throw;
            }
            return responseModel;
        }

        public ResponseModel Statuses(long userId)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    responseModel.Response = ctx.Appointment_Status.ToList();
                    responseModel.Status = "Success";
                }
            }
            catch (Exception)
            {
                throw;
            }
            return responseModel;
        }
      

        public ResponseModel SaveStatus(PracticeAppointmentStatusCreateVM model, long userId)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    foreach (var appointmentStatusId in model.AppointmentStatusesId)
                    {
                        var pas = ctx.PracticeAppointmentStatus.FirstOrDefault(p => p.AppStatusID == appointmentStatusId && p.PracCode == model.PracticeCode && !(p.deleted ?? false));
                        if (pas == null)
                        {
                            pas = new PracticeAppointmentStatu();
                            pas.PracAppSID = Convert.ToInt64(ctx.SP_TableIdGenerator("PracAppSID").FirstOrDefault().ToString());
                            pas.PracCode = model.PracticeCode;
                            pas.AppStatusID = appointmentStatusId;
                            pas.CREATED_BY = userId;
                            pas.CREATED_DATE = DateTime.Now;
                            pas.deleted = false;
                            ctx.Entry(pas).State = System.Data.Entity.EntityState.Added;
                            ctx.PracticeAppointmentStatus.Add(pas);
                        }
                    }
                    responseModel.Status = "Success";
                    ctx.SaveChanges();
                }
            }
            catch (Exception)
            {
                throw;
            }
            return responseModel;
        }

        public ResponseModel GetPracticeAppointmentStatuses(long practiceCode, long userId)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var practiceAppointmentStatuses = (from s in ctx.Appointment_Status
                                                       join pas in ctx.PracticeAppointmentStatus on s.Appointment_Status_Id equals pas.AppStatusID
                                                       join p in ctx.Practices on pas.PracCode equals p.Practice_Code
                                                       where pas.PracCode == practiceCode
                                                       && !(pas.deleted ?? false)
                                                       select new PracticeAppointmentStatusViewModel()
                                                       {
                                                           AppStatusID = s.Appointment_Status_Id,
                                                           LocID = pas.LocID,
                                                           PracAppSID = pas.PracAppSID,
                                                           PracCode = pas.PracCode,
                                                           ProvdID = pas.ProvdID,
                                                           Appointment_Status_Description = s.Appointment_Status_Description
                                                       }).ToList();
                    responseModel.Response = practiceAppointmentStatuses;
                    responseModel.Status = "Success";
                }

            }
            catch (Exception)
            {
                throw;
            }
            return responseModel;
        }

        public ResponseModel GetPracticeAppointmentStatus(long practiceCode, long userId, long practiceAppointmentStatusId)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var practiceAppointmentStatuses = (from s in ctx.Appointment_Status
                                                       join pas in ctx.PracticeAppointmentStatus on s.Appointment_Status_Id equals pas.PracAppSID
                                                       join p in ctx.Practices on pas.PracCode equals p.Practice_Code
                                                       where pas.PracCode == practiceCode && pas.PracAppSID == practiceAppointmentStatusId
                                                       && !(pas.deleted ?? false)
                                                       select new PracticeAppointmentStatusViewModel()
                                                       {
                                                           AppStatusID = s.Appointment_Status_Id,
                                                           LocID = pas.LocID,
                                                           PracAppSID = pas.PracAppSID,
                                                           PracCode = pas.PracCode,
                                                           ProvdID = pas.ProvdID,
                                                           Appointment_Status_Description = s.Appointment_Status_Description
                                                       }).FirstOrDefault();
                    responseModel.Response = practiceAppointmentStatuses;
                    responseModel.Status = "Success";
                }

            }
            catch (Exception)
            {
                throw;
            }
            return responseModel;
        }

        public ResponseModel DeletePracticeAppointmentStatus(long id, long userId)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var practiceAppointmentStatuses = ctx.PracticeAppointmentStatus.FirstOrDefault(pas => pas.PracAppSID == id);
                    if (practiceAppointmentStatuses != null)
                    {
                        practiceAppointmentStatuses.MODIFIED_BY = userId;
                        practiceAppointmentStatuses.MODIFIED_DATE = DateTime.Now;
                        practiceAppointmentStatuses.deleted = true;
                        ctx.Entry(practiceAppointmentStatuses).State = System.Data.Entity.EntityState.Modified;
                        ctx.SaveChanges();
                        responseModel.Status = "Success";
                    }
                    else
                    {
                        responseModel.Status = "No Practice Appointment Status found with id " + id;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return responseModel;
        }

        public ResponseModel GetPracAppointmentReasons(long practiceCode, long providerCode, long locationCode)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var results = (from ar in ctx.appointment_reasons
                                   join par in ctx.PracticeAppointmentReasons on ar.Reason_Id equals par.Reason_Id
                                   where !(ar.Deleted ?? false) && !(par.deleted ?? false) && par.Practice_Code == practiceCode && par.LOCATION_ID == locationCode &&
                                   par.PROVIDER_CODE == providerCode
                                   select new
                                   {
                                       par.PracAppReasonID,
                                       ReasonId = ar.Reason_Id,
                                       Description = ar.Reason_Description,
                                       ReasonColor = !string.IsNullOrEmpty(par.REASON_COLOR) ? par.REASON_COLOR : ar.Reason_Color,
                                   }).OrderBy(r => r.Description).ToList();
                    responseModel.Response = results;
                    responseModel.Status = "Success";
                }
            }
            catch (Exception)
            {
                throw;
            }
            return responseModel;
        }

        public ResponseModel GetAppointmentReasons(string searchText = "", bool all = false)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    if (all)
                    {
                        responseModel.Response = ctx.appointment_reasons.Where(r => !(r.Deleted ?? false)).ToList();
                    }
                    else
                    {
                        responseModel.Response = ctx.appointment_reasons.Where(r => !(r.Deleted ?? false) && r.Reason_Description.Contains(searchText)).OrderBy(r => r.Reason_Description).ToList();
                    }
                    responseModel.Status = "Success";
                }
            }
            catch (Exception)
            {
                throw;
            }
            return responseModel;
        }

        public ResponseModel GetUnassignedAppointmentReasons(long practiceCode, long providerCode, long locationCode, string searchText)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    responseModel.Response = ctx.GetUnassignedAppointmemtReasons(practiceCode, providerCode, locationCode, searchText).ToList();
                    responseModel.Status = "Success";
                }
            }
            catch (Exception)
            {
                throw;
            }
            return responseModel;
        }

        public ResponseModel SaveReason(PracticeAppointmentReasonCreateVM model, long userId)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {

                    var response = ctx.InsertPracAppointmentReasons(model.PracticeCode, model.ProviderCode, model.LocationCode, string.Join(",", model.ReasonsIds), userId);
                    responseModel.Status = "Success";
                    ctx.SaveChanges();
                }
            }
            catch (Exception)
            {
                throw;
            }
            return responseModel;
        }

        public ResponseModel ChangeColor(long PracAppReasonID, string ReasonColor, long userId)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var par = ctx.PracticeAppointmentReasons.FirstOrDefault(p => p.PracAppReasonID == PracAppReasonID && !(p.deleted ?? false));
                    if (par != null)
                    {
                        par.REASON_COLOR = ReasonColor;
                        par.modified_by = userId;
                        par.modified_date = DateTimeOffset.Now;
                        ctx.Entry(par).State = System.Data.Entity.EntityState.Modified;
                        ctx.SaveChanges();
                        responseModel.Status = "Success";
                        responseModel.Response = par;
                    }
                    else
                    {
                        responseModel.Status = $"Practice Appointment Reason with ID {PracAppReasonID} is not found.";
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return responseModel;
        }

        public ResponseModel DeletePracticeAppointmentReason(long id, long userId)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var practiceAppointmentReason = ctx.PracticeAppointmentReasons.FirstOrDefault(par => par.PracAppReasonID == id);
                    if (practiceAppointmentReason != null)
                    {
                        practiceAppointmentReason.modified_by = userId;
                        practiceAppointmentReason.modified_date = DateTime.Now;
                        practiceAppointmentReason.deleted = true;
                        ctx.Entry(practiceAppointmentReason).State = System.Data.Entity.EntityState.Modified;
                        ctx.SaveChanges();
                        responseModel.Status = "Success";
                    }
                    else
                    {
                        responseModel.Status = "No Practice Appointment Reason found with id " + id;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return responseModel;
        }

        public ResponseModel SlotAvailability(SlotAvailabilityRequestVM model, long userId)
        {
            ResponseModel responseModel = new ResponseModel();
            SlotAvailabilityResponseVm slot = new SlotAvailabilityResponseVm();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    if (model.practiceCode > 0 && model.providerCode > 0 && model.locationCode > 0 && model.date != null && !string.IsNullOrEmpty(model.timeFrom))
                    {
                        slot.Appointment = ctx.GetAppointment(model.practiceCode, model.providerCode, model.locationCode, model.date, model.timeFrom).FirstOrDefault();
                        // in edit case > if appointment id == existing appointment id, we will consider as appointment is not already appointed if user doesn't change the parameters
                        if (model.appointmentId != null && model.appointmentId > 0 && slot.Appointment != null && slot.Appointment.Appointment_Id == model.appointmentId)
                            slot.Appointment = null;
                    }
                    if (model.practiceCode > 0 && model.providerCode > 0 && model.locationCode > 0 && model.date != null)
                        slot.OfficeTiming = ctx.GetOfficeTiming(model.practiceCode, model.providerCode, model.locationCode, model.date).FirstOrDefault();
                    var dFrom = Convert.ToDateTime(model.date).ToString("MM/dd/yyyy") + " " + TimeSpan.Parse(model.timeFrom);
                    var dtFrom = Convert.ToDateTime(dFrom);
                    var dur = model.duration == null ? 0 : model.duration;
                    var dTo = dtFrom.AddMinutes((double)dur);
                    var cdbr = ctx.Check_Duplicate_Blocking_Rule(model.practiceCode, model.providerCode, model.locationCode, dtFrom, dTo).FirstOrDefault();
                    if (cdbr != null)
                    {
                        slot.IsBlocked = true;
                        slot.BlockedTimes = cdbr;
                    }
                    else
                    {
                        slot.IsBlocked = false;
                        slot.BlockedTimes = null;
                    }
                    if (model.practiceCode > 0 && model.providerCode > 0 && model.locationCode > 0)
                        slot.Reasons = GetPracAppointmentReasons(model.practiceCode, model.providerCode, model.locationCode).Response;
                }
                responseModel.Status = "Success";
                responseModel.Response = slot;
            }
            catch (Exception)
            {
                throw;
            }
            return responseModel;
        }

        public ResponseModel CheckDeceasedPatient(long PatAccount)
        {
            ResponseModel res = new ResponseModel();
            using (var ctx = new NPMDBEntities())
            {
                var d = ctx.Patients.Where(p => p.Patient_Account == PatAccount).Select(pt => pt.IsDeceased).FirstOrDefault();
                if (d == true)
                    res.Response = "Deceased";
                else
                    res.Response = "Not Deceased";
                return res;
            }
        }
    }
}