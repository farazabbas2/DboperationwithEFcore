using DbOperationsWithEfcoreApp.Data;
using DbOperationsWithEfcoreApp.Interfaces;
using DbOperationsWithEfcoreApp.Models;
using Microsoft.EntityFrameworkCore;

namespace DbOperationsWithEfcoreApp.Repositories
{
    public class FriendRepository:IFriendRepository
    {
        private readonly AppDbContext _context;
        public FriendRepository(AppDbContext context)
        {
            _context = context;
        }


        public async Task AddAsync(Friendship friendship)
        {
            await _context.Friendships.AddAsync(friendship);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Friendship friendship)
        {
            _context.Friendships.Update(friendship);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Friendship friendship)
        {
            _context.Friendships.Remove(friendship);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Friendship>> GetFriendsAsync(long userId)
        {
            return await _context.Friendships
                .Where(f => (f.UserId == userId || f.FriendId == userId) && f.status == 1)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Friendship>> GetPendingRequestsAsync(long userId)
        {
            return await _context.Friendships
                .Where(f => f.FriendId == userId && f.status == 0)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }

        public async Task<Friendship?> GetFriendshipAsync(long userId, long friendId)
        {
            return await _context.Friendships
                .FirstOrDefaultAsync(f =>
                    (f.UserId == userId && f.FriendId == friendId) ||
                    (f.UserId == friendId && f.FriendId == userId));
        }

        public async Task<Friendship?> GetExistingFriendshipAsync(long userId, long friendId)
        {
            return await _context.Friendships
                .FirstOrDefaultAsync(f =>
                    (f.UserId == userId && f.FriendId == friendId) ||
                    (f.UserId == friendId && f.FriendId == userId));
        }

        public async Task<bool> UserExistsAsync(long userId)
        {
            return await _context.Users.AnyAsync(u => u.Id == userId);
        }
    }
}
