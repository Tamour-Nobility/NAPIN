using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web.Http.Results;
using Microsoft.AspNet.SignalR;
using NPMAPI.Models;
using NPMAPI.Repositories;
using OopFactory.X12.Parsing;
using Polly;
using OopFactory.X12.Repositories;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Ocsp;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using static NPOI.HSSF.Util.HSSFColor;
using System.Threading;
using iTextSharp.text;
using System.Net.Http;
using Renci.SshNet.Async;
using NPOI.SS.Formula.Functions;
using System.Collections.Concurrent;
using Renci.SshNet.Security;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Core.Common.EntitySql;
using Microsoft.AspNet.SignalR.Infrastructure;
using static NPOI.SS.UserModel.MissingCellPolicy;
using Microsoft.AspNet.SignalR.Messaging;
using CommandType = System.Data.CommandType;

namespace NPMAPI.Services
{
    public class ERAImportService : IERAImport
    {
        private readonly IPracticeRepository _Practice;
        public ERAImportService(IPracticeRepository practice)
        {
            _Practice = practice;
        }
        public struct ReturnERAResult
        {
            public ResponseModel RM;
            public string FirstDownloadedFileName;
            public string LastDownloadedFileName;
            public string message;
            //public DateTime FirstFileDownloadServerTime;
            //public DateTime LastFileDownloadServerTime;
        }
        private async Task<ReturnERAResult> Download1(long practiceCode, string username, string password, StreamWriter writer, string ERAUserName, string host, int port, string path, User_era_request UserRequest, List<DownloadedFile> previousDownloadedFiles, string logs)
        {
            #region Create_Variables_Also_Set_TimeSpan!
            ReturnERAResult FinalREsult = new ReturnERAResult();
            FinalREsult.message = logs;
            FinalREsult.RM = new ResponseModel();
            //FinalREsult.FirstFileDownloadServerTime = DateTime.Now;
            //FinalREsult.LastFileDownloadServerTime = DateTime.Now;
            FinalREsult.FirstDownloadedFileName = "";
            FinalREsult.LastDownloadedFileName = "";
            var res = new ResponseModel();
            int maxRetryCount = 3;
            TimeSpan retryInterval = TimeSpan.FromSeconds(15);
            int retryCount = 0;
            bool success = false;
            string downloadResult = "";
            List<DownloadedFile> newFiles = new List<DownloadedFile>();

            #endregion End

            #region Start_Main_Download_Process_And_Maintain_Logs!

            do
            {
                try
                {
                    #region CreateDirectories_AccordingTOPracticeCode_IF_Not_Created_Already!
                    CreateDirectories(practiceCode);
                    #endregion End

                    #region SFTP_Connectivity!
                    using (SftpClient client = new SftpClient(host, port, username, password))
                    {
                        #region After_Connectivity_Get_wholeFiles_from_Directory!
                        client.Connect();
                        var files = client.ListDirectory(path);
                        #endregion End

                        // To get latest downloaded ERA from previous downloaded files table
                        var files2 = previousDownloadedFiles.Where(r => r.PracticeCode == practiceCode).OrderByDescending(r => r.DownloadedAt).FirstOrDefault();


                        List<SftpFile> filteredFiles = new List<SftpFile>();
                        if (files2 != null)
                        {
                            
                            files2.DownloadedAt = TimeZoneInfo.ConvertTimeToUtc(files2.DownloadedAt.Date);

                            foreach (var file in files)
                            {
                                file.LastWriteTimeUtc= TimeZoneInfo.ConvertTimeToUtc(file.LastWriteTimeUtc.Date);
                                if (file.LastWriteTimeUtc >= files2.DownloadedAt)
                                {
                                    //Add filtered fies from all files.
                                    filteredFiles.Add(file);
                                }
                            }
                        }
                        else
                        {
                            filteredFiles = (List<SftpFile>)files;                           
                        }

                        #region One_by_One_File_Check_From_PrevoiusDownloadedFiles_And_Download_New_Files!
                        foreach (var file in filteredFiles)
                        {
                            if (!file.IsDirectory && !file.IsSymbolicLink && !isAlreadyDownloaded(previousDownloadedFiles, file, practiceCode))
                            {
                                downloadResult = await DownloadFile1(writer, client, file, HostingEnvironment.MapPath($"~/{ConfigurationManager.AppSettings["ERADownloadDestination"]}/{practiceCode}/Inbound"), FinalREsult.message);
                                newFiles.Add(new DownloadedFile()
                                {
                                    Name = file.Name,
                                    Length = file.Length,
                                    DownloadedAt = DateTime.Now,
                                    DownloadedBy = ERAUserName,
                                    PracticeCode = practiceCode,
                                });
                            }
                        }
                        #endregion End Download

                        if (newFiles.Count() > 0)
                        {
                            #region Update_Stats_And_Maintain_Logs!

                            using (var eraDbModel = new DemoERADBModel())
                            {
                                #region insertion_Of_DownloadedFiles!
                                eraDbModel.DownloadedFiles.AddRange(newFiles);
                                await eraDbModel.SaveChangesAsync();
                                #endregion End

                                #region GetFirstDownloadedFile!&&!Set!asWell!
                                DownloadedFile firstFile = newFiles.FirstOrDefault();
                                if (firstFile != null)
                                {

                                    //FinalREsult.FirstFileDownloadServerTime = firstFile.DownloadedAt;
                                    FinalREsult.FirstDownloadedFileName = firstFile.Name;

                                }
                                #endregion End!

                                #region GetLastDownloadedFile!&&!Set!asWell!
                                if (newFiles.Count() >= 2)
                                {
                                    DownloadedFile lastFile = newFiles.LastOrDefault();
                                    if (lastFile != null)
                                    {

                                        // FinalREsult.LastFileDownloadServerTime = lastFile.DownloadedAt;
                                        FinalREsult.LastDownloadedFileName = lastFile.Name;

                                    }
                                }
                                #endregion End!

                                #region fileEqualToOne!
                                if (newFiles.Count() == 1)
                                {
                                    //FinalREsult.LastFileDownloadServerTime = firstFile.DownloadedAt;
                                    FinalREsult.LastDownloadedFileName = firstFile.Name;
                                }
                                #endregion End!

                                #region Update_DownloadedFiles_Stats_And_Maintain_logs!
                                var entity = eraDbModel.USER_ERA_REQUESTS.FirstOrDefault(e => e.Id == UserRequest.Id);
                                if (entity != null)
                                {
                                    entity.DOWNLOADED_FILE_COUNT = newFiles.Count();
                                    entity.STATUS = "Pending";
                                    entity.FTP_EXCEPTION = downloadResult;
                                    #region logs_Maintain!
                                    FinalREsult.message += $"\r\nNew Files:\t{newFiles.Count()}\r\nDownload Status:\t{entity.STATUS}\r\nFTP_EXCEPTION:\t{entity.FTP_EXCEPTION}";
                                    writer.WriteLine("New Files:\t" + newFiles.Count());
                                    writer.WriteLine("Download Status:\t" + entity.STATUS);
                                    writer.WriteLine("FTP_EXCEPTION:\t" + entity.FTP_EXCEPTION);
                                    #endregion End!

                                }
                                await eraDbModel.SaveChangesAsync();

                                #region GetCurrentStats!
                                var stats = await eraDbModel.USER_ERA_REQUESTS.Where(e => e.Id == UserRequest.Id).FirstOrDefaultAsync();
                                res.Response = stats;
                                FinalREsult.RM.Response = res.Response;
                                #endregion EndStats


                                #endregion End


                            }
                            #endregion End_Stats
                        }
                        else
                        {
                            using (var eraDbModel = new DemoERADBModel())
                            {
                                #region Update_Stats_IF_NO_new_filesFound!
                                var entity = eraDbModel.USER_ERA_REQUESTS.FirstOrDefault(e => e.Id == UserRequest.Id);
                                if (entity != null)
                                {
                                    entity.DOWNLOADED_FILE_COUNT = newFiles.Count();
                                    entity.STATUS = "Parsed";
                                    entity.FTP_EXCEPTION = "No New Files Available";
                                    #region Maintain_Stats_logs_if_no_file_Found_already_Parsed
                                    FinalREsult.message += $"\r\nNew Files:\t{newFiles.Count()}\r\nDownload Status:\t{entity.STATUS}\r\nFTP_EXCEPTION:\t{entity.FTP_EXCEPTION}";
                                    writer.WriteLine("New Files:\t" + newFiles.Count());
                                    writer.WriteLine("Download Status:\t" + entity.STATUS);
                                    writer.WriteLine("FTP_EXCEPTION:\t" + entity.FTP_EXCEPTION);
                                    #endregion End
                                }
                                await eraDbModel.SaveChangesAsync();
                                #endregion End Stats

                                #region GetCurrentStats!
                                var stats = await eraDbModel.USER_ERA_REQUESTS.Where(e => e.Id == UserRequest.Id).FirstOrDefaultAsync();
                                res.Response = stats;
                                FinalREsult.RM.Response = res.Response;
                                #endregion End
                            }

                        }
                    }
                    #endregion Close_SFTP_Connectivity

                    #region SetStatus_If_NO_Exception_Found!
                    res.Status = "Successfull";
                    FinalREsult.RM.Status = res.Status;
                    #endregion End

                    #region SuccessfullyDownloaded_AllFiles!
                    success = true;
                    #endregion End

                }
                catch (Exception ex)
                {
                    #region Set_Increment_the_Retry_Count!

                    retryCount++;

                    #endregion ENd!

                    #region Check_if_the_Maximum_Retry_Count_has_been_Reached_AND_ALSO_SMTP!
                    if (retryCount >= maxRetryCount)
                    {
                        using (var eraDbModel = new DemoERADBModel())
                        {

                            #region update_Stats_If_Exception_Found_during_Downloading_Process_And_Maintain_Logs!
                            var entity = eraDbModel.USER_ERA_REQUESTS.FirstOrDefault(e => e.Id == UserRequest.Id);
                            if (entity != null)
                            {
                                entity.DOWNLOADED_FILE_COUNT = newFiles.Count();
                                entity.STATUS = "Exception_Found!";
                                entity.FTP_EXCEPTION = ex.Message;

                                #region Create_Logs_If_Exception_Found_during_Downloading_Process!
                                FinalREsult.message += $"{logs}\r\nNew Files:\t{newFiles.Count()}\r\nDownload Status:\t{entity.STATUS}\r\nFTP_EXCEPTION at Stack Trace:\t{ex.StackTrace}\nException Message:\t{ex.Message}\nAt Time/Date:{DateTime.Now}\nDownload Exception:\t{downloadResult}";

                                writer.WriteLine("New Files:\t" + newFiles.Count());
                                writer.WriteLine("Download Status:\t" + entity.STATUS);
                                writer.WriteLine("FTP_EXCEPTION at:\t" + ex.StackTrace + "\nException Message:\t" + ex.Message + "\nDownloadException:" + downloadResult);

                                #endregion EndLogs

                            }
                            await eraDbModel.SaveChangesAsync();
                            #endregion End_Stats

                            #region StartMigrationOfStats_If_Exception_Found_during_Downloading_Process!
                            string connection = ConfigurationManager.ConnectionStrings["DemoERADBModel"].ConnectionString;

                            using (SqlConnection con = new SqlConnection(connection))
                            {

                                using (SqlCommand cmd = new SqlCommand("SP_MigrateStats", con))
                                {
                                    cmd.CommandType = CommandType.StoredProcedure;
                                    con.Open();
                                    await cmd.ExecuteNonQueryAsync();

                                }

                            }
                            #endregion EndMigration

                            #region GetCurrentStats!
                            var stats = await eraDbModel.USER_ERA_REQUESTS.FirstOrDefaultAsync(e => e.Id == UserRequest.Id);
                            res.Response = stats;
                            FinalREsult.RM.Response = res.Response;
                            #endregion End

                        }
                        //SMPT is for now is using
                        #region setting SMTP client for email!
                        // SMTP server settings
                        string smtpHost = ConfigurationManager.AppSettings["smtp"];
                        int smtpPort = Convert.ToInt32(ConfigurationManager.AppSettings["portnumber"]);
                        string smtpUsername = ConfigurationManager.AppSettings["username"];
                        string smtpPassword = ConfigurationManager.AppSettings["password"];

                        // Email recipient
                        string emailRecipient = "sfdfsd@nobilityrcm.com";
                        //In CC
                        string TL = "sdfsdf@nobilityrcm.com";
                        string AM = "sdfsdfsd@nobilityrcm.com";
                        // Email subject and body
                        string subject = "Manual ERA Request Stats – " + DateTime.Now;
                        string statsHead = "Manual ERA Request Stats".ToUpper();
                        string disclaimer = "Disclaimer:".ToUpper();
                        string body = $"\r\n{statsHead}\r\nUser ID: {FinalREsult.RM.Response.UserID}\r\nRequest Date/Time:{FinalREsult.RM.Response.ENTRY_DATE}\r\n For Practice:{FinalREsult.RM.Response.ENTRY_DATE}\r\n{FinalREsult.message}\r\n\r\n\r{disclaimer}\r\nPlease do not reply to this email as it is an automated notification. If you require further assistance, please feel free to reach out to our support team or EDI Team.\r\nThank you for your understanding and cooperation.\r\n\r\nBest regards,\nNOBILITY MBS\nNotification Service\r\n";

                        using (SmtpClient client1 = new SmtpClient(smtpHost, smtpPort))
                        {
                            client1.EnableSsl = true;
                            client1.Credentials = new NetworkCredential(smtpUsername, smtpPassword);

                            MailMessage mailMessage = new MailMessage();
                            mailMessage.From = new MailAddress(smtpUsername);
                            mailMessage.To.Add(emailRecipient);
                            mailMessage.CC.Add(TL);
                            mailMessage.CC.Add(AM);
                            mailMessage.Subject = subject;
                            mailMessage.Body = body;

                            try
                            {
                                // Send the email
                                await client1.SendMailAsync(mailMessage);
                            }
                            catch (SmtpException e)
                            {
                                // Handle any exceptions that occur during email sending
                                // Log or perform any necessary error handling
                                FinalREsult.message += $"Failed to send email: {e.Message}";
                            }
                        }
                        #endregion SMTP client setup done

                        #region SetStatus_If_Exception_Found_during_Downloading_Process!
                        res.Status = "Exception Found UnSuccessfull";
                        FinalREsult.RM.Status = res.Status;
                        #endregion End!

                        #region ReturnResult_If_Exception_Found_during_Downloading_Process!
                        return FinalREsult;
                        #endregion End!
                    }
                    #endregion End!

                    #region Wait_For_The_Specified_Interval_Before_Retrying!
                    Thread.Sleep(retryInterval);
                    #endregion
                }

            } while (!success && retryCount < maxRetryCount);

            #endregion End_Download!

            #region ReturnResult
            return FinalREsult;
            #endregion End
        }

        public async Task<ReturnERAResult> Era_User_Request(ERA_Request ERA)
        {
            #region CreateDirectoriesAndLogFileName
            ReturnERAResult FinalResult = new ReturnERAResult();
            FinalResult.RM = new ResponseModel();
            FinalResult.message = "";
            var result = new ResponseModel();
            string Logpath = "";
            CreateDirectories(ERA.PracticeCde);
            string connection = ConfigurationManager.ConnectionStrings["DemoERADBModel"].ConnectionString;
            Logpath += HostingEnvironment.MapPath($"~/{ConfigurationManager.AppSettings["ERADownloadDestination"]}");
            Logpath += "\\" + ERA.PracticeCde + "\\LogFiles";
            string lg_Path = Logpath;
            DateTime date = DateTime.Now; // or any other DateTime object
            DateTime currentDate = DateTime.Today;
            var currentTime = DateTime.Now;
            string formattedDateTime = date.ToString("dd_MM_yyyy_h_mm_ss_tt", System.Globalization.CultureInfo.InvariantCulture);
            Logpath += "\\LogFileOf" + ERA.PracticeCde + "_" + formattedDateTime + ".txt";
            string[] LogpathFilename = Logpath.Split('\\');
            string LogFilenam = LogpathFilename[LogpathFilename.Length - 1];
            List<User_era_request> newRequests = new List<User_era_request>();
            User_era_request SamePersonmultipleRequests = new User_era_request();
            User_era_request differentPersonMultipleRequests = new User_era_request();

            #endregion

            #region Get_UserERARequest
            //if (SamePersonSameRequests == null && DifferentPersonSameRequests == null)
            //{
            newRequests.Add(new User_era_request()
            {
                USER_ID = ERA.UserID,
                USER_NAME = ERA.UserName,
                ENTRY_DATE = DateTime.Now,
                PracticeCode = ERA.PracticeCde,
                LogFile_Name = LogFilenam,
                LogFile_Path = lg_Path,
                DOWNLOADED_FILE_COUNT = 0,
                STATUS = "In_process",
                FTP_EXCEPTION = "Your Request is in Process",
                T_Duplicate_Count = 0,
                Failed_Count = 0,
                Processed_File_Count = 0
            });
            #endregion

            #region Set_UserERARequest!

            if (newRequests.Count() > 0)
            {
                #region Main Process!
                using (var eraDbModel = new DemoERADBModel())
                {
                    try
                    {
                        #region Set_IncreaseTimeOut!
                        eraDbModel.Database.Initialize(false); // Ensure the database is initialized
                        var objectContext = ((IObjectContextAdapter)eraDbModel).ObjectContext;
                        objectContext.CommandTimeout = 60;
                        #endregion End!

                        #region Insert_User_ERA_Requests!

                        eraDbModel.USER_ERA_REQUESTS.AddRange(newRequests);
                        await eraDbModel.SaveChangesAsync();

                        #endregion End!

                        #region Get_Current_Request_which_is_Inserted!

                        var user = await eraDbModel.USER_ERA_REQUESTS.Where(e => e.STATUS == "In_process" && e.USER_ID == ERA.UserID && e.PracticeCode == ERA.PracticeCde && e.USER_NAME == ERA.UserName).OrderBy(e => e.ENTRY_DATE).FirstOrDefaultAsync();

                        #endregion  End!

                        #region Wait_For_One_Minute!

                        //Thread.Sleep(60000);

                        #endregion End!

                        #region Get_List_Of_requests_which_are_in_process_And_with_Same_Practice_as_requested_user!

                        var RequestOnProcessing = await eraDbModel.USER_ERA_REQUESTS.Where(e => e.Id != user.Id && e.STATUS == "In_process" && e.PracticeCode == ERA.PracticeCde).OrderByDescending(e => e.ENTRY_DATE).ToListAsync();

                        #endregion End!

                        #region Commented_Code_Used_with_Nolock_Method!
                        //string query = "SELECT TOP 1 * FROM User_era_request WITH (NOLOCK) WHERE USER_ID = {0} AND PracticeCode = {1}";
                        //var user = eraDbModel.Database.SqlQuery<User_era_request>(query, ERA.UserID, ERA.PracticeCde).FirstOrDefault();
                        #endregion End!

                        #region Check_Requested_Practice_According_to_User_request!
                        if (RequestOnProcessing.Count > 0)
                        {
                            foreach (var lastrequest in RequestOnProcessing)
                            {
                                if (lastrequest.USER_ID == user.USER_ID && lastrequest.USER_NAME == user.USER_NAME && lastrequest.PracticeCode == user.PracticeCode)
                                {
                                    #region Set_Status!
                                    result.Status = "Successfull! Already Same Person Requested For Same Practice Code";
                                    #endregion End!

                                    #region UpdateStats_When_Same_Request_Already_In_Process!
                                    var entity = await eraDbModel.USER_ERA_REQUESTS.FirstOrDefaultAsync(e => e.Id == user.Id);
                                    if (entity != null)
                                    {
                                        entity.STATUS = "Already_In_process";
                                        entity.FTP_EXCEPTION = "Your Request is Already in Process for Same Practice";
                                        entity.DOWNLOADED_FILE_COUNT = 0;
                                        entity.Failed_Count = 0;
                                        entity.Processed_File_Count = 0;
                                        entity.T_Duplicate_Count = 0;

                                    }
                                    await eraDbModel.SaveChangesAsync();
                                    var F_res = await eraDbModel.USER_ERA_REQUESTS.FirstOrDefaultAsync(e => e.Id == user.Id);
                                    #endregion End!

                                    #region Start_STATS_Migration_AND_Maintain_Logs
                                    int c = 0;
                                    var retryPolicy = Polly.Policy.Handle<SqlException>().WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
                                    SqlConnection con = null;
                                    try
                                    {
                                        con = new SqlConnection(connection);
                                        using (SqlCommand cmd = new SqlCommand("SP_MigrateStats_same_prac", con))
                                        {
                                            cmd.CommandType = CommandType.StoredProcedure;
                                            cmd.CommandTimeout = 600; // Set timeout to 600 seconds
                                            cmd.Parameters.AddWithValue("@id", user.Id);

                                            await retryPolicy.ExecuteAsync(async () =>
                                            {
                                                c++;
                                                FinalResult.message += $"In Case of Same User again Requested for Same Practice Code Stats Migration Start For {c} Time";
                                                if (con != null && con.State != ConnectionState.Open)
                                                {
                                                    await con.OpenAsync(); // Open the connection
                                                    FinalResult.message += $"\r\nSql Connection Open Successfully!";
                                                }
                                                //await con.OpenAsync();
                                                await cmd.ExecuteNonQueryAsync();
                                                if (con != null && con.State != ConnectionState.Closed)
                                                {
                                                    con.Close(); // Close the connection
                                                    FinalResult.message += $"\r\nSql Connection Close Successfully!";
                                                }
                                                con?.Dispose(); // Dispose the connection
                                                //con.Close();
                                            });
                                        }


                                        //var res=cmd.ExecuteNonQuery();

                                    }
                                    catch (Exception ex)
                                    {
                                        result.Status = "Successfull! Already Same Person Requested For Same Practice Code";
                                        result.Response = F_res;
                                        FinalResult.RM = result;
                                        FinalResult.message += $"During In Case of Same User again Requested for Same Practice Code Stats Migration Exception Found at Time:{DateTime.Now}\n\rException:{ex.Message}\r\nStack Trace:{ex.StackTrace}";
                                        #region ReturnResult!
                                        return FinalResult;
                                        #endregion End!
                                    }


                                    #endregion End_STATS_Migration

                                    #region Set_Response!
                                    result.Response = F_res;
                                    FinalResult.RM = result;
                                    FinalResult.message += "Successfull! Already Same Person Requested For Same Practice Code";
                                    #endregion End!

                                    #region ReturnResult!
                                    return FinalResult;
                                    #endregion End!
                                }
                                else
                                 if (lastrequest.USER_ID != user.USER_ID && lastrequest.USER_NAME != user.USER_NAME && lastrequest.PracticeCode == user.PracticeCode)
                                {
                                    #region Set_status!
                                    result.Status = "Successfull! Already Different Person Requested For Same Practice Code";
                                    #endregion End!

                                    #region UpdateStats_When_Different_Request_Already_In_Process!
                                    var entity = await eraDbModel.USER_ERA_REQUESTS.FirstOrDefaultAsync(e => e.Id == user.Id);
                                    if (entity != null)
                                    {
                                        entity.STATUS = "Already_In_process";
                                        entity.FTP_EXCEPTION = "Can not process at current time. ERA already requested for Practice [" + lastrequest.PracticeCode + "] by user [" + lastrequest.USER_NAME + "].";
                                        entity.DOWNLOADED_FILE_COUNT = 0;
                                        entity.Failed_Count = 0;
                                        entity.Processed_File_Count = 0;
                                        entity.T_Duplicate_Count = 0;

                                    }
                                    await eraDbModel.SaveChangesAsync();
                                    var F_res = await eraDbModel.USER_ERA_REQUESTS.FirstOrDefaultAsync(e => e.Id == user.Id);
                                    #endregion End!

                                    #region Start_STATS_Migration_AND_Maintain_Logs
                                    int c = 0;
                                    var retryPolicy = Polly.Policy.Handle<SqlException>().WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
                                    SqlConnection con = null;
                                    try
                                    {
                                        con = new SqlConnection(connection);
                                        using (SqlCommand cmd = new SqlCommand("SP_MigrateStats_same_prac", con))
                                        {
                                            cmd.CommandType = CommandType.StoredProcedure;
                                            cmd.CommandTimeout = 120; // Set timeout to 120 seconds
                                            cmd.Parameters.AddWithValue("@id", user.Id);
                                            await retryPolicy.ExecuteAsync(async () =>
                                            {
                                                c++;
                                                FinalResult.message += $"In Case of Different User again Requested for Same Practice Code Stats Migration Start For {c} Time";
                                                if (con != null && con.State != ConnectionState.Open)
                                                {
                                                    await con.OpenAsync(); // Open the connection
                                                    FinalResult.message += $"\r\nSql Connection Open Successfully!";
                                                }

                                                await cmd.ExecuteNonQueryAsync();
                                                if (con != null && con.State != ConnectionState.Closed)
                                                {
                                                    con.Close(); // Close the connection
                                                    FinalResult.message += $"\r\nSql Connection Close Successfully!";
                                                }
                                                con?.Dispose(); // Dispose the connection
                                                //con.Close();
                                            });
                                        }


                                        //var res=cmd.ExecuteNonQuery();
                                    }
                                    catch (Exception ex)
                                    {
                                        result.Status = "Successfull! Already Different Person Requested For Same Practice Code";
                                        result.Response = F_res;
                                        FinalResult.RM = result;
                                        FinalResult.message += $"During In Case of Different User again Requested for Same Practice Code Stats Migration Exception Found at Time:{DateTime.Now}\n\rException:{ex.Message}\r\nStack Trace:{ex.StackTrace}";
                                        #region ReturnResult!
                                        return FinalResult;
                                        #endregion End!
                                    }


                                    #endregion End_STATS_Migration

                                    #region Set_Response!
                                    result.Response = F_res;
                                    FinalResult.RM = result;
                                    FinalResult.message += "Successfull! Already Different Person Requested For Same Practice Code";
                                    #endregion End!

                                    #region ReturnResult!
                                    return FinalResult;
                                    #endregion End!
                                }
                            }
                        }
                        #endregion End!

                        #region If_There_is_NO_Requested_Practice_is_in_Processing!
                        else
                        {
                            result.Status = "Successfull";
                            result.Response = user;
                            FinalResult.RM = result;
                            FinalResult.message += "User ERA Request Inserted Successfull";
                        }
                        #endregion End!
                    }
                    catch (Exception ex)
                    {
                        FinalResult.message += $"Exception Found:{ex.Message}\nAt Stack Trace:{ex.StackTrace}\nAt Time/Date:{DateTime.Now}";

                        result.Status = "UnSuccessfull exception found!";
                        result.Response = ex.Message;
                        FinalResult.RM = result;
                        //SMPT is for now is using
                        #region setting SMTP client for email!
                        // SMTP server settings
                        string smtpHost = ConfigurationManager.AppSettings["smtp"];
                        int smtpPort = Convert.ToInt32(ConfigurationManager.AppSettings["portnumber"]);
                        string smtpUsername = ConfigurationManager.AppSettings["username"];
                        string smtpPassword = ConfigurationManager.AppSettings["password"];

                        // Email recipient
                        string emailRecipient = "dgbdfb@nobilityrcm.com";
                        //In CC
                        string TL = "dfhdd@nobilityrcm.com";
                        string AM = "ytrtyrt@nobilityrcm.com";
                        // Email subject and body
                        string subject = "Manual ERA Request Stats – " + DateTime.Now;
                        string statsHead = "Manual ERA Request Stats".ToUpper();
                        string disclaimer = "Disclaimer:".ToUpper();
                        string body = $"\r\n{statsHead}\r\nUser ID: {ERA.UserID}\r\nRequest Date/Time:{DateTime.Now}\r\n For Practice:{ERA.PracticeCde}\r\n{FinalResult.message}\r\n\r\n\r{disclaimer}\r\nPlease do not reply to this email as it is an automated notification. If you require further assistance, please feel free to reach out to our support team or EDI Team.\r\nThank you for your understanding and cooperation.\r\n\r\nBest regards,\nNOBILITY MBS\nNotification Service\r\n";

                        using (SmtpClient client1 = new SmtpClient(smtpHost, smtpPort))
                        {
                            client1.EnableSsl = true;
                            client1.Credentials = new NetworkCredential(smtpUsername, smtpPassword);

                            MailMessage mailMessage = new MailMessage();
                            mailMessage.From = new MailAddress(smtpUsername);
                            mailMessage.To.Add(emailRecipient);
                            mailMessage.CC.Add(TL);
                            mailMessage.CC.Add(AM);
                            mailMessage.Subject = subject;
                            mailMessage.Body = body;

                            try
                            {
                                // Send the email
                                await client1.SendMailAsync(mailMessage);
                            }
                            catch (SmtpException e)
                            {
                                // Handle any exceptions that occur during email sending
                                // Log or perform any necessary error handling
                                FinalResult.message += $"Failed to send email: {e.Message}";
                            }
                        }
                        #endregion SMTP client setup done
                        return FinalResult;

                    }

                }
                #endregion End Main Process!
            }

            #endregion

            #region ReturnResult!
            return FinalResult;
            #endregion End!

        }



        public async Task<ReturnERAResult> ERA_Download_and_Dump_Process(string username, string logs, ERA_Request ERA)
        {
            #region Declare_VariablesAndSetConnectionStringForStaggingDB!
            var ERAResult = new ReturnERAResult();
            ERAResult.message = logs;
            User_era_request EndResult;
            ERAResult.RM = new ResponseModel();
            var result = new ReturnERAResult();
            var ParseResult = new ReturnERAResult();
            DateTime dateTime = DateTime.Now; // Replace with your DateTime value
            string dayName = dateTime.DayOfWeek.ToString();
            string monthName = dateTime.ToString("MMMM");
            int year = dateTime.Year;
            var Finalresult = new ResponseModel();
            string connection = ConfigurationManager.ConnectionStrings["DemoERADBModel"].ConnectionString;
            string Logpath = "";
            //For SMTP Server:
            // SMTP server settings
            string smtpHost = ConfigurationManager.AppSettings["smtp"];
            int smtpPort = Convert.ToInt32(ConfigurationManager.AppSettings["portnumber"]);
            string smtpUsername = ConfigurationManager.AppSettings["username"];
            string smtpPassword = ConfigurationManager.AppSettings["password"];
            string body = "";
            // Email recipient
            string emailRecipient = "dfgdfgd @nobilityrcm.com";
            //In CC
            string TL = "dfgdfg@nobilityrcm.com";
            string AM = "dgdfgdfgq@nobilityrcm.com";
            // Email subject and body
            string subject = "Manual ERA Request Stats – " + DateTime.Now;
            string statsHead = "Manual ERA Request Stats".ToUpper();
            string disclaimer = "Disclaimer:".ToUpper();

            #endregion End!

            #region Main_Process_Start!
            using (var eraDbModel = new DemoERADBModel())
            {
                #region Get_Current_Request!

                var Requests = await eraDbModel.USER_ERA_REQUESTS.Where(e => e.STATUS == "In_process" && e.USER_ID == ERA.UserID && e.PracticeCode == ERA.PracticeCde && e.USER_NAME == ERA.UserName).OrderByDescending(e => e.ENTRY_DATE).FirstOrDefaultAsync();

                #endregion End!

                #region If_you_want_to_try_with_no_lock_method!(Commented Code!)
                //string query = "SELECT TOP 1 * FROM User_era_request WITH (NOLOCK) WHERE STATUS={0} and USER_ID = {1} AND PracticeCode = {2} and USER_NAME={3} order by Entry_Date Desc";
                //var Requests = eraDbModel.Database.SqlQuery<User_era_request>(query, "In_processing", ERA.UserID, ERA.PracticeCde, ERA.UserName).FirstOrDefault();
                #endregion End!

                #region Process_On_Current_Request!

                if (Requests != null)
                {

                    #region LogPath_Set
                    Logpath = "";
                    Logpath += HostingEnvironment.MapPath($"~/{ConfigurationManager.AppSettings["ERADownloadDestination"]}");
                    Logpath += $"\\{Requests.PracticeCode}\\LogFiles";
                    Logpath += $"\\{Requests.LogFile_Name}";
                    #endregion End!

                    #region LogCreationFromHere_And_Further_Process_Start_from_Here!Click me!
                    using (StreamWriter writer = new StreamWriter(Logpath, true))
                    {
                        #region Starting_Logs!
                        writer.WriteLine("|" + dayName + "|" + monthName + "|===============****STATS****===============|ERA Import Service|");
                        ERAResult.message += $"\r\n|{dayName}|{monthName}|===============****STATS****===============|ERA Import Service|";
                        writer.WriteLine($"User Request Recieved Process Started at this time:{DateTime.Now}");
                        ERAResult.message += $"User Request Recieved Process Started at this time:{DateTime.Now}";
                        #endregion End!

                        #region Get_FTP_According_requested_Practice_Code!

                        var ftps = await _Practice.GetFTPEnabledPractices(Requests.PracticeCode);

                        #endregion End!

                        #region InitialiizeDownloadFileList!

                        List<DownloadedFile> downloadedFiles = new List<DownloadedFile>();

                        #endregion End!

                        #region MainProcessStart_After Getting Sftp!

                        #region IF_Greater_Then_One_Practice_Code_Credientials_Get!
                        if (ftps.Count() > 0)
                        {
                            #region GetPreviousDownloads
                            downloadedFiles = await GetPreviouseDownloadedFiles(ftps.Select(ftp => ftp.PracticeCode).ToList(), ERAResult.message);
                            #endregion End


                            foreach (var ftp in ftps)
                            {
                                #region Set_logs_of_User_request_with_time
                                ERAResult.message += $"\r\nUsername:\t{Requests.USER_NAME}\r\nPractice_Code:\t{Requests.PracticeCode}";
                                writer.WriteLine("Username:\t" + Requests.USER_NAME);
                                writer.WriteLine("Practice_Code:\t" + Requests.PracticeCode);
                                writer.WriteLine($"Download File Verification Started at:{DateTime.Now}");
                                ERAResult.message += $"\r\nDownload File Verification Started at:{DateTime.Now}";
                                #endregion End Logs

                                #region DownloadFilesFromSFTP_Server_Maintain_Logs
                                result = await Download1(
                                    ftp.PracticeCode,
                                    ftp.Username,
                                    ftp.Password,
                                    writer,
                                    username,
                                    ftp.Host,
                                    ftp.Port,
                                    ConfigurationManager.AppSettings["ERADownloadSource"],
                                    Requests,
                                    downloadedFiles.Where(df => df.PracticeCode == ftp.PracticeCode).ToList(), ERAResult.message);
                                ERAResult.message += $"\r\n{result.message}\r\nDownload Procedure End at:{DateTime.Now}";
                                writer.WriteLine("Download Procedure End at:" + DateTime.Now);
                                #endregion

                                #region MigrationDataToLiveDB_Here___AsWellAs___MigrationOfStatsToLive
                                if (result.RM.Status == "Successfull" && result.RM.Response.DOWNLOADED_FILE_COUNT > 0)
                                {

                                    SqlConnection con = null;
                                    try
                                    {
                                        #region ParseAndDumpProcessingStartHere
                                        ParseResult = await ParseAndDump(ftp.PracticeCode, username, writer, ERAResult.message);
                                        writer.WriteLine("Total Failed Files:\t" + ParseResult.RM.Response);
                                        ERAResult.message += $"\r\nTotal Failed Files:\t{ParseResult.RM.Response}";
                                        #endregion

                                        #region UPdate_Stats_Status
                                        var entity = await eraDbModel.USER_ERA_REQUESTS.FirstOrDefaultAsync(e => e.Id == Requests.Id);
                                        if (entity != null)
                                        {
                                            entity.STATUS = "Parsed";
                                            entity.FTP_EXCEPTION = "Successful";
                                            entity.DOWNLOADED_FILE_COUNT = result.RM.Response.DOWNLOADED_FILE_COUNT;
                                            entity.Failed_Count = ParseResult.RM.Response;

                                            if (entity.DOWNLOADED_FILE_COUNT >= entity.Failed_Count)
                                            {
                                                entity.Processed_File_Count = entity.DOWNLOADED_FILE_COUNT - entity.Failed_Count;
                                            }
                                            else
                                            {
                                                entity.Processed_File_Count = 0;
                                            }

                                            entity.T_Duplicate_Count = 0;
                                        }
                                        await eraDbModel.SaveChangesAsync();


                                        //var entity = await eraDbModel.USER_ERA_REQUESTS.FirstOrDefaultAsync(e => e.Id == Requests.Id);
                                        //if (entity != null)
                                        //{
                                        //    entity.STATUS = "Parsed";
                                        //    entity.FTP_EXCEPTION = "Successful";
                                        //    entity.DOWNLOADED_FILE_COUNT = result.RM.Response.DOWNLOADED_FILE_COUNT;
                                        //    entity.Failed_Count = ParseResult.RM.Response;
                                        //    entity.Processed_File_Count = entity.DOWNLOADED_FILE_COUNT - entity.Failed_Count;
                                        //    entity.T_Duplicate_Count = 0;

                                        //}
                                        //await eraDbModel.SaveChangesAsync();
                                        #endregion

                                        #region Start_LiveMigration_AND_Maintain_STATS
                                        int c = 0;
                                        var retryPolicy = Polly.Policy.Handle<SqlException>().WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
                                        con = new SqlConnection(connection);
                                        using (SqlCommand cmd = new SqlCommand("SP_MigrateDataToLiveServer", con))
                                        {
                                            cmd.CommandType = CommandType.StoredProcedure;
                                            cmd.CommandTimeout = 600; // Set timeout to 600 seconds
                                            //add parameter
                                            cmd.Parameters.AddWithValue("@id", Requests.Id);
                                            cmd.Parameters.AddWithValue("@RequestedPersonUsername", Requests.USER_NAME);
                                            cmd.Parameters.AddWithValue("@FTPUserName", ftp.Username);
                                            cmd.Parameters.AddWithValue("@FirstFileName", result.FirstDownloadedFileName);
                                            cmd.Parameters.AddWithValue("@LastFileName", result.LastDownloadedFileName);
                                            await retryPolicy.ExecuteAsync(async () =>
                                            {
                                                c++;
                                                if (con != null && con.State != ConnectionState.Open)
                                                {
                                                    await con.OpenAsync();
                                                    ERAResult.message += $"\r\nSql Connection Open Successfully!";
                                                    writer.WriteLine($"Sql Connection Open Successfully!");
                                                }
                                                writer.WriteLine($"Live Migration Store Procedure start For {c} Time.");
                                                ERAResult.message += $"\r\nFull Downloaded Files Migration Store Procedure start For {c} Time.";
                                                ERAResult.message += $"\r\nFull Downloaded Files Migration Store Procedure start at:{DateTime.Now}";
                                                writer.WriteLine($"Full Downloaded Files Migration Store Procedure start at:{DateTime.Now}");
                                                await cmd.ExecuteNonQueryAsync();
                                                if (con != null && con.State != ConnectionState.Closed)
                                                {
                                                    con.Close(); // Close the connection
                                                    ERAResult.message += $"\r\nSql Connection Close Successfully!";
                                                    writer.WriteLine($"Sql Connection Close Successfully!");
                                                }
                                                con?.Dispose(); // Dispose the connection

                                            });

                                            ERAResult.message += $"\r\nFull Downloaded Files Migration Store Procedure End at:{DateTime.Now}";
                                            writer.WriteLine($"Full Downloaded Files Migration Store Procedure End at:{DateTime.Now}");
                                        }


                                        //var res=cmd.ExecuteNonQuery();


                                        #endregion End_LiveMigration

                                        #region Get_StatsRequest_AND_Maintain_STATS


                                        EndResult = await eraDbModel.USER_ERA_REQUESTS.Where(e => e.Id == Requests.Id).FirstOrDefaultAsync();
                                        Finalresult.Response = EndResult;
                                        Finalresult.Status = "Successfull";
                                        ERAResult.RM = Finalresult;
                                        ERAResult.message += $"\r\nTotal Dublicate Files:\t{EndResult.T_Duplicate_Count}\r\nProcessed Files:\t{EndResult.Processed_File_Count}\r\nStatus:\t{EndResult.STATUS}";
                                        writer.WriteLine("Total Dublicate Files:\t" + EndResult.T_Duplicate_Count);
                                        writer.WriteLine("Processed Files:\t" + EndResult.Processed_File_Count);
                                        writer.WriteLine("Status:\t" + EndResult.STATUS);

                                        #endregion End_Get_StatsRequest

                                    }
                                    catch (Exception ex)
                                    {
                                        #region SetException_AND_Maintain_STATS
                                        Finalresult.Response = result.RM.Response;
                                        Finalresult.Status = "Successfull but Exception Found During Migration Store Prcedure:" + ex.Message;
                                        writer.WriteLine($"\"Successfull but Exception Found During Live Migration Store Prcedure:\" + {ex.Message} \n at:{DateTime.Now}");
                                        ERAResult.message += $"\r\nSuccessfull but Exception Found During Live Migration Store Prcedure:\" + {ex.Message} \n at:{DateTime.Now}";
                                        #endregion

                                        //SMPT is for now is using
                                        #region setting SMTP client for email!

                                        string body1 = $"\r\n{statsHead}\r\nUsername: {username}\r\nRequest Date/Time:{DateTime.Now}\r\n{ERAResult.message}\r\n\r\n\r{disclaimer}\r\nPlease do not reply to this email as it is an automated notification. If you require further assistance, please feel free to reach out to our support team or EDI Team.\r\nThank you for your understanding and cooperation.\r\n\r\nBest regards,\nNOBILITY MBS\nNotification Service\r\n";

                                        using (SmtpClient client1 = new SmtpClient(smtpHost, smtpPort))
                                        {
                                            client1.EnableSsl = true;
                                            client1.Credentials = new NetworkCredential(smtpUsername, smtpPassword);

                                            MailMessage mailMessage = new MailMessage();
                                            mailMessage.From = new MailAddress(smtpUsername);
                                            mailMessage.To.Add(emailRecipient);
                                            mailMessage.CC.Add(TL);
                                            mailMessage.CC.Add(AM);
                                            mailMessage.Subject = subject;
                                            mailMessage.Body = body1;

                                            try
                                            {
                                                // Send the email
                                                await client1.SendMailAsync(mailMessage);
                                            }
                                            catch (SmtpException e)
                                            {
                                                // Handle any exceptions that occur during email sending
                                                // Log or perform any necessary error handling
                                                ERAResult.message += $"Failed to send email: {e.Message}";
                                            }
                                        }
                                        #endregion SMTP client setup done

                                    }

                                }
                                else
                                {
                                    SqlConnection con = null;
                                    try
                                    {

                                        #region UPdate_Stats_Status
                                        var entity = await eraDbModel.USER_ERA_REQUESTS.FirstOrDefaultAsync(e => e.Id == Requests.Id);
                                        if (entity != null)
                                        {
                                            entity.FTP_EXCEPTION = "No New Files Available";
                                            entity.STATUS = "Parsed";
                                            entity.Failed_Count = 0;
                                            entity.Processed_File_Count = 0;
                                            entity.T_Duplicate_Count = 0;

                                        }
                                        await eraDbModel.SaveChangesAsync();
                                        #endregion

                                        #region Start_STATS_Migration_AND_Maintain_Logs
                                        int c = 0;
                                        var retryPolicy = Polly.Policy.Handle<SqlException>().WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
                                        con = new SqlConnection(connection);


                                        using (SqlCommand cmd = new SqlCommand("SP_MigrateStats", con))
                                        {
                                            cmd.CommandType = CommandType.StoredProcedure;
                                            cmd.CommandTimeout = 120; // Set timeout to 120 seconds

                                            await retryPolicy.ExecuteAsync(async () =>
                                            {
                                                c++;
                                                writer.WriteLine($"Stats Migration Store Procedure start For {c} Time.");
                                                writer.WriteLine($"Stats Migration Store Procedure start at:{DateTime.Now}");
                                                ERAResult.message += $"\r\nStats Migration Store Procedure start For {c} Time.";
                                                ERAResult.message += $"\r\nStats Migration Store Procedure start at:{DateTime.Now}";

                                                if (con != null && con.State != ConnectionState.Open)
                                                {
                                                    await con.OpenAsync();
                                                    writer.WriteLine("SQL Connection Open Successfully!");
                                                    ERAResult.message += "\r\nSQL Connection Open Successfully!";
                                                }
                                                await cmd.ExecuteNonQueryAsync();
                                                if (con != null && con.State != ConnectionState.Closed)
                                                {
                                                    con.Close(); // Close the connection
                                                    writer.WriteLine("SQL Connection Close Successfully!");
                                                    ERAResult.message += "\r\nSQL Connection Close Successfully!";
                                                }
                                                con?.Dispose(); // Dispose the connection
                                                //con.Close();
                                            });
                                            ERAResult.message += $"\r\nStats Migration Store Procedure start at:{DateTime.Now}";
                                            writer.WriteLine($"Stats Migration Store Procedure start at:{DateTime.Now}");

                                        }


                                        //var res=cmd.ExecuteNonQuery();


                                        #endregion End_STATS_Migration

                                        #region Get_StatOfRequest_AND_Maintain_LOGS_If_no_new_Files_Found

                                        EndResult = await eraDbModel.USER_ERA_REQUESTS.Where(e => e.Id == Requests.Id).FirstOrDefaultAsync();
                                        Finalresult.Response = EndResult;
                                        ERAResult.RM.Response = Finalresult.Response;
                                        ERAResult.message += $"\r\nTotal Dublicate Files:\t{EndResult.T_Duplicate_Count}\r\nProcessed Files:\t{EndResult.Processed_File_Count}\r\nStatus:\t{EndResult.STATUS}";
                                        writer.WriteLine("Total Dublicate Files:\t" + EndResult.T_Duplicate_Count);
                                        writer.WriteLine("Processed Files:\t" + EndResult.Processed_File_Count);
                                        writer.WriteLine("Status:\t" + EndResult.STATUS);

                                        #endregion End_Stats

                                        #region Set_Status
                                        Finalresult.Status = "Successfull";
                                        ERAResult.RM.Status = Finalresult.Status;
                                        #endregion End

                                    }
                                    catch (Exception ex)
                                    {

                                        #region Handle_Exception_AND_Maintain_logs
                                        Finalresult.Response = result.RM.Response;
                                        ERAResult.message += $"\r\nSuccessfull but Exception Found During Migration Store Prcedure:{ex.Message}\nAt Stack Trace:{ex.StackTrace}\nAt Date/Time:{DateTime.Now}";
                                        Finalresult.Status = "Successfull but Exception Found During Migration Store Prcedure:" + ex.Message;
                                        ERAResult.RM = Finalresult;
                                        writer.WriteLine($"\"Successfull but Exception Found During Stats Migration Store Prcedure:\" + {ex.Message} \n at:{DateTime.Now}");
                                        #endregion End

                                        //SMPT is for now is using
                                        #region setting SMTP client for email!

                                        string body1 = $"\r\n{statsHead}\r\nUsername: {username}\r\nRequest Date/Time:{DateTime.Now}\r\n{ERAResult.message}\r\n\r\n\r{disclaimer}\r\nPlease do not reply to this email as it is an automated notification. If you require further assistance, please feel free to reach out to our support team or EDI Team.\r\nThank you for your understanding and cooperation.\r\n\r\nBest regards,\nNOBILITY MBS\nNotification Service\r\n";

                                        using (SmtpClient client1 = new SmtpClient(smtpHost, smtpPort))
                                        {
                                            client1.EnableSsl = true;
                                            client1.Credentials = new NetworkCredential(smtpUsername, smtpPassword);

                                            MailMessage mailMessage = new MailMessage();
                                            mailMessage.From = new MailAddress(smtpUsername);
                                            mailMessage.To.Add(emailRecipient);
                                            mailMessage.CC.Add(TL);
                                            mailMessage.CC.Add(AM);
                                            mailMessage.Subject = subject;
                                            mailMessage.Body = body1;

                                            try
                                            {
                                                // Send the email
                                                await client1.SendMailAsync(mailMessage);
                                            }
                                            catch (SmtpException e)
                                            {
                                                // Handle any exceptions that occur during email sending
                                                // Log or perform any necessary error handling
                                                ERAResult.message += $"Failed to send email: {e.Message}";
                                            }
                                        }
                                        #endregion SMTP client setup done
                                    }

                                }
                                #endregion

                            }
                        }
                        #endregion End!

                        #region IF_There_is_No_Practice_Code_Credientials_Get_Then!
                        else
                        {
                            #region Update_Stats_If_no_practiceCode_Found_AND_Maintain_LOGs!
                            writer.WriteLine("Username:\t" + Requests.USER_NAME);
                            ERAResult.message += $"\r\nUsername:\t{Requests.USER_NAME}\r\nPractice_Code:\t{Requests.PracticeCode}";
                            writer.WriteLine("Practice_Code:\t" + Requests.PracticeCode);
                            var entity = await eraDbModel.USER_ERA_REQUESTS.FirstOrDefaultAsync(e => e.Id == Requests.Id);
                            if (entity != null)
                            {
                                entity.DOWNLOADED_FILE_COUNT = 0;
                                entity.STATUS = "Failed";
                                entity.FTP_EXCEPTION = "PracticeCode Not Found";
                                #region log_if_PracticeCodeNotFound
                                ERAResult.message += $"New Files:\t{entity.DOWNLOADED_FILE_COUNT}Status:\t{entity.STATUS}FTP_EXCEPTION:\t{entity.FTP_EXCEPTION}";
                                writer.WriteLine("New Files:\t" + entity.DOWNLOADED_FILE_COUNT);
                                writer.WriteLine("Status:\t" + entity.STATUS);
                                writer.WriteLine("FTP_EXCEPTION:\t" + entity.FTP_EXCEPTION);
                                #endregion

                            }
                            await eraDbModel.SaveChangesAsync();
                            #endregion End_Update_Stats!

                            #region Get_Stats_for_UI_if_no_PRacticeCodeFound!
                            EndResult = await eraDbModel.USER_ERA_REQUESTS.FirstOrDefaultAsync(e => e.Id == Requests.Id);
                            Finalresult.Response = EndResult;
                            ERAResult.RM.Response = Finalresult.Response;
                            #endregion

                            #region Start_MigrationOFStats_IF_NO_PracticeCode_Found!
                            SqlConnection con = null;
                            try
                            {
                                #region StartMigrationOfStats_AND_Maintain_Logs
                                int c = 0;
                                var retryPolicy = Polly.Policy.Handle<SqlException>().WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
                                con = new SqlConnection(connection);

                                using (SqlCommand cmd = new SqlCommand("SP_MigrateStats", con))
                                {
                                    cmd.CommandType = CommandType.StoredProcedure;
                                    cmd.CommandTimeout = 120; // Set timeout to 120 seconds

                                    await retryPolicy.ExecuteAsync(async () =>
                                    {
                                        c++;
                                        ERAResult.message += $"\r\nStats Migration Store Procedure Start For {c} Time.";
                                        ERAResult.message += $"\r\nStats Migration Store Procedure Start at:{DateTime.Now}";
                                        writer.WriteLine($"Stats Migration Store Procedure Start For {c} Time.");
                                        writer.WriteLine($"Stats Migration Store Procedure Start at:{DateTime.Now}");

                                        if (con != null && con.State != ConnectionState.Open)
                                        {
                                            await con.OpenAsync();
                                            writer.WriteLine("SQL Connection Open Successfully!");
                                            ERAResult.message += "\r\nSQL Connection Open Successfully!";
                                        }
                                        await cmd.ExecuteNonQueryAsync();
                                        if (con != null && con.State != ConnectionState.Closed)
                                        {
                                            con.Close(); // Close the connection
                                            writer.WriteLine("SQL Connection Close Successfully!");
                                            ERAResult.message += "\r\nSQL Connection Close Successfully!";
                                        }
                                        con?.Dispose(); // Dispose the connection
                                        //con.Close();
                                    });

                                    ERAResult.message += $"Stats Migration Store Procedure End at:{DateTime.Now}";
                                    writer.WriteLine($"Stats Migration Store Procedure End at:{DateTime.Now}");

                                }


                                //var res=cmd.ExecuteNonQuery();

                                #endregion EndMigration

                            }
                            catch (Exception ex)
                            {
                                #region ExceptionHandling_And_Maintain_logs
                                Finalresult.Response = EndResult;
                                ERAResult.RM.Response = Finalresult.Response;
                                Finalresult.Status = "Successfull but Exception Found During Migration Store Prcedure:" + ex.Message + "At Stack Trace:" + ex.StackTrace + "At Time:" + DateTime.Now;
                                ERAResult.RM.Status = Finalresult.Status;
                                writer.WriteLine($"\"Successfull but Exception Found During Stats Migration Store Prcedure:\" + {ex.Message} \n at:{DateTime.Now}");
                                ERAResult.message += $"Successfull but Exception Found During Stats Migration Store Prcedure:\" + {ex.Message} \nAt Stack trace:{ex.StackTrace}\n at:{DateTime.Now}";
                                #endregion End

                                //SMPT is for now is using
                                #region setting SMTP client for email!

                                string body1 = $"\r\n{statsHead}\r\nUsername: {username}\r\nRequest Date/Time:{DateTime.Now}\r\n{ERAResult.message}\r\n\r\n\r{disclaimer}\r\nPlease do not reply to this email as it is an automated notification. If you require further assistance, please feel free to reach out to our support team or EDI Team.\r\nThank you for your understanding and cooperation.\r\n\r\nBest regards,\nNOBILITY MBS\nNotification Service\r\n";

                                using (SmtpClient client1 = new SmtpClient(smtpHost, smtpPort))
                                {
                                    client1.EnableSsl = true;
                                    client1.Credentials = new NetworkCredential(smtpUsername, smtpPassword);

                                    MailMessage mailMessage = new MailMessage();
                                    mailMessage.From = new MailAddress(smtpUsername);
                                    mailMessage.To.Add(emailRecipient);
                                    mailMessage.CC.Add(TL);
                                    mailMessage.CC.Add(AM);
                                    mailMessage.Subject = subject;
                                    mailMessage.Body = body1;

                                    try
                                    {
                                        // Send the email
                                        await client1.SendMailAsync(mailMessage);
                                    }
                                    catch (SmtpException e)
                                    {
                                        // Handle any exceptions that occur during email sending
                                        // Log or perform any necessary error handling
                                        ERAResult.message += $"Failed to send email: {e.Message}";
                                    }
                                }
                                #endregion SMTP client setup done

                            }


                            #endregion End_MigrationOFStats

                            #region Set_Status
                            Finalresult.Status = "Successfull";
                            ERAResult.RM.Status = Finalresult.Status;
                            #endregion End
                        }
                        #endregion End!

                        #endregion End!

                        #region End Log File!
                        ERAResult.message += $"\r\n|{year}|===============*******END*******===============|Date:{DateTime.Now}|";
                        writer.WriteLine("|" + year + "|===============*******END*******===============|Date:" + DateTime.Now + "|");
                        #endregion End!


                    }
                    #endregion End!

                    #region If Process Successfully End then Sending Overall Stats in Email!

                    #region Get_Mail_stats!

                    var em = await eraDbModel.USER_ERA_REQUESTS.FirstOrDefaultAsync(e => e.Id == Requests.Id);

                    #endregion End!
                    #region variable declaration
                    string requestFor = $"Request For Manual ERA Downloading for this [{em.PracticeCode}] Practice Code.";
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
            <td>" + em.USER_NAME + @"</td>
        </tr>
        <tr>
            <th>Requested For Practice Code</th>
            <td>" + em.PracticeCode + @"</td>
        </tr>
        <tr>
            <th>Requested Date/Time</th>
            <td>" + DateTime.Now + @"</td>
        </tr>
        <tr>
            <th>Total Downloaded Files</th>
            <td>" + em.DOWNLOADED_FILE_COUNT + @"</td>
        </tr>
        <tr>
            <th>Total Failed Files</th>
            <td>" + em.Failed_Count + @"</td>
        </tr>
        <tr>
            <th>Total Processed Files</th>
            <td>" + em.Processed_File_Count + @"</td>
        </tr>
        <tr>
            <th>Total Duplicate Files</th>
            <td>" + em.T_Duplicate_Count + @"</td>
        </tr>
        <tr>
            <th>FTP Exception</th>
            <td>" + em.FTP_EXCEPTION + @"</td>
        </tr>
        <tr>
            <th>Status</th>
            <td>" + em.STATUS + @"</td>
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


                    string body2 = $"{emailBody}";

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
                        mailMessage.Body = body2;
                        #region if you want to attach a file with it!
                        //// Create the attachment
                        Attachment attachment = new Attachment(Logpath);
                        // Add the attachment to the email
                        mailMessage.Attachments.Add(attachment);
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
                            ERAResult.message += "\t\t\t\t\t\t\t\t>>>-------------------------Problem Encountered During Email--------------------------------\n";
                            ERAResult.message += "\t\t\t\t\t\t\t\tDate" + DateTime.Now;
                            ERAResult.message += "\n\t\t\t\t\t\t\t\tFailed to Send Email\r\nException Found: " + ex.Message;
                            ERAResult.message += "\n";
                            ERAResult.message += "\t\t\t\t\t\t\t\tStack Trace: " + ex.StackTrace;
                            ERAResult.message += "\n\t\t\t\t\t\t\t\t>>>---------------------------------------------------------";

                        }
                    }
                    #endregion SMTP client setup done

                }

                #endregion End!

            }
            #endregion

            #region ReturnResult!

            return ERAResult;
            #endregion

        }


        public ResponseModel SearchWeeklyHistory(long PracticeCode)
        {
            #region Declare_variables!
            DateTime currentDate = DateTime.Now;
            DateTime lastSevenDays = currentDate.AddDays(-7);
            var stats = new ResponseModel();
            #endregion End!
            try
            {
                using (var eraDbModel = new NPMDBEntities())
                {

                    var res = eraDbModel.User_era_request.Where(e => e.PracticeCode == PracticeCode && e.ENTRY_DATE >= lastSevenDays && e.ENTRY_DATE <= currentDate).ToList();
                    stats.Status = "Successfull";
                    stats.Response = res;
                    return stats;
                }

                //This Code is For LocalServer
                #region ForLocalServerToShowHistory
                //List<User_era_request> requests = new List<User_era_request>();
                //using (SqlConnection con = new SqlConnection("Data Source=NOB-IT025;Initial Catalog=CRUDD; Integrated Security=true;MultipleActiveResultSets=True;App=EntityFramework;"))
                //{
                //    SqlCommand cmd = new SqlCommand("WeekHistory", con);
                //    cmd.Parameters.AddWithValue("@PracCode", PracticeCode);
                //    cmd.CommandType = CommandType.StoredProcedure;
                //    con.Open();
                //    SqlDataReader rdr = cmd.ExecuteReader();
                //    while (rdr.Read())
                //    {
                //        var request = new User_era_request()
                //        {
                //            Id = Convert.ToInt32(rdr["Id"]),
                //            USER_ID = Convert.ToInt64(rdr["USER_ID"].ToString()),
                //            USER_NAME = rdr["USER_NAME"].ToString(),
                //            //ENTRY_DATE = rdr["cnic"].ToString(),
                //            PracticeCode = Convert.ToInt64(rdr["PracticeCode"].ToString()),
                //            LogFile_Name = rdr["LogFile_Name"].ToString(),
                //            LogFile_Path = rdr["LogFile_Path"].ToString(),
                //            DOWNLOADED_FILE_COUNT = Convert.ToInt32(rdr["DOWNLOADED_FILE_COUNT"].ToString()),
                //            STATUS = rdr["STATUS"].ToString(),
                //            FTP_EXCEPTION = rdr["FTP_EXCEPTION"].ToString()
                //        };
                //        if (!rdr.IsDBNull(rdr.GetOrdinal("ENTRY_DATE")))
                //        {
                //            request.ENTRY_DATE = rdr.GetDateTime(rdr.GetOrdinal("ENTRY_DATE"));
                //        }
                //        requests.Add(request);
                //    }
                //    stats.Status = "Successfull";
                //    stats.Response = requests;
                //    return stats;
                #endregion
                //End Code

            }
            catch (Exception ex)
            {
                stats.Status = "UnSuccessfull";
                stats.Response = ex.Message;
                return stats;
            }

        }
        private async Task<string> DownloadFile1(StreamWriter writer, SftpClient client, SftpFile file, string directory, string logs)
        {
            try
            {
                using (Stream fileStream = File.OpenWrite(Path.Combine(directory, file.Name)))
                {
                    await client.DownloadAsync(file.FullName, fileStream);
                }
                return "Download Successfull";
            }
            catch (Exception ex)
            {
                writer.WriteLine($"\"Exception Found During Downloading at:\" +{ex.StackTrace}\n {ex.Message}\n at Time:{DateTime.Now}");
                logs += $"\r\nException Found During Downloading at:\" +{ex.StackTrace}\n {ex.Message}\n at Time:{DateTime.Now}";
                //SMPT is for now is using
                #region setting SMTP client for email!
                // SMTP server settings
                string smtpHost = ConfigurationManager.AppSettings["smtp"];
                int smtpPort = Convert.ToInt32(ConfigurationManager.AppSettings["portnumber"]);
                string smtpUsername = ConfigurationManager.AppSettings["username"];
                string smtpPassword = ConfigurationManager.AppSettings["password"];

                // Email recipient
                string emailRecipient = "sfddsf@nobilityrcm.com";
                //In CC
                string TL = "qwe@nobilityrcm.com";
                string AM = "qwe@nobilityrcm.com";
                // Email subject and body
                string subject = "Manual ERA Request Stats – " + DateTime.Now;
                string statsHead = "Manual ERA Request Stats".ToUpper();
                string disclaimer = "Disclaimer:".ToUpper();
                string body = $"\r\n{statsHead}\r\n{logs}\r\n\r\n\r{disclaimer}\r\nPlease do not reply to this email as it is an automated notification. If you require further assistance, please feel free to reach out to our support team or EDI Team.\r\nThank you for your understanding and cooperation.\r\n\r\nBest regards,\nNOBILITY MBS\nNotification Service\r\n";

                using (SmtpClient client1 = new SmtpClient(smtpHost, smtpPort))
                {
                    client1.EnableSsl = true;
                    client1.Credentials = new NetworkCredential(smtpUsername, smtpPassword);

                    MailMessage mailMessage = new MailMessage();
                    mailMessage.From = new MailAddress(smtpUsername);
                    mailMessage.To.Add(emailRecipient);
                    mailMessage.CC.Add(TL);
                    mailMessage.CC.Add(AM);
                    mailMessage.Subject = subject;
                    mailMessage.Body = body;

                    try
                    {
                        // Send the email
                        await client1.SendMailAsync(mailMessage);
                    }
                    catch (SmtpException e)
                    {
                        // Handle any exceptions that occur during email sending
                        // Log or perform any necessary error handling
                        logs += $"Failed to send email: {e.Message}";
                    }
                }
                #endregion SMTP client setup done
                return ex.Message;

            }
        }
        private void CreateDirectories(long practiceCode)
        {
            try
            {
                var basePath = HostingEnvironment.MapPath($"~/{ConfigurationManager.AppSettings["ERADownloadDestination"]}");
                var completePath = $"{basePath}\\{practiceCode}";
                var inboundPath = $"{completePath}\\Inbound";
                var archivePath = $"{completePath}\\Archive";
                var failuresPath = $"{completePath}\\Failures";
                var logPath = $"{completePath}\\LogFiles";
                if (!Directory.Exists(basePath))
                    Directory.CreateDirectory(basePath);
                if (!Directory.Exists(completePath))
                {
                    Directory.CreateDirectory(completePath);
                    Directory.CreateDirectory(inboundPath);
                    Directory.CreateDirectory(archivePath);
                    Directory.CreateDirectory(failuresPath);
                    Directory.CreateDirectory(logPath);
                }
                if (Directory.Exists(inboundPath))
                    Directory.CreateDirectory(inboundPath);
                if (Directory.Exists(archivePath))
                    Directory.CreateDirectory(archivePath);
                if (Directory.Exists(failuresPath))
                    Directory.CreateDirectory(failuresPath);
                if (Directory.Exists(logPath))
                    Directory.CreateDirectory(logPath);
            }
            catch (Exception)
            {
                throw;
            }
        }
        private async Task<ReturnERAResult> ParseAndDump(long practiceCode, string username, StreamWriter wt, string logs)
        {
            #region Create_Variables_Also_Set_TimeSpan!
            ReturnERAResult Final = new ReturnERAResult();
            Final.message = logs;
            Final.RM = new ResponseModel();
            ResponseModel result = new ResponseModel();
            int TFailedFiles = 0;
            TimeSpan retryInterval = TimeSpan.FromSeconds(15);
            wt.WriteLine($"Parsing start at:{DateTime.Now}");
            Final.message += $"\r\nParsing start at:{DateTime.Now}";
            string dsn = ConfigurationManager.ConnectionStrings["DemoERADBModel"].ConnectionString;
            bool throwExceptionOnSyntaxErrors = ConfigurationManager.AppSettings["ThrowExceptionOnSyntaxErrors"] == "true";
            string[] segments = ConfigurationManager.AppSettings["IndexedSegments"].Split(',');
            string parseDirectory = HostingEnvironment.MapPath($"~/{ConfigurationManager.AppSettings["ERADownloadDestination"]}/{practiceCode}/Inbound");
            string parseSearchPattern = ConfigurationManager.AppSettings["ParseSearchPattern"];
            string archiveDirectory = HostingEnvironment.MapPath($"~/{ConfigurationManager.AppSettings["ERADownloadDestination"]}/{practiceCode}/Archive");
            string failureDirectory = HostingEnvironment.MapPath($"~/{ConfigurationManager.AppSettings["ERADownloadDestination"]}/{practiceCode}/Failures");
            string sqlDateType = ConfigurationManager.AppSettings["SqlDateType"];
            int segmentBatchSize = Convert.ToInt32(ConfigurationManager.AppSettings["SqlSegmentBatchSize"]);

            var specFinder = new SpecificationFinder();
            var parser = new X12Parser(throwExceptionOnSyntaxErrors);
            parser.ParserWarning += new X12Parser.X12ParserWarningEventHandler(parser_ParserWarning);
            var repo = new SqlTransactionRepository<int>(dsn, specFinder, segments, ConfigurationManager.AppSettings["schema"], ConfigurationManager.AppSettings["containerSchema"], segmentBatchSize, sqlDateType);
            #endregion End!

            #region FilesParsing_Process_Start_And_Maintain_Logs!
            foreach (var filename in Directory.GetFiles(parseDirectory, parseSearchPattern, SearchOption.AllDirectories))
            {
                byte[] header = new byte[6];
                using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
                {
                    fs.Read(header, 0, 6);
                    fs.Close();
                }
                Encoding encoding = (header[1] == 0 && header[3] == 0 && header[5] == 0) ? Encoding.Unicode : Encoding.UTF8;
                var fi = new FileInfo(filename);
                using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
                {
                    //not used
                    #region notused
                    //int maxRetryCount = 3;
                    //int retryCount = 0;
                    //bool success = false;
                    //do
                    //{
                    #endregion
                    //using
                    try
                    {
                        var interchanges = parser.ParseMultiple(fs, encoding);
                        foreach (var interchange in interchanges)
                        {
                            repo.Save(interchange, filename, username);
                        }
                        if (!string.IsNullOrWhiteSpace(archiveDirectory))
                            MoveTo(fi, parseDirectory, archiveDirectory);
                        result.Status = "Successfull";
                        Final.RM.Status = result.Status;
                    }
                    catch (Exception ex)
                    {
                        #region Exception_And_MailHandling!

                        wt.WriteLine($"Error parsing ${fi.FullName} \n{ex.Message} \n{ex.StackTrace}\nAt Time:{DateTime.Now}");
                        Final.message += $"\r\nError parsing ${fi.FullName} \n{ex.Message} \n{ex.StackTrace}\nAt Time:{DateTime.Now}";
                        NPMLogger.GetInstance().Error($"Error parsing ${fi.FullName} \n{ex.Message} \n{ex.StackTrace}");
                        //SMPT is for now is using
                        #region setting SMTP client for email!
                        // SMTP server settings
                        string smtpHost = ConfigurationManager.AppSettings["smtp"];
                        int smtpPort = Convert.ToInt32(ConfigurationManager.AppSettings["portnumber"]);
                        string smtpUsername = ConfigurationManager.AppSettings["username"];
                        string smtpPassword = ConfigurationManager.AppSettings["password"];

                        // Email recipient
                        string emailRecipient = "dfsdfsd@nobilityrcm.com";
                        //In CC
                        string TL = "sdfsdf@nobilityrcm.com";
                        string AM = "sfdsdfsdq@nobilityrcm.com";
                        // Email subject and body
                        string subject = "Manual ERA Request Stats – " + DateTime.Now;
                        string statsHead = "Manual ERA Request Stats".ToUpper();
                        string disclaimer = "Disclaimer:".ToUpper();
                        string body = $"\r\n{statsHead}\r\nUsername: {username}\r\nRequest Date/Time:{DateTime.Now}\r\n For Practice:{practiceCode}\r\n{Final.message}\r\n\r\n\r{disclaimer}\r\nPlease do not reply to this email as it is an automated notification. If you require further assistance, please feel free to reach out to our support team or EDI Team.\r\nThank you for your understanding and cooperation.\r\n\r\nBest regards,\nNOBILITY MBS\nNotification Service\r\n";

                        using (SmtpClient client1 = new SmtpClient(smtpHost, smtpPort))
                        {
                            client1.EnableSsl = true;
                            client1.Credentials = new NetworkCredential(smtpUsername, smtpPassword);

                            MailMessage mailMessage = new MailMessage();
                            mailMessage.From = new MailAddress(smtpUsername);
                            mailMessage.To.Add(emailRecipient);
                            mailMessage.CC.Add(TL);
                            mailMessage.CC.Add(AM);
                            mailMessage.Subject = subject;
                            mailMessage.Body = body;

                            try
                            {
                                // Send the email
                                await client1.SendMailAsync(mailMessage);
                            }
                            catch (SmtpException e)
                            {
                                // Handle any exceptions that occur during email sending
                                // Log or perform any necessary error handling
                                Final.message += $"Failed to send email: {e.Message}";
                            }
                        }
                        #endregion SMTP client setup done

                        if (!string.IsNullOrEmpty(failureDirectory))
                        {
                            TFailedFiles++;
                            MoveTo(fi, parseDirectory, failureDirectory);
                        }
                        result.Status = $"Error parsing ${fi.FullName} \n{ex.Message} \n{ex.StackTrace}\nAt Time:{DateTime.Now}";

                        #endregion End!


                    }
                    //} while (!success && retryCount < maxRetryCount);
                }
                result.Response = TFailedFiles;
                Final.RM.Response = result.Response;
            }

            wt.WriteLine($"Parsing End at:{DateTime.Now}");
            Final.message += $"\r\nParsing End at:{DateTime.Now}";
            #endregion End!

            #region ReturnResult!
            result.Response = TFailedFiles;
            result.Status = "Successfull";
            Final.RM.Response = result.Response;
            Final.RM.Status = result.Status;
            return Final;
            #endregion End!
        }
        //Skipped code to handel duplicate files.
        //private static void MoveTo(FileInfo fi, string sourceDirectory, string targetDirectory)
        //{
        //    string targetFilename = string.Format("{0}{1}", targetDirectory, fi.FullName.Replace(sourceDirectory, ""));
        //    FileInfo targetFile = new FileInfo(targetFilename);
        //    try
        //    {
        //        if (!targetFile.Directory.Exists)
        //        {
        //            targetFile.Directory.Create();
        //        }
        //        fi.MoveTo(targetFilename);
        //    }
        //    catch (Exception exc2)
        //    {
        //        Trace.TraceError("Error moving {0} to {1}: {2}\n{3}", fi.FullName, targetFilename, exc2.Message, exc2.StackTrace);
        //    }
        //}

        //Added by HAMZA ZULFIQAR for change duplicate file names.
        private static void MoveTo(FileInfo fi, string sourceDirectory, string targetDirectory)
        {
            string targetFilename = string.Format("{0}{1}", targetDirectory, fi.FullName.Replace(sourceDirectory, ""));
            FileInfo targetFile = new FileInfo(targetFilename);
            try
            {
                if (!targetFile.Directory.Exists)
                {
                    targetFile.Directory.Create();
                }
                if (targetFile.Exists)
                {
                    string newTargetFilename = GetUniqueFilename(targetFile.FullName);
                    targetFile.MoveTo(newTargetFilename);
                }
                fi.MoveTo(targetFilename);
            }
            catch (Exception exc2)
            {
                Trace.TraceError("Error moving {0} to {1}: {2}\n{3}", fi.FullName, targetFilename, exc2.Message, exc2.StackTrace);
            }
        }
        //Added by HAMZA ZULFIQAR for change duplicate file names.
        private static string GetUniqueFilename(string existingFilePath)
        {
            string directory = Path.GetDirectoryName(existingFilePath);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(existingFilePath);
            string extension = Path.GetExtension(existingFilePath);
            int counter = 1;
            string newFilePath = existingFilePath;
            while (File.Exists(newFilePath))
            {
                string newName = $"{fileNameWithoutExtension}_{counter}{extension}";
                newFilePath = Path.Combine(directory, newName);
                counter++;
            }
            return newFilePath;
        }
        
        private static void parser_ParserWarning(object sender, X12ParserWarningEventArgs args)
        {
            Trace.TraceWarning("Error parsing interchange {0} at position {1}: {2}", args.InterchangeControlNumber, args.SegmentPositionInInterchange, args.Message);
        }
        private async Task<List<DownloadedFile>> GetPreviouseDownloadedFiles(List<long> practiceCode, string log)
        {
            try
            {
                using (var eraDbModel = new DemoERADBModel())
                {
                    return await eraDbModel.DownloadedFiles.Where(df => practiceCode.Contains(df.PracticeCode)).ToListAsync();
                }
            }
            catch (Exception ex)
            {
                //SMPT is for now is using
                #region setting SMTP client for email!
                // SMTP server settings
                log += $"\r\nException Found At:{ex.Message}\nAt Stack Trace:{ex.StackTrace}\nAt Time/Date{DateTime.Now}";
                string smtpHost = ConfigurationManager.AppSettings["smtp"];
                int smtpPort = Convert.ToInt32(ConfigurationManager.AppSettings["portnumber"]);
                string smtpUsername = ConfigurationManager.AppSettings["username"];
                string smtpPassword = ConfigurationManager.AppSettings["password"];

                // Email recipient
                string emailRecipient = "sdfsdf@nobilityrcm.com";
                //In CC
                string TL = "sdfsdf@nobilityrcm.com";
                string AM = "sfdsdfsdq@nobilityrcm.com";
                // Email subject and body
                string subject = "Manual ERA Request Stats – " + DateTime.Now;
                string statsHead = "Manual ERA Request Stats".ToUpper();
                string disclaimer = "Disclaimer:".ToUpper();
                string body = $"\r\n{statsHead}\r\nRequest Date/Time:{DateTime.Now}\r\n{log}\r\n\r\n\r{disclaimer}\r\nPlease do not reply to this email as it is an automated notification. If you require further assistance, please feel free to reach out to our support team or EDI Team.\r\nThank you for your understanding and cooperation.\r\n\r\nBest regards,\nNOBILITY MBS\nNotification Service\r\n";

                using (SmtpClient client1 = new SmtpClient(smtpHost, smtpPort))
                {
                    client1.EnableSsl = true;
                    client1.Credentials = new NetworkCredential(smtpUsername, smtpPassword);

                    MailMessage mailMessage = new MailMessage();
                    mailMessage.From = new MailAddress(smtpUsername);
                    mailMessage.To.Add(emailRecipient);
                    mailMessage.CC.Add(TL);
                    mailMessage.CC.Add(AM);
                    mailMessage.Subject = subject;
                    mailMessage.Body = body;

                    try
                    {
                        // Send the email
                        await client1.SendMailAsync(mailMessage);
                    }
                    catch (SmtpException e)
                    {
                        // Handle any exceptions that occur during email sending
                        // Log or perform any necessary error handling
                        log += $"Failed to send email: {e.Message}";
                    }
                }
                #endregion SMTP client setup done
                throw;
            }
        }
        private bool isAlreadyDownloaded(List<DownloadedFile> previousDownloadedFiles, SftpFile file, long practiceCode)
        {
            if (previousDownloadedFiles.FirstOrDefault(d => d.Name == file.Name && d.PracticeCode == practiceCode && d.Length == file.Length) == null)
                return false;
            return true;
        }

        //not used...Any where
        #region create Table
        //private void CreateTable()
        //{
        //    string dsn = ConfigurationManager.ConnectionStrings["EraDbModel2"].ConnectionString;
        //    bool throwExceptionOnSyntaxErrors = ConfigurationManager.AppSettings["ThrowExceptionOnSyntaxErrors"] == "true";
        //    string[] segments = ConfigurationManager.AppSettings["IndexedSegments"].Split(',');
        //    string sqlDateType = ConfigurationManager.AppSettings["SqlDateType"];
        //    int segmentBatchSize = Convert.ToInt32(ConfigurationManager.AppSettings["SqlSegmentBatchSize"]);

        //    var specFinder = new SpecificationFinder();
        //    var parser = new X12Parser(throwExceptionOnSyntaxErrors);
        //    parser.ParserWarning += new X12Parser.X12ParserWarningEventHandler(parser_ParserWarning);
        //var repo = new SqlTransectionMigrationRepository<int>(dsn, specFinder, segments, ConfigurationManager.AppSettings["schema"], ConfigurationManager.AppSettings["containerSchema"], segmentBatchSize, sqlDateType);
        //repo.EnsureSchema();
        //}
        #endregion end


    }
}