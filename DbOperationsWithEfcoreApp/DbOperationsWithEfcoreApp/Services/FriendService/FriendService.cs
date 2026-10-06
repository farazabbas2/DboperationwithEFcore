using DbOperationsWithEfcoreApp.Interfaces;
using DbOperationsWithEfcoreApp.Models;
using DbOperationsWithEfcoreApp.Repositories;
using Microsoft.EntityFrameworkCore;
using DbOperationsWithEfcoreApp.Services;

namespace DbOperationsWithEfcoreApp.Services.FriendService
{
    public class FriendService : IFriendService
    {
        private readonly IFriendRepository _repo;
        public FriendService(IFriendRepository repo) { _repo = repo; }

        public async Task<(bool Success, string Message)> SendFriendRequest(long userId, long friendId)
        {
            if (userId == friendId)
            {
                return (false, "You cannot send a friend request to yourself.");
            }

            var friendExists = await _repo.UserExistsAsync(friendId);
            if (!friendExists)
            {
                return (false, "User ID does not exist.");
            }

            var userExists = await _repo.UserExistsAsync(userId);
            if (!userExists)
            {
                return (false, "Sender User ID does not exist.");
            }

            var alreadyConnected = await _repo.FriendshipExistsAsync(userId, friendId);
            if (alreadyConnected)
            {
                return (false, "You are already connected with this user.");
            }

            var friendship = new Friendship { UserId = userId, FriendId = friendId, status = 1, CreatedAt = DateTime.UtcNow };
            await _repo.AddAsync(friendship);
            return (true, "Friend request sent successfully!");
        }

        public async Task<List<Friendship>> GetMyFriends(long userId)
        {
            return await _repo.GetFriendsAsync(userId);
        }
    }
}