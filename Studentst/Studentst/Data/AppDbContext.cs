using Microsoft.EntityFrameworkCore;
using Studentst.Models;

namespace Studentst.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 1. Base method sirf EK baar call karein (Sabse upar)
            base.OnModelCreating(modelBuilder);

            // 2. Department Seed Data
            modelBuilder.Entity<Department>().HasData(
                new Department { id = 1, name = "artificial intelligence", description = "all about ai" },
                new Department { id = 2, name = "Data anaylise", description = "ok" },
                new Department { id = 3, name = "soft", description = "test" }
            );

            // 3. Team Seed Data
            modelBuilder.Entity<Team>().HasData(
                new Team { id = 1, teamName = "alpha" },
                new Team { id = 2, teamName = "panther" },
                new Team { id = 3, teamName = "trojans" }
            );

            // 4. Course Seed Data
            modelBuilder.Entity<Course>().HasData(
                new Course { id = 1, Coursename = "BCA", duration = "3 years" },
                new Course { id = 2, Coursename = "Biotech", duration = "3 years" },
                new Course { id = 3, Coursename = "F&N", duration = "3 years" }
            );

            // 5. (Optional but Recommended) Relationship Explicitly Define Karein
            // Agar Student model mein DepartmentId hai, toh relationship aise set hota hai:
            modelBuilder.Entity<Student>()
                .HasOne(s => s.department)
                .WithMany(d => d.Student) // Aapne property ka naam 'Student' rakha hai
                .HasForeignKey(s => s.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict); // Department delete karne par Student delete nahi hoga
        }

        public DbSet<Student> Students { get; set; }
        public DbSet<Department> Department { get; set; }
        public DbSet<Team> Team { get; set; }
        public DbSet<Course> course { get; set; }
        public DbSet<User> Users { get; set; }
    }
}