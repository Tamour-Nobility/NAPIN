using NPMWorkers.Data;
using NPMWorkers.Models;
using NPMWorkers.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPMWorkers.Services
{
    public class PracticeRepository : IPracticeRepository
    {
        private readonly NPMDBContext _npmContext;
        public PracticeRepository(NPMDBContext npmContext)
        {
            _npmContext = npmContext;
        }

        public List<PracticeFTPViewModel> GetFTPEnabledPractices()
        {
            try
            {
                var practices = _npmContext.Practices.Where(p => !(p.Deleted ?? false) && (p.Is_Active ?? false) && p.FTP_ENABLE == true && !string.IsNullOrEmpty(p.FTP_Path)).ToList();
                List<PracticeFTPViewModel> ftps = new List<PracticeFTPViewModel>();
                foreach (var practice in practices)
                {
                    string[] ftpInfo = practice.FTP_Path.Split('|');
                    if (ftpInfo.Length == 3)
                    {
                        ftps.Add(new PracticeFTPViewModel()
                        {
                            Username = ftpInfo[0],
                            Password = ftpInfo[1],
                            Host = ftpInfo[2],
                            // TODO: Add Destination
                            //Destination = ConfigurationManager.AppSettings["ERASource"],
                            Port = 22,
                            PracticeCode = practice.Practice_Code
                        });
                    }
                }
                return ftps;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
