using DbOperationsWithEfcoreApp.Data;
using DbOperationsWithEfcoreApp.Interfaces;
using DbOperationsWithEfcoreApp.Models;
using Microsoft.EntityFrameworkCore;

namespace DbOperationsWithEfcoreApp.Repositories
{
    public class ActivityRepository:IActivityRepository
    {
        private readonly AppDbContext _context;
        public ActivityRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(UserActivity activity)
        {
            await _context.UserActivities.AddAsync(activity);
            await _context.SaveChangesAsync();
        }
        public async Task<List<UserActivity>> GetFeedAsync(List<long> friendIds)
        {
            return await _context.UserActivities
                .Where(a => friendIds.Contains(a.UserId))
                .OrderByDescending(a => a.CreatedAt)
                .Take(50)
                .ToListAsync();
        }
    }
}
