namespace DbOperationsWithEfcoreApp.Models
{
    public class UserActivity
    {

        public int Id { get; set; }
        public long UserId { get; set; }
        public string ActionType { get; set; } // e.g., "RatedBook"
        public int BookId { get; set; }
        public string Description { get; set; } // e.g., "Gave 4 stars"
        public DateTime CreatedAt { get; set; }
    }
}
