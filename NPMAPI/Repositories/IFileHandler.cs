using System.Web;
using NPMAPI.Models;

namespace NPMAPI.Repositories
{
    public interface IFileHandler
    {
        ResponseModel UploadImage(HttpPostedFile File, string FilePath, string[] SupportedTypes, string fileNewName, long? MaximumUploadSize);

        ResponseModel UploadTicketImage(HttpPostedFile File, string FilePath, string[] SupportedTypes, string fileNewName, long? MaximumUploadSize);
        
        ResponseModel DownloadFile(string FilePath);
    }
}