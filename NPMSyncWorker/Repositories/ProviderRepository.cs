using Dapper;
using NPMSyncWorker.Entities;
using NPMSyncWorker.Repositories.Interfaces;
using System.Collections.Generic;
using System.Data;

namespace NPMSyncWorker.Repositories
{
    internal class ProviderRepository : RepositoryBase, IProviderRepository
    {
        public ProviderRepository(IDbTransaction Transaction) : base(Transaction)
        {

        }

        public List<Provider> GetProvidersForSync(long practiceCode)
        {
            return
                Connection.Query<Provider>(
                    Procedures.GetProvidersForSync,
                    new { pPracticeCode = practiceCode },
                    transaction: Transaction,
                    commandType: CommandType.StoredProcedure
                    ).AsList();
        }
    }
}
