using Dapper;
using NPMSyncWorker.Entities;
using NPMSyncWorker.Repositories.Interfaces;
using System.Data;
using System.Linq;

namespace NPMSyncWorker.Repositories
{
    internal class SynchronizationRepository : RepositoryBase, ISynchronizationRepository
    {
        public SynchronizationRepository(IDbTransaction transaction) : base(transaction)
        {

        }

        public int Add(PracticeSynchronization entity)
        {
            throw new System.NotImplementedException();
        }

        public int Update(PracticeSynchronization entity)
        {
            var param = new
            {
                pPracticeSyncId = entity.PracticeSyncId,
                pNotes = entity.Notes,
                pIsFailed = entity.IsFailed
            };
            return AddOrUpdate<PracticeSynchronization>(Procedures.UpdatePracticeSynchronization, param);
        }
    }
}
