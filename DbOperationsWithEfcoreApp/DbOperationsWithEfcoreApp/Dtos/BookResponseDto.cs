namespace DbOperationsWithEfcoreApp.Dtos
{
    // Main Book Response DTO
    public class BookResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int NoOfPages { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; }
        public string? PdfFilePath { get; set; }

        // Nested Lists
        public List<BookLanguageDto> Languages { get; set; }
        public List<BookColorDto> Colors { get; set; }
        public List<BookPriceDto> Prices { get; set; }
    }

    // Language DTO
    public class BookLanguageDto
    {
        public int LanguageId { get; set; }
        public string LanguageName { get; set; }
    }

    // Color DTO
    public class BookColorDto
    {
        public int ColorId { get; set; }
        public string ColorName { get; set; }
    }

    // Price DTO
    public class BookPriceDto
    {
        public int PriceId { get; set; }
        public decimal Amount { get; set; } // amount ka type decimal ya double jo bhi aapke model mein hai
        public string CurrencyName { get; set; }
    }
}