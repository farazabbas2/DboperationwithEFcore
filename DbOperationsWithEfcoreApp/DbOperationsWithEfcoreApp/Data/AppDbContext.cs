using DbOperationsWithEfcoreApp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace DbOperationsWithEfcoreApp.Data
{
    public class AppDbContext : DbContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public AppDbContext(DbContextOptions<AppDbContext> options, IHttpContextAccessor httpContextAccessor) : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            // User requirement: IsActive = 1 (true) => Active record, 0 (false) => Inactive/Deleted.
            // Query filter sirf ACTIVE records (IsActive == true) dikhayega.
            modelBuilder.Entity<Friendship>(entity =>
            {
                entity.HasKey(e => e.Id); // Primary Key

                // Performance ke liye Indexes (Fast searching ke liye)
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.FriendId);

                // Default Value
                entity.Property(e => e.status).HasDefaultValue(1);
            });
            modelBuilder.Entity<UserActivity>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.BookId);

                entity.Property(e => e.ActionType)
                    .HasMaxLength(50);

                entity.Property(e => e.Description)
                    .HasMaxLength(255);

              
            });

            modelBuilder.Entity<Currency>()
                .HasQueryFilter(c => c.IsActive);

            modelBuilder.Entity<Language>()
                .HasQueryFilter(l => l.IsActive);

            modelBuilder.Entity<Color>()
                .HasQueryFilter(c => c.IsActive);

            modelBuilder.Entity<Book>()
                .HasQueryFilter(b => b.IsActive);
                          //seed data
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


           
            // 3. JUNCTION TABLES CONFIGURATION (Many-to-Many)
            

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


            modelBuilder.Entity<Color>()
            .HasIndex(c => c.Name)
            .IsUnique(); 
        }


        // ==========================================
        // 4. DB SETS (PascalCase Naming Convention)
        // ==========================================
        public DbSet<Book> Books { get; set; }
        public DbSet<Language> Languages { get; set; }
        public DbSet<Color> Colors { get; set; }       
        public DbSet<Currency> Currency { get; set; }
        public DbSet<BookColor> BookColors { get; set; }
        public DbSet<BookLanguage> BookLanguages { get; set; }
        public DbSet<BookPrice> BookPrices { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<AuditLog> AuditLogs { get; set; }

        public DbSet<Friendship> Friendships { get; set; }

        public DbSet<UserActivity> UserActivities { get; set; }

        // ==========================================
        // 5. SAVE CHANGES OVERRIDES (IST Timezone Logic)
        // ==========================================
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyTimestamps();
            await ApplyAuditLogsAsync(); // 
            return await base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            ApplyTimestamps();
            ApplyAuditLogs();
            return base.SaveChanges();
        }

        private async Task ApplyAuditLogsAsync()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added ||
                            e.State == EntityState.Modified ||
                            e.State == EntityState.Deleted)
                .ToList();


            foreach (var entry in entries)
            {
                // Khud ki AuditLog table ko audit na karein (Infinite loop se bachne ke liye)
                if (entry.Entity is AuditLog) continue;

                var auditLog = new AuditLog
                {
                    TableName = entry.Entity.GetType().Name,
                    ChangedAt = DateTime.UtcNow.AddHours(5).AddMinutes(30), // IST Time
                    ChangedBy = _httpContextAccessor?.HttpContext?.User?.Identity?.Name ?? "System"
                };

                if (entry.State == EntityState.Added)
                {
                    auditLog.Action = "INSERT";
                    auditLog.NewValues = JsonSerializer.Serialize(entry.CurrentValues.ToObject());
                }
                else if (entry.State == EntityState.Modified)
                {
                    auditLog.Action = "UPDATE";
                    auditLog.OldValues = JsonSerializer.Serialize(entry.OriginalValues.ToObject());
                    auditLog.NewValues = JsonSerializer.Serialize(entry.CurrentValues.ToObject());
                }
                else if (entry.State == EntityState.Deleted)
                {
                    auditLog.Action = "DELETE";
                    auditLog.OldValues = JsonSerializer.Serialize(entry.OriginalValues.ToObject());
                }

                // Audit entry ko tracker me add kar do taaki wo save ho jaye
                AuditLogs.Add(auditLog);
            }
        }


        // Sync version (agar kahin sync SaveChanges use ho)
        private void ApplyAuditLogs()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added ||
                            e.State == EntityState.Modified ||
                            e.State == EntityState.Deleted)
                .ToList();

            foreach (var entry in entries)
            {
                if (entry.Entity is AuditLog) continue;

                var auditLog = new AuditLog
                {
                    TableName = entry.Entity.GetType().Name,
                    ChangedAt = DateTime.UtcNow.AddHours(5).AddMinutes(30),
                    ChangedBy = _httpContextAccessor?.HttpContext?.User?.Identity?.Name ?? "System"
                };

                if (entry.State == EntityState.Added)
                {
                    auditLog.Action = "INSERT";
                    auditLog.NewValues = JsonSerializer.Serialize(entry.CurrentValues.ToObject());
                }
                else if (entry.State == EntityState.Modified)
                {
                    auditLog.Action = "UPDATE";
                    auditLog.OldValues = JsonSerializer.Serialize(entry.OriginalValues.ToObject());
                    auditLog.NewValues = JsonSerializer.Serialize(entry.CurrentValues.ToObject());
                }
                else if (entry.State == EntityState.Deleted)
                {
                    auditLog.Action = "DELETE";
                    auditLog.OldValues = JsonSerializer.Serialize(entry.OriginalValues.ToObject());
                }

                AuditLogs.Add(auditLog);
            }
        }


        private void ApplyTimestamps()
        {
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
                        createdAtProperty.IsModified = false;
                    }
                }
            }
        }
    }
}




    