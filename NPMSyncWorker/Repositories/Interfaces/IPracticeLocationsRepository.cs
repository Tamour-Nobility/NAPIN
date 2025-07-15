using NPMSyncWorker.Entities;
using System.Collections.Generic;

namespace NPMSyncWorker.Repositories.Interfaces
{
    internal interface IPracticeLocationsRepository
    {
        List<PracticeLocations> GetPracticeLocationsForSync(long practice_Code);
    }
}
