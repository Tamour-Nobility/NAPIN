using NPMSyncWorker.Entities;
using System.Collections.Generic;

namespace NPMSyncWorker.Repositories.Interfaces
{
    internal interface IPatientRepository
    {
        List<Patient> GetPatientsForSync(long practiceCode);
    }
}
