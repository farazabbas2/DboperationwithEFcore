namespace DbOperationsWithEfcoreApp.Models
{
    public class Friendship
    {
            public int Id { get; set; }
            public long UserId { get; set; }
            public long FriendId { get; set; }
            public int status { get; set; }  // 0: Pending, 1: Accepted
        public DateTime CreatedAt { get; set; }
        }
}
