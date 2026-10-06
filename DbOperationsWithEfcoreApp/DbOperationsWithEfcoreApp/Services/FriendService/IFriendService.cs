using DbOperationsWithEfcoreApp.Models;


namespace DbOperationsWithEfcoreApp.Services.FriendService
{
    public interface IFriendService
    {
        Task<(bool Success, string Message)> SendFriendRequest(long userId, long friendId);
        Task<List<Friendship>> GetMyFriends(long userId);
    }
}