namespace DbOperationsWithEfcoreApp.Models
{
    public class Language
    {
        public int Id { get; set; }              // ✅ Capital I
        public string Name { get; set; } = string.Empty;  // ✅ Title nahi, Name
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; 
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;  // ✅ Capital I

        public ICollection<BookLanguage> BookLanguages { get; set; } = new List<BookLanguage>();
    }
}