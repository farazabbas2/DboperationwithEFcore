namespace DbOperationsWithEfcoreApp.Data
{
    public class Language
    {

        public int id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public ICollection<Book> Books {  get; set; }

    }
}