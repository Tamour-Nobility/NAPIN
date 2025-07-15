using Microsoft.AspNet.SignalR;
using NPMAPI.com.gatewayedi.services;
using NPMAPI.Models;
using NPMAPI.Models.ViewModels;
using NPMAPI.Repositories;
using System;
using System.Dynamic;
using System.Linq;
using System.Web.Http;
using NPMAPI.com.gatewayedi.services;
using NPMAPI.Models;
using NPMAPI.Models.ViewModels;
using NPMAPI.Repositories;
using System.Collections.Generic;

namespace NPMAPI.Controllers
{
    public class SchedulerController : BaseController
    {
        private ISchedulerRepository _schedulerRepository;
        private IDemographicRepository _iDemographicRepository;
        private IEligibility _iEligibility;
        public SchedulerController(ISchedulerRepository schedulerRepository, IDemographicRepository iDemographicRepository, IEligibility iEligibility)
        {
            _schedulerRepository = schedulerRepository;
            _iDemographicRepository = iDemographicRepository;
            _iEligibility = iEligibility;
        }
        [HttpPost]
        [ActionName("events")]
        public ResponseModel GetAppointments(AppointmentsSearchViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return new ResponseModel()
                {
                    Response = null,
                    Status = string.Join(";", ModelState.Values.SelectMany(e => e.Errors).Select(e => e.ErrorMessage))
                };
            }
            return _schedulerRepository.GetAppointments(model);
        }
        [HttpPost]
        public ResponseModel GetExistingAppointments(AppointmentCreateModel model)
        {
            if (!ModelState.IsValid)
            {
                return new ResponseModel()
                {
                    Response = null,
                    Status = string.Join(";", ModelState.Values.SelectMany(e => e.Errors).Select(e => e.ErrorMessage))
                };
            }
            return _schedulerRepository.GetExistingAppointments(model);
        }
        [HttpGet]
        [ActionName("settings")]
        public ResponseModel GetProviderAppointmentSettings(long providerCode)
        {
            return _schedulerRepository.GetProviderAppointmentSettings(providerCode);
        }
        [HttpGet]
        [ActionName("GetAppointmentModel")]
        public ResponseModel GetAppointmentModel(long practiceCode, long? id)
        {
            return _schedulerRepository.GetAppointmentModel(practiceCode, id);
        }
        [HttpGet]
        public ResponseModel GetReasonsSelectList(string searchText)
        {
            return _schedulerRepository.GetReasonsSelectList(searchText);
        }
        [HttpPost]
        public ResponseModel Save([FromBody] AppointmentCreateModel model)
        {
            ResponseModel responseModel = new ResponseModel();
            if (!ModelState.IsValid)
            {
                responseModel.Status = String.Join(";", ModelState.Values.SelectMany(e => e.Errors).Select(m => m.ErrorMessage));
                return responseModel;
            }
            return _schedulerRepository.Save(model, GetUserId());
        }
        [HttpGet]
        public ResponseModel Delete(long id)
        {
            return _schedulerRepository.Delete(id, GetUserId());
        }
        [HttpGet]
        public ResponseModel GetProviderSchedules(long practiceCode, long providerCode, long locationCode)
        {
            return _schedulerRepository.GetProviderSchedules(practiceCode, providerCode, locationCode);
        }
        [HttpGet]
  
        public ResponseModel GetProviderSchedulesWithLocation( long practiceCode,string providerCodes, string locationCodes)
        {
            // Split the comma-separated values into lists
            var providerCodeList = providerCodes.Split(',').Select(long.Parse).ToList();
            var locationCodeList = locationCodes.Split(',').Select(long.Parse).ToList();

            return _schedulerRepository.GetProviderSchedulesWithLocation(practiceCode, providerCodeList, locationCodeList);
        }
        [HttpGet]
       public  ResponseModel GetOfficeTimingForProvidersAndLocations(long practiceCode, string providerCodes, string locationCodes, DateTime dateFrom, DateTime dateTo)
        {
            return _schedulerRepository.GetOfficeTimingForProvidersAndLocations(practiceCode, providerCodes, locationCodes, dateFrom, dateTo);
        }
        public ResponseModel GetProviderBlockedSchedules(long practiceCode, long providerCode, long locationCode)
        {
            return _schedulerRepository.GetProviderBlockedSchedules(practiceCode, providerCode, locationCode);
        }
        [HttpPost]
        public ResponseModel Timings(TimingSearchViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return new ResponseModel()
                {
                    Response = null,
                    Status = string.Join(";", ModelState.Values.SelectMany(e => e.Errors).Select(e => e.ErrorMessage))
                };
            }
            return _schedulerRepository.Timings(model, GetUserId());
        }
        [HttpPost]
        public ResponseModel Rules(ProviderAppointmentRuleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return new ResponseModel()
                {
                    Response = null,
                    Status = string.Join(";", ModelState.Values.SelectMany(e => e.Errors).Select(e => e.ErrorMessage))
                };
            }
            return _schedulerRepository.Rules(model, GetUserId());
        }
        [HttpPost]
        public ResponseModel GetProviderAppointmentRules(ProviderAppointmentRuleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return new ResponseModel()
                {
                    Response = null,
                    Status = string.Join(";", ModelState.Values.SelectMany(e => e.Errors).Select(e => e.ErrorMessage))
                };
            }
            return _schedulerRepository.GetProviderAppointmentRules(model, GetUserId());
        }
        [HttpPost]
        public ResponseModel SaveOfficeTimings(OfficeTimingCreateViewModel model)
        {
            ResponseModel responseModel = new ResponseModel();
            if (!ModelState.IsValid)
            {
                responseModel.Status = string.Join(";", ModelState.Values.SelectMany(e => e.Errors).Select(e => e.ErrorMessage));
                return responseModel;
            }
            return _schedulerRepository.SaveOfficeTimings(model, GetUserId());
        }
        [HttpPost]
        public ResponseModel SaveAppointmentRules(ProviderAppointmentRuleViewModel rule)
        {
            ResponseModel responseModel = new ResponseModel();
            if (!ModelState.IsValid)
            {
                responseModel.Status = string.Join(";", ModelState.Values.SelectMany(e => e.Errors).Select(e => e.ErrorMessage));
                return responseModel;
            }
            return _schedulerRepository.SaveAppointmentRules(rule, GetUserId());
        }
        [HttpGet]
        public ResponseModel GetProvidersAndLocations(long practiceCode)
        {
            ResponseModel responseModel = new ResponseModel();
            responseModel.Response = new ExpandoObject();
            responseModel.Status = "Success";
            responseModel.Response.Locations = _iDemographicRepository.GetLocationSelectList("", practiceCode, true).Response;
            responseModel.Response.Providers = _iDemographicRepository.GetProviderSelectList("", practiceCode, true).Response;
            return responseModel;
        }
        [HttpPost]
        public ResponseModel GetMatchingSchedules(ProviderSchedulesViewModel model)
        {
            ResponseModel responseModel = new ResponseModel();
            if (!ModelState.IsValid)
            {
                responseModel.Status = string.Join(";", ModelState.Values.SelectMany(e => e.Errors).Select(e => e.ErrorMessage));
                return responseModel;
            }
            return _schedulerRepository.GetMatchingSchedules(model, GetUserId());
        }
        [HttpGet]
        public ResponseModel Statuses()
        {
            return _schedulerRepository.Statuses(GetUserId());
        }
        [HttpGet]
        public ResponseModel GetPracticeAppointmentStatuses(long practiceCode)
        {
            return _schedulerRepository.GetPracticeAppointmentStatuses(practiceCode, GetUserId());
        }
        [HttpGet]
        public ResponseModel GetPracticeAppointmentStatuse(long practiceCode, long id)
        {
            return _schedulerRepository.GetPracticeAppointmentStatus(practiceCode, GetUserId(), id);
        }
        [HttpGet]
        public ResponseModel DeletePracticeAppointmentStatuse(long id)
        {
            return _schedulerRepository.DeletePracticeAppointmentStatus(id, GetUserId());
        }
        [HttpPost]
        public ResponseModel SaveStatus(PracticeAppointmentStatusCreateVM model)
        {
            if (!ModelState.IsValid)
            {
                return new ResponseModel()
                {
                    Status = string.Join(";", ModelState.Values.SelectMany(m => m.Errors).Select(m => m.ErrorMessage))
                };
            }
            return _schedulerRepository.SaveStatus(model, GetUserId());
        }
        [HttpGet]
        public ResponseModel GetPracAppointmentReasons(long practiceCode, long providerCode, long locationCode)
        {
            return _schedulerRepository.GetPracAppointmentReasons(practiceCode, providerCode, locationCode);
        }
        [HttpGet]
        public ResponseModel GetAppointmentReasons(string searchText = "", bool all = false)
        {
            return _schedulerRepository.GetAppointmentReasons(searchText, all);
        }
        [HttpGet]
        public ResponseModel GetUnassignedAppointmentReasons(long practiceCode, long providerCode, long locationCode, string searchText = "")
        {
            return _schedulerRepository.GetUnassignedAppointmentReasons(practiceCode, providerCode, locationCode, searchText);
        }
        [HttpPost]
        public ResponseModel SaveReason(PracticeAppointmentReasonCreateVM model)
        {
            return _schedulerRepository.SaveReason(model, GetUserId());
        }
        [HttpPost]
        public ResponseModel ChangeColor(ColorChangeModel model)
        {
            if (!ModelState.IsValid)
            {
                return new ResponseModel()
                {
                    Status = string.Join(";", ModelState.Values.SelectMany(e => e.Errors).Select(e => e.ErrorMessage))
                };
            }
            return _schedulerRepository.ChangeColor(model.PracAppReasonID, model.ReasonColor, GetUserId());
        }
        [HttpGet]
        public ResponseModel DeletePracticeAppointmentReason(long id)
        {
            return _schedulerRepository.DeletePracticeAppointmentReason(id, GetUserId());
        }
        [HttpPost]
        public ResponseModel SlotAvailability(SlotAvailabilityRequestVM model)
        {
            if (!ModelState.IsValid)
            {
                return new ResponseModel()
                {
                    Status = string.Join(";", ModelState.Values.SelectMany(m => m.Errors).Select(m => m.ErrorMessage))
                };
            }
            return _schedulerRepository.SlotAvailability(model, GetUserId());
        }
        [HttpGet]
        public ResponseModel InquiryByAppointmentId(long PracticeCode, long AppointmentId)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                AppointmentViewModel appointment = _schedulerRepository.GetAppointmentModel(PracticeCode, AppointmentId).Response;
                if (appointment != null)
                {
                    SPAppChkEligibility_Result eligibilityModel = _iEligibility.GetEligibilityModel(PracticeCode, (long)appointment.PatientAccount, (long)appointment.ProviderCode, 0).Response;
                    if (eligibilityModel != null)
                    {
                        WSEligibilityResponse response = _iEligibility.DoInquiry(new DoInquiryModel()
                        {
                            GediPayerID = eligibilityModel.GediPayerID ?? "",
                            InsuranceNum = eligibilityModel.InsuranceNum ?? "",
                            InsuredDOB = eligibilityModel.InsuredDob != null ? ((DateTime)eligibilityModel.InsuredDob).ToString("yyyyMMdd") : "",
                            InsuredFirstName = eligibilityModel.InsuredFirstName ?? "",
                            InsuredLastName = eligibilityModel.InsuredLastName ?? "",
                            InsuredSSN = eligibilityModel.Insuredssn ?? "",
                            InsuredState = eligibilityModel.InsuredState ?? "",
                            Npi = eligibilityModel.NPI ?? "",
                            ProviderId = eligibilityModel.PROVIDERID.ToString(),
                            ProviderFirstName = eligibilityModel.ProviderFirstName.ToString(),
                            ProviderLastName = eligibilityModel.ProviderLastName.ToString()

                        }, (long)eligibilityModel.Practice_Code);
                        responseModel.Status = "success";
                        using (var ctx = new NPMDBEntities())
                        {
                            var appointToUpdate = ctx.Appointments.FirstOrDefault(a => a.Appointment_Id == AppointmentId);
                            if (appointToUpdate != null)
                            {
                                if (response != null && response.SuccessCode == SuccessCode.Success)
                                {
                                    appointToUpdate.eligibilityColor = "Green";
                                }
                                else
                                {
                                    appointToUpdate.eligibilityColor = "Red";
                                }
                                ctx.Entry(appointToUpdate).State = System.Data.Entity.EntityState.Modified;
                                ctx.SaveChanges();
                                responseModel.Response = appointToUpdate.eligibilityColor;
                            }
                        }
                        return responseModel;
                    }
                    else
                    {
                        responseModel.Status = "error";
                        responseModel.Response = "No Information found for patient.";
                    }
                }
                else
                {
                    responseModel.Status = "error";
                    responseModel.Response = "No Appointment found.";
                }
            }
            catch (Exception ex)
            {
                responseModel.Status = "error";
                responseModel.Response = ex;
            }
            return responseModel;
        }

        [HttpGet]
        public ResponseModel CheckDeceasedPatient(long PatAccount)
        {
            ResponseModel res = new ResponseModel();
            if (PatAccount > 0)
            {
                return _schedulerRepository.CheckDeceasedPatient(PatAccount);
            }
            else
            {
                res.Status = "error";
                res.Response = "No/Invalid patient account";
                return res;
            }
        }


    }
}
