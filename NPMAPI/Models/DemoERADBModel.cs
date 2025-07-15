using System;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;

namespace NPMAPI.Models
{
    public partial class DemoERADBModel : DbContext
    {
        public DemoERADBModel()
            : base("name=DemoERADBModel")
        {
        }

        public virtual DbSet<User_era_request> USER_ERA_REQUESTS { get; set; }

        public virtual DbSet<DownloadedFile> DownloadedFiles { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User_era_request>()
               .Property(e => e.USER_NAME)
               .IsUnicode(false);

            modelBuilder.Entity<User_era_request>()
                .Property(e => e.FTP_EXCEPTION)
                .IsUnicode(false);

            modelBuilder.Entity<User_era_request>()
                .Property(e => e.STATUS)
                .IsUnicode(false);

            modelBuilder.Entity<User_era_request>()
                .Property(e => e.LogFile_Name)
                .IsUnicode(false);

            modelBuilder.Entity<User_era_request>()
                .Property(e => e.LogFile_Path)
                .IsUnicode(false);

            modelBuilder.Entity<DownloadedFile>()
               .Property(e => e.Name)
               .IsUnicode(false);

            modelBuilder.Entity<DownloadedFile>()
                .Property(e => e.DownloadedBy)
                .IsUnicode(false);


        }


    }
}

