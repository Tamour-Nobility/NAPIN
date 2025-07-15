using System.Collections.Generic;
using System.Threading.Tasks;
using NPMAPI.Models;
using static NPMAPI.Services.ERAImportService;

namespace NPMAPI.Repositories
{
    public interface IERAImport
    {
        //void Download(long practiceCode, string username, string password, string host, int port, string path, List<DownloadedFile> previousDownloadedFiles);
        //void ParseAndDump(long practiceCode);
        //List<DownloadedFile> GetPreviouseDownloadedFiles(List<long> practiceCode);

        Task<ReturnERAResult> Era_User_Request(ERA_Request ERA);

        Task<ReturnERAResult> ERA_Download_and_Dump_Process(string username, string message, ERA_Request ERA);

        ResponseModel SearchWeeklyHistory(long PracticeCode);
    }
}
