namespace DbOperationsWithEfcoreApp.Models
{
    public class Currency
    {
        public int id { get; set; }

        public string Title { get; set; }
   
        public string description { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; } 


  
    }
}
