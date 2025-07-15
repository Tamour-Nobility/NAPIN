using NPMAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace NPMAPI.Repositories
{
   public interface ITicketTrackerRrepo
    {
        ResponseModel GetTTDepartmentsList();
        ResponseModel GetPracticeInfo(long practicecode);
        ResponseModel GetPatientAccounts(long practicecode);
        ResponseModel GetPatientInfo(string patientacc);
        ResponseModel GetClaimInfo(string claimno);
        ResponseModel GetRenderingProvider(long providercode);
        ResponseModel GetPatientDueAmt(string patientacc);
        ResponseModel SaveTicket(Tickettracker model);
        ResponseModel SearchTicket(TicketSearchModel model);
        ResponseModel TicketAssignment(TicketAssignmentRequest model);
        ResponseModel GetTicketById(long ticketid);
        ResponseModel GetTicketTrackById(long ticketid);
        ResponseModel GetProviderList(long practicecode);
        ResponseModel SaveTrackDetails(SaveTrackDetails model);
        ResponseModel EditTrackDetails(TicketMessageDetail model);
        ResponseModel DeleteTrackDetails(TicketMessageDetail model);
        ResponseModel GetAssignedUser(long practicecode);

    }
}
