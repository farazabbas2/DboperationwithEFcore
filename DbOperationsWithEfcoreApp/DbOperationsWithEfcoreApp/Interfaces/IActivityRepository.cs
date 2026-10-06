using DbOperationsWithEfcoreApp.Models;

namespace DbOperationsWithEfcoreApp.Interfaces
{
    public interface IActivityRepository
    {

        Task AddAsync(UserActivity activity);
        Task<List<UserActivity>> GetFeedAsync(List<long> friendIds);
    }
}
