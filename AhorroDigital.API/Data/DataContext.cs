using AhorroDigital.API.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;

namespace AhorroDigital.API.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }

        public DbSet<AccountType> AccountTypes { get; set; }
        public DbSet<DocumentType> DocumentTypes { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AccountType>().HasIndex(x => x.Name).IsUnique();
            modelBuilder.Entity<DocumentType>().HasIndex(x => x.Name).IsUnique();

        }

    }
}
