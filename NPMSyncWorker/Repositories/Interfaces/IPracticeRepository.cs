using NPMSyncWorker.Entities;
using NPMSyncWorker.Models;
using System.Collections.Generic;

namespace NPMSyncWorker.Repositories.Interfaces
{
    internal interface IPracticeRepository
    {
        List<PracticeToSync> GetPracticesForSync();
    }
}
