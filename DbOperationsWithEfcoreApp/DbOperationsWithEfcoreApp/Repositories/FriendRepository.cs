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

        public async Task<List<Friendship>> GetFriendsAsync(long userId)
        {
            return await _context.Friendships
                .Where(f => (f.UserId == userId || f.FriendId == userId) && f.status == 1)
                .ToListAsync();
        }

        public async Task<bool> UserExistsAsync(long userId)
        {
            return await _context.Users.AnyAsync(u => u.Id == userId);
        }

        public async Task<bool> FriendshipExistsAsync(long userId, long friendId)
        {
            return await _context.Friendships.AnyAsync(f =>
                (f.UserId == userId && f.FriendId == friendId) ||
                (f.UserId == friendId && f.FriendId == userId));
        }
    }
}
