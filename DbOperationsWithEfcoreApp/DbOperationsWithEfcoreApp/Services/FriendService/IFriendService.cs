using DbOperationsWithEfcoreApp.Models;


namespace DbOperationsWithEfcoreApp.Services.FriendService
{
    public interface IFriendService
    {
        Task<(bool Success, string Message)> SendFriendRequest(long userId, long friendId);
        Task<(bool Success, string Message)> AcceptFriendRequest(long userId, long requesterId);
        Task<(bool Success, string Message)> RejectFriendRequest(long userId, long requesterId);
        Task<List<Friendship>> GetMyFriends(long userId);
        Task<List<Friendship>> GetPendingRequests(long userId);
    }
}