using System;
using System.ComponentModel.DataAnnotations;

namespace NPMSyncWorker.Entities
{
    internal class PracticeSynchronization
    {
        [Key]
        public long PracticeSyncId { get; set; }
        public long PracticeId { get; set; }
        public DateTimeOffset StartedAt { get; set; }
        public DateTimeOffset CompletedAt { get; set; }
        public long SynchronizedBy { get; set; }
        public int SynchronizationProgress { get; set; }
        public bool IsFailed { get; set; }
        public string Notes { get; set; }
    }
}
