using DbOperationsWithEfcoreApp.Models;

namespace DbOperationsWithEfcoreApp.Interfaces
{
    public interface IFriendRepository
    {
        Task AddAsync(Friendship friendship);
        Task UpdateAsync(Friendship friendship);
        Task DeleteAsync(Friendship friendship);
        Task<List<Friendship>> GetFriendsAsync(long userId);
        Task<List<Friendship>> GetPendingRequestsAsync(long userId);
        Task<Friendship?> GetFriendshipAsync(long userId, long friendId);
        Task<Friendship?> GetExistingFriendshipAsync(long userId, long friendId);
        Task<bool> UserExistsAsync(long userId);
    }
}
