namespace DbOperationsWithEfcoreApp.Models
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public int NoOfPages { get; set; }
        public bool IsActive { get; set; } = true;

        // Many-to-Many: Languages
        public ICollection<BookLanguage> BookLanguages { get; set; } = new List<BookLanguage>();

        // Many-to-Many: Colors
        public ICollection<BookColor> BookColors { get; set; } = new List<BookColor>();

        // Other collections
        public ICollection<BookPrice> BookPrices { get; set; } = new List<BookPrice>();
    }
}