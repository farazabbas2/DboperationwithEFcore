using DbOperationsWithEfcoreApp.Models;
using System.Drawing;
using Color = DbOperationsWithEfcoreApp.Models.Color;

public class BookColor
{
    

    // Foreign Keys
    public int BookId { get; set; }
    public int ColorId { get; set; }

    // Navigation Properties
    public Book Book { get; set; }
    public Color color { get; set; }
}