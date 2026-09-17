namespace DbOperationsWithEfcoreApp.Models
{
    public class Currency
    {
        public int id { get; set; }

        public string Title { get; set; }
   
        public string description { get; set; }

        public bool isDeleted { get; set; } = true;


        public ICollection<BookPrice>BookPrices { get; set; }
    }
}
