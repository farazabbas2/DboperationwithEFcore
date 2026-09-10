using Microsoft.EntityFrameworkCore;
namespace DbOperationsWithEfcoreApp.Data
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) :base(options)
        {
            
        }

        public DbSet<Book> Books { get; set; }
        public DbSet<Language> Languages { get; set; }
        public DbSet<color> Color {  get; set; }

    }
}
