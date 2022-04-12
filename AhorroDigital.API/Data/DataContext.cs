using AhorroDigital.API.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AhorroDigital.API.Data
{
    public class DataContext:IdentityDbContext<User>
    {
        public DataContext(DbContextOptions<DataContext> options): base(options)
        {

        }

        public DbSet<TypeOfSaving> typeOfSavings { get; set; }
        public DbSet<TypeOfRetirement> typeOfRetirements { get; set; }

        public DbSet<DocumentType> documentTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<TypeOfSaving>().HasIndex(x => x.Description).IsUnique();
            modelBuilder.Entity<TypeOfRetirement>().HasIndex(x => x.Description).IsUnique();
            modelBuilder.Entity<DocumentType>().HasIndex(x => x.Description).IsUnique();
        }
    }
}
