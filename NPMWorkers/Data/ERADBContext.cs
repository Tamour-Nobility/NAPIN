using Microsoft.EntityFrameworkCore;
using NPMWorkers.Entities;

namespace NPMWorkers.Data
{
    public class ERADBContext : DbContext
    {
        public ERADBContext(DbContextOptions<ERADBContext> options)
          : base(options)
        {
        }

        public virtual DbSet<DownloadedFile> DownloadedFiles { get; set; }

    }
}
