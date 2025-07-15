using NPMWorkers.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPMWorkers.Repositories
{
    public interface IERAImportRepository
    {
        void Download(long practiceCode, string username, string password, string host, int port, string path, List<DownloadedFile> previousDownloadedFiles);
        void ParseAndDump(long practiceCode);
        List<DownloadedFile> GetPreviouseDownloadedFiles(List<long> practiceCode);
    }
}
