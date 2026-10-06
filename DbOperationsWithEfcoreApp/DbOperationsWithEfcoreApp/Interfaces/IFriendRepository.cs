using DbOperationsWithEfcoreApp.Models;

namespace DbOperationsWithEfcoreApp.Interfaces
{
    public interface IFriendRepository
    {

        Task AddAsync(Friendship friendship);
        Task<List<Friendship>> GetFriendsAsync(long userId);
        Task<bool> UserExistsAsync(long userId);
        Task<bool> FriendshipExistsAsync(long userId, long friendId);
    }
}
