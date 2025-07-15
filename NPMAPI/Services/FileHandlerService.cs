using System;
using System.IO;
using System.Linq;
using System.Web;
using NPMAPI.Models;
using NPMAPI.Repositories;

namespace NPMAPI.Services
{
    public class FileHandlerService : IFileHandler
    {
        public ResponseModel UploadImage(HttpPostedFile File, string FilePath, string[] SupportedTypes, string fileNewName, long? MaximumUploadSize)
        {
            try
            {
                string fileName = File.FileName;
                string ext = Path.GetExtension(fileName).ToLower();
                if (!SupportedTypes.Contains(ext))
                    return new ResponseModel() { Status = "failure", Response = "Invalid file type." };
                if (File.ContentLength > MaximumUploadSize)
                    return new ResponseModel() { Status = "failure", Response = "Maximum file size exceeded." };
                if (ValidateDirectory(FilePath))
                    File.SaveAs($"{FilePath}{ext}");
                return new ResponseModel() { Status = "success", Response = $"{fileNewName}{ext}" };
            }
            catch (Exception ex)
            {
                return new ResponseModel() { Status = ex.ToString() };
            }
        }


        public ResponseModel UploadTicketImage(HttpPostedFile file, string filePath, string[] supportedTypes, string fileNewName, long? maximumUploadSize)
        {
            try
            {
                string fileName = file.FileName;
                string ext = Path.GetExtension(fileName).ToLower();

                if (!supportedTypes.Contains(ext))
                    return new ResponseModel() { Status = "failure", Response = "Invalid file type." };

                if (file.ContentLength > maximumUploadSize)
                    return new ResponseModel() { Status = "failure", Response = "Maximum file size exceeded." };

                if (ValidateDirectory(filePath))
                {
                    // ✅ Combine full path with unique file name
                    string fullPath = Path.Combine(filePath, $"{fileNewName}{ext}");

                    // ✅ Save using full unique file name
                    file.SaveAs(fullPath);

                    return new ResponseModel() { Status = "success", Response = $"{fileNewName}{ext}" };
                }

                return new ResponseModel() { Status = "failure", Response = "Directory validation failed." };
            }
            catch (Exception ex)
            {
                return new ResponseModel() { Status = "failure", Response = ex.Message };
            }
        }


        public ResponseModel DownloadFile(string FilePath)
        {
            try
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(FilePath);
                if (directoryInfo == null)
                    return new ResponseModel() { Status = "Directory not found." };
                if (File.Exists(FilePath))
                {
                    byte[] imgData = File.ReadAllBytes($"{FilePath}");
                    return new ResponseModel() { Status = "success", Response = imgData };
                }
                else
                {
                    return new ResponseModel()
                    {
                        Status = "error",
                        Response = "File not found"
                    };
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool ValidateDirectory(string path)
        {
            string directory = path.Substring(0, path.LastIndexOf("\\"));
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            return true;
        }
    }
}