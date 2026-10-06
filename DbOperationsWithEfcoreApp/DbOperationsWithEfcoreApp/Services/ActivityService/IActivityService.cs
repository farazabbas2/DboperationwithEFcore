using DbOperationsWithEfcoreApp.Models;


namespace DbOperationsWithEfcoreApp.Services.ActivityService
{
    public interface IActivityService
    {
        Task LogActivity(long userId, string actionType, int bookId, string description);
        Task<List<UserActivity>> GetFeedAsync(long userId);
    }
}