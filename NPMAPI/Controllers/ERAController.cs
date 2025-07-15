using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using NPMAPI.Models;
using NPMAPI.Repositories;
using static NPMAPI.Services.ERAImportService;

namespace NPMAPI.Controllers
{
    public class ERAController : ApiController
    {
        private readonly IPracticeRepository _practiceService;
        private readonly IERAImport _iERAImport;

        public ERAController(IPracticeRepository practiceService, IERAImport iERAImport)
        {
            _practiceService = practiceService;
            _iERAImport = iERAImport;
        }

        [HttpPost]
        public async Task<ResponseModel> Import(ERA_Request ERA)
        {
            #region DeclareVariables!

            #region SFTP Variables!
            //For SMTP Server:
            //SMTP server settings
            string smtpHost = ConfigurationManager.AppSettings["smtp"];
            int smtpPort = Convert.ToInt32(ConfigurationManager.AppSettings["portnumber"]);
            string smtpUsername = ConfigurationManager.AppSettings["username"];
            string smtpPassword = ConfigurationManager.AppSettings["password"];
            //string body = "";
            // Email recipient
            string emailRecipient = "h.mehmood @nobilityrcm.com";
            //In CC
            string TL = "mabbas@nobilityrcm.com";
            string AM = "a.farooq@nobilityrcm.com";
            // Email subject and body
            string subject = "Manual ERA Request Stats – " + DateTime.Now;
            string statsHead = "Manual ERA Request Stats".ToUpper();
            string disclaimer = "Disclaimer:".ToUpper();
            #endregion End!

            ResponseModel res = new ResponseModel();
            ReturnERAResult stats = new ReturnERAResult();
            ReturnERAResult User = new ReturnERAResult();
            #endregion End!

            #region User_ERa_Request_Insertion_And_Set_Log_Name!
            User = await _iERAImport.Era_User_Request(ERA);
            //string Logfile = "";
            //Logfile += User.RM.Response.LogFile_Path + "\\" + User.RM.Response.LogFile_Name;
            #endregion End!

            if (User.RM.Status == "Successfull")
            {
                #region DownloadAndDump_LogMaintain!
                stats = await _iERAImport.ERA_Download_and_Dump_Process(User.RM.Response.USER_NAME, User.message, ERA);
                res.Status = stats.RM.Status;
                res.Response = stats.RM.Response;
                #endregion End!


            }
            #region Successfull! Already Requested For Same Practice Code!
            else if (User.RM.Status == "Successfull! Already Same Person Requested For Same Practice Code" || User.RM.Status == "Successfull! Already Different Person Requested For Same Practice Code")
            {

                #region Set_Status
                res.Status = "Successfull";
                res.Response = User.RM.Response;
                #endregion End

                #region SMTP Main MAil to EDI Team
                #region variable declaration
                string requestFor = $"Request For Manual ERA Downloading for this [{ERA.PracticeCde}] Practice Code.";
                var emailBody = @"<html>
                    <head>
                <style>
                    table {
                        border-collapse: collapse;
                        width: 100%;
                    }

                    th, td {
                        text-align: left;
                        padding: 8px;
                        border-bottom: 1px solid white;
                    }

                    th {
                        background-color: white;
                        width:150px;
                    }
                </style>
            </head>
            <body>
                <h2>" + statsHead + @"</h2>
                <table>
                    <tr>
                        <th>System Username</th>
                        <td>" + Environment.UserName + @"</td>
                    </tr>
                    <tr>
                        <th>System Name</th>
                        <td>" + Environment.MachineName + @"</td>
                    </tr>
                    <tr>
                        <th>Requested Username</th>
                        <td>" + ERA.UserName + @"</td>
                    </tr>
                    <tr>
                        <th>Requested For Practice Code</th>
                        <td>" + ERA.PracticeCde + @"</td>
                    </tr>
                    <tr>
                        <th>Current Time</th>
                        <td>" + DateTime.Now + @"</td>
                    </tr>
                    <tr>
                        <th>FTP Exception</th>
                        <td>Already in Process for this [" + ERA.PracticeCde + @"] Practice Code.<br/>Please Try Again For Different Practice!</td>
                    </tr>
                    <tr>
                        <th>Service Name</th>
                        <td>" + requestFor + @"</td>
                    </tr>
                    <!-- Add more rows for additional statistics -->
                </table>
                <p>
                    <br/>" + disclaimer + @"<br/>Please do not reply to this email as it is an automated notification. If you require further assistance, please feel free to reach out to our support team or EDI Team.<br/>Thank you for your understanding and cooperation.<br/><br/>Best regards,<br/>NOBILITY MBS<br/>Notification Service
                </p>
            </body>
            </html>";


                #endregion End!


                string body = $"{emailBody}";

                using (SmtpClient client1 = new SmtpClient(smtpHost, smtpPort))
                {
                    client1.EnableSsl = true;

                    client1.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                    MailMessage mailMessage = new MailMessage();
                    mailMessage.IsBodyHtml = true;
                    mailMessage.From = new MailAddress(smtpUsername);
                    mailMessage.To.Add(emailRecipient);
                    mailMessage.CC.Add(TL);
                    mailMessage.CC.Add(AM);
                    mailMessage.Subject = subject;
                    mailMessage.Body = body;
                    #region if you want to attach a file with it!
                    //// Create the attachment
                    //Attachment attachment = new Attachment(Logpath);
                    ////// Add the attachment to the email
                    //mailMessage.Attachments.Add(attachment);
                    #endregion

                    try
                    {
                        // Send the email
                        await client1.SendMailAsync(mailMessage);
                    }
                    catch (SmtpException ex)
                    {
                        // Handle any exceptions that occur during email sending
                        // Log or perform any necessary error handling
                        User.message += "\t\t\t\t\t\t\t\t>>>-------------------------Problem Encountered During Email--------------------------------\n";
                        User.message += "\t\t\t\t\t\t\t\tDate" + DateTime.Now;
                        User.message += "\n\t\t\t\t\t\t\t\tFailed to Send Email\r\nException Found: " + ex.Message;
                        User.message += "\n";
                        User.message += "\t\t\t\t\t\t\t\tStack Trace: " + ex.StackTrace;
                        User.message += "\n\t\t\t\t\t\t\t\t>>>---------------------------------------------------------";

                    }
                }
                #endregion SMTP client setup done

            }
            #endregion End!

            #region ReturnResult!
            return res;
            #endregion End!
        }


        //[HttpPost]
        //public IHttpActionResult Import()
        //{
        //    var ftps = _practiceService.GetFTPEnabledPractices();
        //    List<DownloadedFile> downloadedFiles = new List<DownloadedFile>();
        //    if (ftps.Count() > 0)
        //    {
        //        downloadedFiles = _iERAImport.GetPreviouseDownloadedFiles(ftps.Select(ftp => ftp.PracticeCode).ToList());
        //        foreach (var ftp in ftps)
        //        {
        //            _iERAImport.Download(
        //                ftp.PracticeCode,
        //                ftp.Username,
        //                ftp.Password,
        //                ftp.Host,
        //                ftp.Port,
        //                ConfigurationManager.AppSettings["ERADownloadSource"],
        //                downloadedFiles.Where(df => df.PracticeCode == ftp.PracticeCode).ToList());
        //            _iERAImport.ParseAndDump(ftp.PracticeCode);
        //        }
        //    }
        //    return Ok();
        //}



        [HttpPost]
        public ResponseModel WeekHistoryOfERA(CheckPracticeCode Search)
        {
            var result = new ResponseModel();

            result = _iERAImport.SearchWeeklyHistory(Search.practiceCode);
            return result;
        }

        [HttpGet]
        public IHttpActionResult LogException()
        {
            throw new NotImplementedException();
        }
    }
}
