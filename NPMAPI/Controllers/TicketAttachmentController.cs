//using NPMAPI.App_Start;
//using NPMAPI.Models;
//using NPMAPI.Models.ViewModels;
//using NPMAPI.Repositories;
//using System;
//using System.Collections.Generic;
//using System.Configuration;
//using System.Linq;
//using System.Net;
//using System.Net.Http;
//using System.Web;
//using System.Web.Http;

//namespace NPMAPI.Controllers
//{
//    public class TicketAttachmentController : BaseController
//    {
//        private readonly IFileHandler _fileHandler;
//        private readonly ITicketAttachment _ticketAttachment;
//        public TicketAttachmentController(IFileHandler fileHandler, ITicketAttachment ticketAttachment)
//        {
//            _fileHandler = fileHandler;
//            _ticketAttachment = ticketAttachment;
//        }
//        [HttpPost]
//        public IHttpActionResult TicketAttach()
//        {
//            try
//            {
//                var responseList = new List<ResponseModel>();
//                var files = HttpContext.Current.Request.Files;

//                System.Diagnostics.Debug.WriteLine("File count: " + files.Count);
//                if (files.Count == 0)
//                {
//                    return BadRequest("No files uploaded.");
//                }

//                for (int i = 0; i < files.Count; i++)
//                {
//                    var file = files[i];

//                    // Generate unique file name
//                    string fileNewName = $"{(Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds}{Guid.NewGuid()}";

//                    var fileUploadResponse = _fileHandler.UploadImage(
//                        file,
//                        HttpContext.Current.Server.MapPath($"~/{ConfigurationManager.AppSettings["TicketAttachments"]}/{fileNewName}"),
//                        new string[] {
//                    ".jpg", ".jpeg", ".png", ".gif", ".jfif",
//                    ".doc", ".docx", ".csv", ".pdf", ".xls", ".xlsx", ".txt"
//                        },
//                        fileNewName,
//                        GlobalVariables.MaximumPatientAttachmentSize);

//                    if (fileUploadResponse.Status == "success")
//                    {
//                        var saveResponse = _ticketAttachment.Save(new CreateAttachmentRequest
//                        {
//                            FileName = fileNewName,
//                            FilePath = fileUploadResponse.Response
//                        }, GetUserId());

//                        responseList.Add(saveResponse);
//                    }
//                    else
//                    {
//                        responseList.Add(fileUploadResponse); // Include failed upload result too
//                    }
//                }

//                return Ok(responseList); // Return the result of all files
//            }
//            catch (Exception ex)
//            {
//                return InternalServerError(ex);
//            }
//        }


//    }
//}

