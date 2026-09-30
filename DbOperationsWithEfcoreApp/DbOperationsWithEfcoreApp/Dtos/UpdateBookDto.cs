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
        public List<UpdateBookPriceDto> Prices { get; set; }
    }

    public class UpdateBookPriceDto
    {
        public int? Id { get; set; }      // Agar purani price update karni ho (Optional)
        public decimal Amount { get; set; } // Note: Apne model me check karein ki 'Amount' hai ya 'amount'
        public int CurrencyId { get; set; }
    }
}
