namespace DbOperationsWithEfcoreApp.Models
{
    public class Language
    {

        public int id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public bool isDeleted { get; set; } = true;

        public ICollection<Book> Books {  get; set; }


    }
}