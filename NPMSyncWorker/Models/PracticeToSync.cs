using NPMSyncWorker.Entities;

namespace NPMSyncWorker.Models
{
    internal class PracticeToSync : Entities.Practice
    {
        public long SynchronizedBy { get; set; }
        public long PracticeSyncId { get; set; }
    }
}
