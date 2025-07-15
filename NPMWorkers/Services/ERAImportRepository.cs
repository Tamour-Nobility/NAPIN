using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using NPMWorkers.Data;
using NPMWorkers.Entities;
using NPMWorkers.Repositories;
using OopFactory.X12.Parsing;
using OopFactory.X12.Repositories;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPMWorkers.Services
{
    public class ERAImportRepository : IERAImportRepository
    {
        private readonly ERADBContext _eraContext;
        private readonly ILogger<ERAImportRepository> _logger;
        private readonly IConfiguration _configuration;

        public ERAImportRepository(ILogger<ERAImportRepository> logger, ERADBContext eraContext, IConfiguration configuration)
        {
            _logger = logger;
            _eraContext = eraContext;
            _configuration = configuration;
        }

        public void Download(long practiceCode, string username, string password, string host, int port, string path, List<DownloadedFile> previousDownloadedFiles)
        {
            try
            {
                CreateDirectories(practiceCode);
                List<DownloadedFile> newFiles = new List<DownloadedFile>();
                using (SftpClient client = new SftpClient(host, port, username, password))
                {
                    client.Connect();
                    var files = client.ListDirectory(path);
                    foreach (SftpFile file in files.Take(2))
                    {
                        if (!file.IsDirectory && !file.IsSymbolicLink && !IsAlreadyDownloaded(previousDownloadedFiles, file, practiceCode))
                        {
                            DownloadFile(client, file, Path.Combine(_configuration.GetValue<string>("X12Parser:Destination"), $"{practiceCode}/Inbound"));
                            newFiles.Add(new DownloadedFile()
                            {
                                Name = file.Name,
                                Length = file.Length,
                                DownloadedAt = DateTime.Now,
                                DownloadedBy = Environment.UserName,
                                PracticeCode = practiceCode,
                            });
                        }
                    }
                    if (newFiles.Count() > 0)
                    {
                        _eraContext.DownloadedFiles.AddRange(newFiles);
                        _eraContext.SaveChanges();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<DownloadedFile> GetPreviouseDownloadedFiles(List<long> practiceCode)
        {
            try
            {
                return _eraContext.DownloadedFiles.Where(df => practiceCode.Contains(df.PracticeCode)).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void ParseAndDump(long practiceCode)
        {
            string dsn = _configuration.GetConnectionString("era");
            bool throwExceptionOnSyntaxErrors = _configuration.GetValue<bool>("X12Parser:ThrowExceptionOnSyntaxErrors");
            string[] segments = _configuration.GetValue<string>("X12Parser:IndexedSegments").Split(',');
            string parseDirectory = Path.Combine(_configuration.GetValue<string>("X12Parser:Destination"), $"{practiceCode}/Inbound");
            string parseSearchPattern = _configuration.GetValue<string>("X12Parser:ParseSearchPattern");
            string archiveDirectory = Path.Combine(_configuration.GetValue<string>("X12Parser:Destination"), $"{practiceCode}/Archive");
            string failureDirectory = Path.Combine(_configuration.GetValue<string>("X12Parser:Destination"), $"{practiceCode}/Failures");
            string sqlDateType = _configuration.GetValue<string>("X12Parser:SqlDateType");
            int segmentBatchSize = _configuration.GetValue<int>("X12Parser:SqlSegmentBatchSize");

            var specFinder = new SpecificationFinder();
            var parser = new X12Parser(throwExceptionOnSyntaxErrors);
            parser.ParserWarning += new X12Parser.X12ParserWarningEventHandler(parser_ParserWarning);
            var repo = new SqlTransactionRepository<int>(dsn, specFinder, segments, _configuration.GetValue<string>("X12Parser:Schema"), _configuration.GetValue<string>("X12Parser:ContainerSchema"), segmentBatchSize, sqlDateType);

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
                    try
                    {
                        var interchanges = parser.ParseMultiple(fs, encoding);
                        foreach (var interchange in interchanges)
                        {
                            repo.Save(interchange, filename, Environment.UserName);
                        }
                        if (!string.IsNullOrWhiteSpace(archiveDirectory))
                            MoveTo(fi, parseDirectory, archiveDirectory);
                    }
                    catch (Exception ex)
                    {
                        if (!string.IsNullOrEmpty(failureDirectory))
                            MoveTo(fi, parseDirectory, failureDirectory);
                    }
                }
            }
        }
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
                fi.MoveTo(targetFilename);
            }
            catch (Exception exc2)
            {
                Trace.TraceError("Error moving {0} to {1}: {2}\n{3}", fi.FullName, targetFilename, exc2.Message, exc2.StackTrace);
            }
        }
        private static void parser_ParserWarning(object sender, X12ParserWarningEventArgs args)
        {
            Trace.TraceWarning("Error parsing interchange {0} at position {1}: {2}", args.InterchangeControlNumber, args.SegmentPositionInInterchange, args.Message);
        }

        private void DownloadFile(SftpClient client, SftpFile file, string directory)
        {
            try
            {
                using (Stream fileStream = File.OpenWrite(Path.Combine(directory, file.Name)))
                {
                    client.DownloadFile(file.FullName, fileStream);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        private void CreateDirectories(long practiceCode)
        {
            try
            {
                var basePath = _configuration.GetValue<string>("X12Parser:Destination");
                var completePath = $"{basePath}/{practiceCode}";
                var inboundPath = $"{completePath}/Inbound";
                var archivePath = $"{completePath}/Archive";
                var failuresPath = $"{completePath}/Failures";
                if (!Directory.Exists(basePath))
                    Directory.CreateDirectory(basePath);
                if (!Directory.Exists(completePath))
                {
                    Directory.CreateDirectory(completePath);
                    Directory.CreateDirectory(inboundPath);
                    Directory.CreateDirectory(archivePath);
                    Directory.CreateDirectory(failuresPath);
                }
                if (Directory.Exists(inboundPath))
                    Directory.CreateDirectory(inboundPath);
                if (Directory.Exists(archivePath))
                    Directory.CreateDirectory(archivePath);
                if (Directory.Exists(failuresPath))
                    Directory.CreateDirectory(failuresPath);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool IsAlreadyDownloaded(List<DownloadedFile> previousDownloadedFiles, SftpFile file, long practiceCode)
        {
            if (previousDownloadedFiles.FirstOrDefault(d => d.Name == file.Name && d.PracticeCode == practiceCode && d.Length == file.Length) == null)
                return false;
            return true;
        }
    }
}
