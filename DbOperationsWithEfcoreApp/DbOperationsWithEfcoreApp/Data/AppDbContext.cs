using DbOperationsWithEfcoreApp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DbOperationsWithEfcoreApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ==========================================
            // 1. QUERY FILTERS (1 / true = ACTIVE, 0 / false = INACTIVE/DELETED)
            // ==========================================
            // User requirement: IsActive = 1 (true) => Active record, 0 (false) => Inactive/Deleted.
            // Query filter sirf ACTIVE records (IsActive == true) dikhayega.

            modelBuilder.Entity<Currency>()
                .HasQueryFilter(c => c.IsActive);

            modelBuilder.Entity<Language>()
                .HasQueryFilter(l => l.IsActive);

            modelBuilder.Entity<Color>()
                .HasQueryFilter(c => c.IsActive);

            modelBuilder.Entity<Book>()
                .HasQueryFilter(b => b.IsActive);


            // ==========================================
            // 2. SEED DATA (IsActive = true / 1)
            // ==========================================
            modelBuilder.Entity<Currency>().HasData(
                new Currency { id = 1, Title = "INR", description = "Indian Rupee", IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
                new Currency { id = 2, Title = "Dollar", description = "US Dollar", IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
                new Currency { id = 3, Title = "Euro", description = "Euro", IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
                new Currency { id = 4, Title = "Dinar", description = "Dinar", IsActive = true, CreatedAt = new DateTime(2024, 1, 1) }
            );

            modelBuilder.Entity<Language>().HasData(
                new Language { Id = 1, Name = "Hindi", Description = "Indian Rupee", IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
                new Language { Id = 2, Name = "Tamil", Description = "Tamil Language", IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
                new Language { Id = 3, Name = "Punjabi", Description = "Punjabi Language", IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
                new Language { Id = 4, Name = "Urdu", Description = "Urdu Language", IsActive = true, CreatedAt = new DateTime(2024, 1, 1) }
            );

            modelBuilder.Entity<Color>().HasData(
                new Color { Id = 1, Name = "Red", IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
                new Color { Id = 2, Name = "Blue", IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
                new Color { Id = 3, Name = "Green", IsActive = true, CreatedAt = new DateTime(2024, 1, 1) }
            );


            // ==========================================
            // 3. JUNCTION TABLES CONFIGURATION (Many-to-Many)
            // ==========================================

            // BookColor Composite Key (Zaroori hai!)
            modelBuilder.Entity<BookColor>()
                .HasKey(bc => new { bc.BookId, bc.ColorId });

            modelBuilder.Entity<BookColor>()
                .HasOne(bc => bc.Book)
                .WithMany(b => b.BookColors)
                .HasForeignKey(bc => bc.BookId)
                .OnDelete(DeleteBehavior.Cascade); // Book delete hone par BookColor bhi delete hoga

            modelBuilder.Entity<BookColor>()
                .HasOne(bc => bc.color)
                .WithMany(c => c.BookColors)
                .HasForeignKey(bc => bc.ColorId)
                .OnDelete(DeleteBehavior.Restrict); // SQL Server multiple cascade path error se bachne ke liye Restrict


            // BookLanguage Composite Key (Zaroori hai!)
            modelBuilder.Entity<BookLanguage>()
                .HasKey(bl => new { bl.BookId, bl.LanguageId });

            modelBuilder.Entity<BookLanguage>()
                .HasOne(bl => bl.Book)
                .WithMany(b => b.BookLanguages)
                .HasForeignKey(bl => bl.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BookLanguage>()
                .HasOne(bl => bl.Language)
                .WithMany(l => l.BookLanguages)
                .HasForeignKey(bl => bl.LanguageId)
                .OnDelete(DeleteBehavior.Restrict);
        }


        // ==========================================
        // 4. DB SETS (PascalCase Naming Convention)
        // ==========================================
        public DbSet<Book> Books { get; set; }
        public DbSet<Language> Languages { get; set; }
        public DbSet<Color> Colors { get; set; }       // 'color' se 'Color' kiya
        public DbSet<Currency> Currency { get; set; }
        public DbSet<BookColor> BookColors { get; set; }
        public DbSet<BookLanguage> BookLanguages { get; set; }
        public DbSet<BookPrice> BookPrices { get; set; } // Agar hai toh

        public DbSet <User> Users { get; set; } // Agar hai toh

        // ==========================================
        // 5. SAVE CHANGES OVERRIDES (IST Timezone Logic)
        // ==========================================
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            ApplyTimestamps();
            return base.SaveChanges();
        }

        private void ApplyTimestamps()
        {
            // India ka Time (IST = UTC + 5 hours 30 minutes)
            var istNow = DateTime.UtcNow.AddHours(5).AddMinutes(30);

            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.State == EntityState.Added)
                {
                    var createdAtProperty = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "CreatedAt");
                    if (createdAtProperty != null && (createdAtProperty.CurrentValue == null || (createdAtProperty.CurrentValue is DateTime dt && dt == default)))
                    {
                        createdAtProperty.CurrentValue = istNow;
                    }

                    var updatedAtProperty = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "UpdatedAt");
                    if (updatedAtProperty != null)
                    {
                        updatedAtProperty.CurrentValue = null;
                    }
                }
                else if (entry.State == EntityState.Modified)
                {
                    var updatedAtProperty = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "UpdatedAt");
                    if (updatedAtProperty != null)
                    {
                        updatedAtProperty.CurrentValue = istNow;
                    }

                    var createdAtProperty = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "CreatedAt");
                    if (createdAtProperty != null)
                    {
                        createdAtProperty.IsModified = false; // CreatedAt ko change hone se rokein
                    }
                }
            }
        }
    }
}