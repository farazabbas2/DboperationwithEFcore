using Microsoft.Identity.Client;

namespace DbOperationsWithEfcoreApp.Models
{
    public class Book
    {
        public int id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int NoOfPages { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }

        public int LanguageId { get; set; }
        public int ColorId { get; set; }

    
         
        public Language Language { get; set; }

       
        public color Color { get; set; }

        public ICollection<BookPrice>BookPrices {  get; set; }
    }
}
