namespace DbOperationsWithEfcoreApp.Dtos
{
    public class UpdateBookDto
    {
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int NoOfPages { get; set; }

        // Multiple Languages ke liye
        public List<int> LanguageIds { get; set; } = new List<int>();

        // Multiple Colors ke liye
        public List<int> ColorIds { get; set; } = new List<int>();
    }
}
