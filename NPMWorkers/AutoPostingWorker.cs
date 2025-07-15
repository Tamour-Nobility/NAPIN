using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NPMWorkers.Entities;
using NPMWorkers.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NPMWorkers
{
    public class AutoPostingWorker : BackgroundService
    {
        private readonly ILogger<AutoPostingWorker> _logger;
        private readonly IConfiguration _configuration;
        private readonly int _delay;
        private readonly IPracticeRepository _practiceRepository;
        private readonly IERAImportRepository _eraImportRepository;

        public AutoPostingWorker(ILogger<AutoPostingWorker> logger,
            IConfiguration configuration,
            IPracticeRepository practiceRepository,
            IERAImportRepository eraImportRepository)
        {
            _logger = logger;
            _configuration = configuration;
            _practiceRepository = practiceRepository;
            _delay = _configuration.GetValue<int>("delay");
            _eraImportRepository = eraImportRepository;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {

                _logger.LogInformation("ERA Auto import started at : {time}", DateTimeOffset.Now);
                ImportERA();
                _logger.LogInformation("ERA Auto import ended at : {time}", DateTimeOffset.Now);
                await Task.Delay(_delay, stoppingToken);
            }
        }

        private void ImportERA()
        {
            try
            {
                var ftplist = _practiceRepository.GetFTPEnabledPractices().Take(1);
                List<DownloadedFile> downloadedFiles = new List<DownloadedFile>();
                if (ftplist.Count() > 0)
                {
                    downloadedFiles = _eraImportRepository.GetPreviouseDownloadedFiles(ftplist.Select(ftp => ftp.PracticeCode).ToList());
                    foreach (var ftp in ftplist)
                    {
                        _eraImportRepository.Download(
                            ftp.PracticeCode,
                            ftp.Username,
                            ftp.Password,
                            ftp.Host,
                            ftp.Port,
                            _configuration.GetValue<string>("X12Parser:Source"),
                            downloadedFiles.Where(df => df.PracticeCode == ftp.PracticeCode).ToList());
                        _eraImportRepository.ParseAndDump(ftp.PracticeCode);
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
