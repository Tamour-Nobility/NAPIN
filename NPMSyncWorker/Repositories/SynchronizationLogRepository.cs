using NPMSyncWorker.Entities;
using NPMSyncWorker.Repositories.Interfaces;
using System.Data;

namespace NPMSyncWorker.Repositories
{
    internal class SynchronizationLogRepository : RepositoryBase, ISynchronizationLogRepository
    {
        public SynchronizationLogRepository(IDbTransaction transaction) : base(transaction)
        {

        }

        public int Add(PracticeSynchronizationLog entity)
        {
            return AddOrUpdate<PracticeSynchronizationLog>(
                Procedures.AddPracticeSyncLog,
                new
                {
                    pPracticeSyncId = entity.PracticeSyncId,
                    pLogMessage = entity.PracticeSyncId
                });
        }

        public int Update(PracticeSynchronizationLog entity)
        {
            throw new System.NotImplementedException();
        }
    }
}
