using NPMWorkers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPMWorkers.Repositories
{
    public interface IPracticeRepository
    {
        public List<PracticeFTPViewModel> GetFTPEnabledPractices();
    }
}
