using Microsoft.EntityFrameworkCore;
using NPMWorkers.Entities;

namespace NPMWorkers.Data
{
    public class NPMDBContext : DbContext
    {
        public NPMDBContext(DbContextOptions<NPMDBContext> options)
          : base(options)
        {

        }
        public DbSet<Practice> Practices { get; set; }
    }
}