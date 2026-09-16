namespace DbOperationsWithEfcoreApp.Models
{
    public class BookPrice
    {

        public int Id { get; set; }
        public decimal amount { get; set; }

        public int BookId { get; set; }

        public Book Books { get; set; }

        public int CurrencyId { get; set; }
        public Currency Currency { get; set; }
    }
}
