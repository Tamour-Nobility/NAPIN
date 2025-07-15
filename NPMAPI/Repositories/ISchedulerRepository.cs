using NPMAPI.Models;
using NPMAPI.Models.ViewModels;
using System;
using System.Collections.Generic;

namespace NPMAPI.Repositories
{
    public interface ISchedulerRepository
    {
        ResponseModel GetAppointments(AppointmentsSearchViewModel model);
        ResponseModel GetExistingAppointments(AppointmentCreateModel model);
        ResponseModel GetProviderAppointmentSettings(long providerCode);
        ResponseModel GetAppointmentModel(long practiceCode, long? id);
        ResponseModel GetReasonsSelectList(string searchText);
        ResponseModel Save(AppointmentCreateModel model, long userId);
        ResponseModel Delete(long id, long userId);
        ResponseModel Timings(TimingSearchViewModel model, long userId);
        ResponseModel SaveOfficeTimings(OfficeTimingCreateViewModel model, long userId);
        ResponseModel GetProviderSchedules(long practiceCode, long providerCode, long locationCode);
        ResponseModel GetProviderSchedulesWithLocation(long practiceCode, List<long> providerCodes, List<long> locationCodes);
        ResponseModel GetOfficeTimingForProvidersAndLocations(long practiceCode, string providerCodes, string locationCodes, DateTime dateFrom, DateTime dateTo);
        ResponseModel GetMatchingSchedules(ProviderSchedulesViewModel model, long userId);
        ResponseModel Statuses(long userId);
        ResponseModel SaveStatus(PracticeAppointmentStatusCreateVM model, long userId);
        ResponseModel GetPracticeAppointmentStatuses(long practiceCode, long userId);
        ResponseModel GetPracticeAppointmentStatus(long practiceCode, long userId, long practiceAppointmentStatusId);
        ResponseModel DeletePracticeAppointmentStatus(long id, long userId);
        ResponseModel GetPracAppointmentReasons(long practiceCode, long providerCode, long locationCode);
        ResponseModel GetAppointmentReasons(string searchText = "", bool all = false);
        ResponseModel GetUnassignedAppointmentReasons(long practiceCode, long providerCode, long locationCode, string searchText);
        ResponseModel SaveReason(PracticeAppointmentReasonCreateVM model, long userId);
        ResponseModel ChangeColor(long PracAppReasonID, string ReasonColor, long userId);
        ResponseModel DeletePracticeAppointmentReason(long id, long userId);
        ResponseModel SlotAvailability(SlotAvailabilityRequestVM model, long v);
        ResponseModel GetProviderBlockedSchedules(long practiceCode, long providerCode, long locationCode);
        ResponseModel SaveAppointmentRules(ProviderAppointmentRuleViewModel rule, long v);
        ResponseModel GetProviderAppointmentRules(ProviderAppointmentRuleViewModel model, long v);
        ResponseModel Rules(ProviderAppointmentRuleViewModel model, long v);
        ResponseModel CheckDeceasedPatient(long PatAccount);

    }
}
