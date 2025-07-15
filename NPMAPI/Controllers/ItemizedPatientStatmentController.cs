using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using NPMAPI.Models;
using NPMAPI.Repositories;

namespace NPMAPI.Controllers
{
    public class ItemizedPatientStatmentController : BaseController
    {
        private readonly IPDFRepository pdf;
        public ItemizedPatientStatmentController(IPDFRepository _pdf)
        {
            pdf = _pdf;
        }

        [HttpGet]
        public HttpResponseMessage GenerateItemizedPsForPrint(long patientAccount, long practiceCode, string messageToPrint)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                byte[] buffer = pdf.GenerateItemizedPatientStatment(patientAccount, practiceCode, GetUserId(), messageToPrint);
                if (buffer.Length > 0)
                {
                    response.StatusCode = HttpStatusCode.OK;
                    response.Content = new StreamContent(new MemoryStream(buffer));
                    response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                    response.Content.Headers.ContentLength = buffer.Length;
                    response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                    {
                        FileName = "myFirstPDF.pdf"
                    };
                }
                else
                {
                    response.StatusCode = HttpStatusCode.NoContent;
                }
                return response;
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost]
        public HttpResponseMessage GenerateItemizedPsForDownload(StatmentDownloadRequestModel model )
        {
            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                byte[] buffer = pdf.GenerateItemizedPsForDownload(model, GetUserId());
                if (buffer.Length > 0)
                {
                    response.StatusCode = HttpStatusCode.OK;
                    response.Content = new StreamContent(new MemoryStream(buffer));
                    response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                    response.Content.Headers.ContentLength = buffer.Length;
                    response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                    {
                        FileName = "myFirstPDF.pdf"
                    };
                }
                else
                {
                    response.StatusCode = HttpStatusCode.NoContent;
                }
                return response;
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet]
        public HttpResponseMessage GeneraterollingForDownload(string prac_code , string duration)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                byte[] buffer = pdf.GenerateRollingFordowloadForDownload(prac_code, duration);
                if (buffer.Length > 0)
                {
                    response.StatusCode = HttpStatusCode.OK;
                    response.Content = new StreamContent(new MemoryStream(buffer));
                    response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                    response.Content.Headers.ContentLength = buffer.Length;
                    response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                    {
                        FileName = "myFirstPDF.pdf"
                    };
                }
                else
                {
                    response.StatusCode = HttpStatusCode.NoContent;
                }
                return response;
            }
            catch (Exception)
            {
                throw;
            }
        }




        [HttpGet]
        public ResponseModel GetAllMessages()
        {

            return pdf.GetAllMessages();
        }

        [HttpPost]
        public ResponseModel AddMessage(PatientStatementMessages Model)
        {
            return pdf.AddMessage(Model, GetUserId());

        }


        [HttpPost]
        public ResponseModel EditMessage(PatientStatementMessages Model)
        {
            return pdf.EditMessage(Model, GetUserId());

        }

        [HttpGet]
        public ResponseModel DeleteMessage(long MessageID)
        {
            return pdf.DeleteMessage(MessageID);
        }
    }
}
