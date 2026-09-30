namespace DbOperationsWithEfcoreApp.Models
{
    public class AuditLog
    {
        public int Id { get; set; }
        public string TableName { get; set; }      // Kis table me change hua (e.g., "Books")
        public string Action { get; set; }         // "INSERT", "UPDATE", ya "DELETE"
        public string? OldValues { get; set; }     // Purana data (JSON format me)
        public string? NewValues { get; set; }     // Naya data (JSON format me)
        public string? ChangedBy { get; set; }     // Kaunse user ne kiya (User ID ya Email)
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow; // 
    }
}
