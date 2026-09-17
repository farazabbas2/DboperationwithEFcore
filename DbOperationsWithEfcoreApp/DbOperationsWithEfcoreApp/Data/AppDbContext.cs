using DbOperationsWithEfcoreApp.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
namespace DbOperationsWithEfcoreApp.Data
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) :base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Currency>()
           .HasQueryFilter(c => c.isDeleted);

            modelBuilder.Entity<Currency>().HasData(
                new Currency() { id = 1, Title= "INR", description="indian inr" },
                new Currency() { id = 2, Title= "Dollar", description="dollar" },
                new Currency() { id = 3, Title= "Euro", description="euro" },
                new Currency() { id = 4, Title= "Dinar", description="dinar" }
                );

            modelBuilder.Entity<Language>().HasQueryFilter(c => c.isDeleted);

            modelBuilder.Entity<Language>().HasData(
              new Language() { id = 1, Title = "Hindi", Description = "all bout hindi" },
              new Language() { id = 2, Title = "Tamil", Description = "all about tamil" },
              new Language() { id = 3, Title = "Punjabi", Description = "all about punjabi" },
              new Language() { id = 4, Title = "Urdu", Description = "all about urdu" }
              );
        }

 

        public DbSet<Book> Books { get; set; }
        public DbSet<Language> Languages { get; set; }
        public DbSet<color> Color {  get; set; }

        public DbSet<Currency> Currency { get; set; }

    }
}
