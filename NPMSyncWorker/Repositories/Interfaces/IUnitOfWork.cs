using System;

namespace NPMSyncWorker.Repositories.Interfaces
{
    internal interface IUnitofWork : IDisposable
    {
        IPracticeRepository PracticeRepository { get; }
        IPracticeLocationsRepository PracticeLocationsRepository { get; }
        ISynchronizationRepository SynchronizationRepository { get; }
        ISynchronizationLogRepository SynchronizationLogRepository { get; }
        IPatientRepository PatientRepository { get; }
        IProviderRepository ProviderRepository { get; }
        void SaveChanges();
    }
}
