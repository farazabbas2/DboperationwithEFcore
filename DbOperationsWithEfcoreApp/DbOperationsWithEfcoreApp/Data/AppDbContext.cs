using Microsoft.EntityFrameworkCore;
namespace DbOperationsWithEfcoreApp.Data
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) :base(options)
        {
            
        }

    }
}
