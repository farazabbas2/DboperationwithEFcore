using System.ComponentModel.DataAnnotations;

public class CreateBookDto
{
    [Required]
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int NoOfPages { get; set; }

    // Multiple Languages ke liye
    public List<int> LanguageIds { get; set; } = new List<int>();

    // Multiple Colors ke liye
    public List<int> ColorIds { get; set; } = new List<int>();
    public List<CreateBookPriceDto> Prices { get; set; }
}

public class CreateBookPriceDto
{
    public decimal Amount { get; set; }  // Ya 'amount' (apne model ke hisaab se)
    public int CurrencyId { get; set; }
}