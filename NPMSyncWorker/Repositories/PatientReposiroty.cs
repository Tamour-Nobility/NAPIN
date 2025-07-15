using Dapper;
using NPMSyncWorker.Entities;
using NPMSyncWorker.Repositories.Interfaces;
using System.Collections.Generic;
using System.Data;

namespace NPMSyncWorker.Repositories
{
    internal class PatientReposiroty : RepositoryBase, IPatientRepository
    {
        public PatientReposiroty(IDbTransaction Transaction) : base(Transaction)
        {

        }

        public List<Patient> GetPatientsForSync(long practiceCode)
        {
            return Connection.Query<Patient>(Procedures.GetPatientsForSync,
                 new { pPracticeCode = practiceCode },
                 Transaction,
                 commandType: CommandType.StoredProcedure).AsList();
        }
    }
}
