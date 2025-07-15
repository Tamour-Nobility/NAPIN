using System;
using System.ComponentModel.DataAnnotations;

namespace NPMWorkers.Entities
{
    public class DownloadedFile
    {
        [Key]
        public long Id { get; set; }
        public string Name { get; set; }
        public DateTime DownloadedAt { get; set; }
        public string DownloadedBy { get; set; }
        public long Length { get; set; }
        public long PracticeCode { get; set; }
    }
}
