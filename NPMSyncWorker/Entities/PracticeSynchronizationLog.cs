using System;
using System.ComponentModel.DataAnnotations;

namespace NPMSyncWorker.Entities
{
    internal class PracticeSynchronizationLog
    {
        [Key]
        public long LogId { get; set; }
        public long PracticeSyncId { get; set; }
        public string LogMessage { get; set; }
        public DateTimeOffset LogTime { get; set; }
    }
}
