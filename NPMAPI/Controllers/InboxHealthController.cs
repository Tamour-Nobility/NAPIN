using System;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;
using NPMAPI.Enums;
using NPMAPI.Models.InboxHealth;
using NPMAPI.Repositories;

namespace NPMAPI.Controllers
{
    public class InboxHealthController : ApiController
    {
        private readonly IPatientBilling _patientBilling;
        public InboxHealthController(IPatientBilling patientBilling)
        {
            _patientBilling = patientBilling;
        }

        /// <summary>
        /// Inbox Health Webhook Receiver
        /// </summary>
        /// <returns>IHttpActionResult OK(200) on Success and BadRequest() on failure of task</returns>
        [HttpPost]
        public async Task<IHttpActionResult> Webhook()
        {
            try
            {
            var content = JsonConvert.DeserializeObject<InboxHealthEvent>(await Request.Content.ReadAsStringAsync());
            if (HandleEvent(content) > 0)
                return StatusCode(System.Net.HttpStatusCode.OK);
            else
                return BadRequest();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
                throw;
            }
        }

        private int HandleEvent(InboxHealthEvent content)
        {
            switch (content.EventType)
            {
                case eInboxHealthEvent.patient_payment_processed:
                    {
                        return _patientBilling.onPatientPaymentProcessed(content.EventData);
                    }
                case eInboxHealthEvent.invoice_payment_created:
                    {
                        return _patientBilling.onPatientPaymentProcessed(content.EventData);
                    }
                default:
                    return 0;
            }
        }
    }
}
