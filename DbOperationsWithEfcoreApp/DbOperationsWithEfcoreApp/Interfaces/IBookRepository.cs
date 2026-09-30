using DbOperationsWithEfcoreApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DbOperationsWithEfcoreApp.Interfaces
{
    public interface IBookRepository
    {
        // 1. Data Fetching Methods
        Task<List<Book>> GetAllBooksAsync();
        Task<Book?> GetBookByIdAsync(int id);
        Task<Book?> GetBookByIdIgnoreFiltersAsync(int id); // Update ke liye

        // 2. Validation Helpers (Controller ko clean rakhne ke liye)
        Task<bool> IsBookDuplicateAsync(string title, string description);
        Task<List<Language>> GetLanguagesByIdsAsync(List<int> ids);
        Task<List<Color>> GetColorsByIdsAsync(List<int> ids);

        // 3. Create / Update / Delete Methods
        Task AddBookAsync(Book book);
        Task AddBooksRangeAsync(List<Book> books);
        Task UpdateBookAsync(Book book);
        Task SoftDeleteBookAsync(int id);
        Task UpdateBookPdfPathAsync(int id, string filePath);
        Task ReplaceBookRelationsAsync(int bookId, List<int> languageIds, List<int> colorIds);
    }
}