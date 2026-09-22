using DbOperationsWithEfcoreApp.Models;

public class BookLanguage
{
   

    // Foreign Keys
    public int BookId { get; set; }
    public int LanguageId { get; set; }

    // Navigation Properties
    public Book Book { get; set; }
    public Language Language { get; set; }
}