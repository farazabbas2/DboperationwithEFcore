namespace DbOperationsWithEfcoreApp.Dtos
{
    public class LogActivityDto
    {
        
            public long UserId { get; set; }
            public string ActionType { get; set; }
            public int BookId { get; set; }
           public string Description { get; set; }
        
    }
}
