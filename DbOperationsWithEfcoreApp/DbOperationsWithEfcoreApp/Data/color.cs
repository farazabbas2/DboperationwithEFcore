namespace DbOperationsWithEfcoreApp.Data
{
    public class color
    {
        public int id { get; set; }
        public string name { get; set; }    

        public ICollection<Book>Books { get; set; }
    }
}
