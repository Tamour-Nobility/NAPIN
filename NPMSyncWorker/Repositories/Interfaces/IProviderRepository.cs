using NPMSyncWorker.Entities;
using System.Collections.Generic;

namespace NPMSyncWorker.Repositories.Interfaces
{
    internal interface IProviderRepository
    {
        List<Provider> GetProvidersForSync(long practiceCode);
    }
}
