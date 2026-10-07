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

            var existing = await _repo.GetExistingFriendshipAsync(userId, friendId);
            if (existing != null)
            {
                if (existing.status == 1)
                {
                    return (false, "You are already connected with this user.");
                }
                if (existing.status == 0)
                {
                    if (existing.UserId == userId)
                    {
                        return (false, "Friend request has already been sent and is pending approval.");
                    }
                    else
                    {
                        // The other user already sent a request, so accept it!
                        existing.status = 1;
                        await _repo.UpdateAsync(existing);
                        return (true, "Friend request accepted! You are now connected.");
                    }
                }
            }

            // Status = 0 (Pending until recipient accepts)
            var friendship = new Friendship 
            { 
                UserId = userId, 
                FriendId = friendId, 
                status = 0, 
                CreatedAt = DateTime.UtcNow 
            };
            await _repo.AddAsync(friendship);
            return (true, "Friend request sent successfully! Awaiting recipient's acceptance.");
        }

        public async Task<(bool Success, string Message)> AcceptFriendRequest(long userId, long requesterId)
        {
            var friendship = await _repo.GetFriendshipAsync(requesterId, userId);
            if (friendship == null)
            {
                return (false, "Friend request not found.");
            }

            if (friendship.status == 1)
            {
                return (false, "You are already friends with this user.");
            }

            friendship.status = 1; // 1 = Accepted
            await _repo.UpdateAsync(friendship);
            return (true, "Friend request accepted! Added to your friends list.");
        }

        public async Task<(bool Success, string Message)> RejectFriendRequest(long userId, long requesterId)
        {
            var friendship = await _repo.GetFriendshipAsync(requesterId, userId);
            if (friendship == null)
            {
                return (false, "Friend request not found.");
            }

            await _repo.DeleteAsync(friendship);
            return (true, "Friend request declined.");
        }

        public async Task<List<Friendship>> GetMyFriends(long userId)
        {
            return await _repo.GetFriendsAsync(userId);
        }

        public async Task<List<Friendship>> GetPendingRequests(long userId)
        {
            return await _repo.GetPendingRequestsAsync(userId);
        }
    }
}