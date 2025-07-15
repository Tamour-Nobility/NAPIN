using NPMSyncWorker.Repositories.Interfaces;
using System.Collections.Generic;
using System.Data;
using Dapper;
using System.Linq;
using NPMSyncWorker.Models;
using NPMSyncWorker.Entities;

namespace NPMSyncWorker.Repositories
{
    internal class PracticeReposiroty : RepositoryBase, IPracticeRepository, IPracticeLocationsRepository
    {
        public PracticeReposiroty(IDbTransaction transaction) : base(transaction)
        {
        }

        public List<PracticeToSync> GetPracticesForSync()
        {
            return Connection.Query<PracticeToSync>(Procedures.GetPracticesForSync, new { }, transaction: Transaction, commandType: CommandType.StoredProcedure).ToList();
        }

        public List<PracticeLocations> GetPracticeLocationsForSync(long practice_Code)
        {
            return Connection.Query<PracticeLocations>(Procedures.GetPracticeLocationsForSync, new { pPracticeCode = practice_Code }, transaction: Transaction, commandType: CommandType.StoredProcedure).ToList();
        }

    }
}
