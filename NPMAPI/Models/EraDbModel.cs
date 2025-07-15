using System.Data.Entity;

namespace NPMAPI.Models
{
    public partial class EraDbModel : DbContext
    {
        public EraDbModel()
            : base("name=EraDbModel")
        {
        }

        public virtual DbSet<DownloadedFile> DownloadedFiles { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DownloadedFile>()
                .Property(e => e.Name)
                .IsUnicode(false);

            modelBuilder.Entity<DownloadedFile>()
                .Property(e => e.DownloadedBy)
                .IsUnicode(false);
        }
    }
}
