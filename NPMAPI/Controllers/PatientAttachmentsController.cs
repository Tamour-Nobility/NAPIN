using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using NPMAPI.App_Start;
using NPMAPI.Models;
using NPMAPI.Models.ViewModels;
using NPMAPI.Repositories;

namespace NPMAPI.Controllers
{
    public class PatientAttachmentsController : BaseController
    {
        private readonly IFileHandler _fileHandler;
        private readonly IPatientAttachment _patientAttachment;
        public PatientAttachmentsController(IFileHandler fileHandler, IPatientAttachment patientAttachment)
        {
            _fileHandler = fileHandler;
            _patientAttachment = patientAttachment;
        }

        [HttpPost]
        public IHttpActionResult Attach()
        {

            try
            {
                string typeCode = HttpContext.Current.Request.Form["TypeCode"];
                string patientAccount = HttpContext.Current.Request.Form["PatientAccount"];
                if (string.IsNullOrEmpty(typeCode))
                    return BadRequest("Please provide TypeCode field");
                if (string.IsNullOrEmpty(patientAccount))
                    return BadRequest("Please provide PatientAccount field");
                string fileNewName = $"{(Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds}{Guid.NewGuid().ToString()}";
                var fileUploadResponse = _fileHandler.UploadImage(
                      HttpContext.Current.Request.Files[0],
                      HttpContext.Current.Server.MapPath($"~/{ConfigurationManager.AppSettings["PatientAttachments"]}/{fileNewName}"),
                      new string[] {
                          ".jpg",
                          ".jpeg",
                          ".png",
                          ".gif",
                          ".jfif",
                          ".doc",
                          ".docx",
                          ".csv",
                          ".pdf",
                          ".xls",
                          ".xlsx",
                          ".txt"
                      },
                      fileNewName,
                      GlobalVariables.MaximumPatientAttachmentSize);
                if (fileUploadResponse.Status == "success")
                {
                    var attachmentResponse = _patientAttachment.Save(new CreateAttachmentRequest()
                    {
                        Attachment_TypeCode_Id = Convert.ToInt32(typeCode),
                        FileName = HttpContext.Current.Request.Files[0].FileName,
                        FilePath = fileUploadResponse.Response,
                        Patient_Account = Convert.ToInt64(patientAccount)
                    }, GetUserId());
                    return Ok(attachmentResponse);
                }
                else
                {
                    return Ok(fileUploadResponse);
                }
            }
            catch (Exception)
            {
                throw;
            }




        }

        [HttpGet]
        public IHttpActionResult Delete(long id)
        {
            return Ok(_patientAttachment.Delete(id, GetUserId()));
        }

        [HttpGet]
        public IHttpActionResult GetAll(long patientAccount)
        {
            return Ok(_patientAttachment.GetAll(patientAccount));
        }

        [HttpGet]
        public IHttpActionResult GetAttachmentCodeList()
        {
            return Ok(_patientAttachment.GetAttachmentTypeCodesList());
        }
        [HttpGet]
        public HttpResponseMessage Download(long id)
        {
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
            var fileInfo = _patientAttachment.Get(id);
            string filePath;
            if (fileInfo.Status == "success" && fileInfo != null)
            {
                filePath = HttpContext.Current.Server.MapPath($"~/{ConfigurationManager.AppSettings["PatientAttachments"] + "/" + fileInfo.Response.FilePath}");
                if (!File.Exists(filePath))
                {
                    response.StatusCode = HttpStatusCode.NotFound;
                    response.ReasonPhrase = string.Format("File not found: {0} .", fileInfo.Response.FileName);
                    return response;
                }
                byte[] bytes = File.ReadAllBytes(filePath);
                response.Content = new ByteArrayContent(bytes);
                response.Content.Headers.ContentLength = bytes.LongLength;
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                response.Content.Headers.ContentDisposition.FileName = fileInfo.Response.FileName;
                response.Content.Headers.ContentType = new MediaTypeHeaderValue(MimeMapping.GetMimeMapping(fileInfo.Response.FileName));
                return response;
            }
            else
            {
                response.StatusCode = HttpStatusCode.NotFound;
                response.ReasonPhrase = string.Format("File not found: {0} .", fileInfo.Response.FileName);
                return response;
            }
        }
        // [HttpPost]
        // public IHttpActionResult TicketAttach()
        //{
        //try
        //{

        //    string typeCode = HttpContext.Current.Request.Form["TypeCode"];
        //    string patientAccount = HttpContext.Current.Request.Form["PatientAccount"];
        //    if (string.IsNullOrEmpty(typeCode))
        //        return BadRequest("Please provide TypeCode field");
        //    if (string.IsNullOrEmpty(patientAccount))
        //        return BadRequest("Please provide PatientAccount field");

        //    string fileNewName = $"{(Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds}{Guid.NewGuid().ToString()}";
        //    var fileUploadResponse = _fileHandler.UploadImage(
        //          HttpContext.Current.Request.Files[0],
        //          HttpContext.Current.Server.MapPath($"~/{ConfigurationManager.AppSettings["TicketAttachments"]}/{fileNewName}"),
        //          new string[] {
        //              ".jpg",
        //              ".jpeg",
        //              ".png",
        //              ".gif",
        //              ".jfif",
        //              ".doc",
        //              ".docx",
        //              ".csv",
        //              ".pdf",
        //              ".xls",
        //              ".xlsx",
        //              ".txt"
        //          },
        //          fileNewName,
        //          GlobalVariables.MaximumPatientAttachmentSize);
        //    if (fileUploadResponse.Status == "success")
        //    {
        //        var attachmentResponse = _patientAttachment.Save(new CreateAttachmentRequest()
        //        {
        //            //Attachment_TypeCode_Id = Convert.ToInt32(typeCode),
        //            FileName = HttpContext.Current.Request.Files[0].FileName,
        //            FilePath = fileUploadResponse.Response,
        //            //Patient_Account = Convert.ToInt64(patientAccount)
        //        }, GetUserId());
        //        return Ok(attachmentResponse);
        //    }
        //    else
        //    {
        //        return Ok(fileUploadResponse);
        //    }
        //}
        //catch (Exception ex)
        //{
        //    throw ex;
        //}
        //}

        [HttpPost]
        public async Task<IHttpActionResult> UploadFiles()
        {
            if (!Request.Content.IsMimeMultipartContent())
                return BadRequest("Unsupported media type");

            string ticketId = HttpContext.Current.Request.Form["TicketId"];

            if (string.IsNullOrEmpty(ticketId))
                return BadRequest("Missing required fields");

            var supportedTypes = new string[]
            {
        ".jpg", ".jpeg", ".png", ".gif", ".jfif", ".doc", ".docx", ".csv", ".pdf", ".xls", ".xlsx", ".txt"
            };

            var files = HttpContext.Current.Request.Files;

            for (int i = 0; i < files.Count; i++)
            {
                var file = files[i];

                if (file?.ContentLength > 0)
                {
                    //string extension = Path.GetExtension(file.FileName);
                    //string fileNewName = $"{(Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds}_{Guid.NewGuid()}{extension}";
                    //string fullPath = HttpContext.Current.Server.MapPath($"~/{ConfigurationManager.AppSettings["TicketAttachments"]}/{fileNewName}");

                    //var fileUploadResponse = _fileHandler.UploadImage(
                    //    file,
                    //    HttpContext.Current.Server.MapPath($"~/{ConfigurationManager.AppSettings["TicketAttachments"]}/"),
                    //    supportedTypes,
                    //    Path.GetFileNameWithoutExtension(fileNewName),
                    //    GlobalVariables.MaximumPatientAttachmentSize
                    //);


                    string extension = Path.GetExtension(file.FileName).ToLower();

                    // Check if extension is supported
                    if (!supportedTypes.Contains(extension))
                        return BadRequest("Unsupported file type.");

                    // Generate new file name
                    string fileNewName = $"{(Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds}_{Guid.NewGuid()}";
                    string fullSaveDirectory = HttpContext.Current.Server.MapPath($"~/{ConfigurationManager.AppSettings["TicketAttachments"]}/");

                    var fileUploadResponse = _fileHandler.UploadTicketImage(
                        file,
                        fullSaveDirectory,
                        supportedTypes,
                        fileNewName, // Correct use of generated name without extension
                        GlobalVariables.MaximumPatientAttachmentSize
                    );


                    if (fileUploadResponse.Status == "success")
                    {
                        var attachmentResponse = _patientAttachment.SaveTicketAttachment(new CreateAttachmentRequest()
                        {
                            Ticket_id = Convert.ToInt32(ticketId),
                            FileName = file.FileName,
                            FilePath = fileUploadResponse.Response,
                        }, GetUserId());

                        // Optional: continue loop or return one-by-one result
                    }
                    else
                    {
                        return Ok(fileUploadResponse); // Return first failure immediately
                    }
                }
            }

            return Ok(new { Status = "success", Message = "Files uploaded successfully." });
        }







    }
}
