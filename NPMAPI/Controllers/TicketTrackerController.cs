using NPMAPI.Models;
using NPMAPI.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;


namespace NPMAPI.Controllers
{
    public class TicketTrackerController : BaseController
    {
        private readonly ITicketTrackerRrepo _tickerservice;
        public TicketTrackerController(ITicketTrackerRrepo tickerservice)
        {
            _tickerservice = tickerservice;
        }
        [HttpGet]
        public ResponseModel GetPatientDueAmt(string patientacc)
        {
            try
            {
                return _tickerservice.GetPatientDueAmt(patientacc);
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
        [HttpGet]
        public ResponseModel GetRenderingProvider(long providercode)
        {
            try
            {
                return _tickerservice.GetRenderingProvider(providercode);
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
        [HttpGet]
        public ResponseModel GetClaimInfo(string claimno)
        {
            try
            {
                return _tickerservice.GetClaimInfo(claimno);
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
        [HttpGet]
        public ResponseModel GetPatientInfo(string patientacc)
        {
            try
            {
                return _tickerservice.GetPatientInfo(patientacc);
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
        [HttpGet]
        public ResponseModel GetPatientAccounts(long practicecode)
        {
            try
            {
                return _tickerservice.GetPatientAccounts(practicecode);
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
        [HttpGet]
        public ResponseModel GetPracticeInfo(long practicecode)
        {
            try
            {
                return _tickerservice.GetPracticeInfo(practicecode);
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
        [HttpGet]
        public ResponseModel GetTTDepartmentsList()
        {
            try
            {
                return _tickerservice.GetTTDepartmentsList();
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
        [HttpPost]
        public ResponseModel SaveTicket([FromBody] Tickettracker model)
        {
            try
            {
                var response = _tickerservice.SaveTicket(model);
                return response; 
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
        [HttpPost]
        public ResponseModel SearchTicket(TicketSearchModel model)
        {
            try
            {
                var result = _tickerservice.SearchTicket(model);
                return result;
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
            try
            {
                var result = _tickerservice.TicketAssignment(model);
                return result;
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
        public ResponseModel GetTicketById(long ticketid)
        {
            try
            {
                var result = _tickerservice.GetTicketById(ticketid);
                return result;
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
        public ResponseModel GetTicketTrackById(long ticketid)
        {
            try
            {
                var result = _tickerservice.GetTicketTrackById(ticketid);
                return result;
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
        [HttpGet]
        public ResponseModel GetProviderList(long practicecode)
        {
            try
            {
                return _tickerservice.GetProviderList(practicecode);
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
        [HttpPost]
        public ResponseModel SaveTrackDetails(SaveTrackDetails model)
        {
            try
            {
                var result = _tickerservice.SaveTrackDetails(model);
                return result;
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
        public ResponseModel EditTrackDetails(TicketMessageDetail model)
        {
            try
            {
                var result = _tickerservice.EditTrackDetails(model);
                return result;
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
        [HttpPost]
        public ResponseModel DeleteTrackDetails([FromBody]  TicketMessageDetail model)
        {
            try
            {
                var result = _tickerservice.DeleteTrackDetails(model);
                return result;
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
        public ResponseModel GetAssignedUser(long practicecode)
        {
            try
            {
                var result = _tickerservice.GetAssignedUser(practicecode);
                return result;
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