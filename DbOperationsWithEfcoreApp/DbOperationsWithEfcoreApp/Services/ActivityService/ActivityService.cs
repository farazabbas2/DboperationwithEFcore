using DbOperationsWithEfcoreApp.Interfaces;
using DbOperationsWithEfcoreApp.Models;
using DbOperationsWithEfcoreApp.Services;
using DbOperationsWithEfcoreApp.Data;
using DbOperationsWithEfcoreApp.Repositories;


namespace DbOperationsWithEfcoreApp.Services.ActivityService
{
    public class ActivityService : IActivityService
    {
        private readonly IActivityRepository _activityRepo;
        private readonly IFriendRepository _friendRepo;

        public ActivityService(IActivityRepository activityRepo, IFriendRepository friendRepo)
        {
            _activityRepo = activityRepo;
            _friendRepo = friendRepo;
        }

        public async Task LogActivity(long userId, string actionType, int bookId, string description)
        {
            var activity = new UserActivity
            {
                UserId = userId,
                ActionType = actionType,
                BookId = bookId,
                Description = description,
                CreatedAt = DateTime.Now
            };
            await _activityRepo.AddAsync(activity);
        }

        public async Task<List<UserActivity>> GetFeedAsync(long userId)
        {
            var friends = await _friendRepo.GetFriendsAsync(userId);
            // Friends ki IDs + Current user ki ID
            var friendIds = friends.Select(f => f.UserId == userId ? f.FriendId : f.UserId).Distinct().ToList();
            if (!friendIds.Contains(userId))
            {
                friendIds.Add(userId);
            }

            return await _activityRepo.GetFeedAsync(friendIds);
        }
    }
}