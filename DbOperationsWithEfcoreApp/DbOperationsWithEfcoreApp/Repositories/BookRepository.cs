using DbOperationsWithEfcoreApp.Data;
using DbOperationsWithEfcoreApp.Dtos;
using DbOperationsWithEfcoreApp.Interfaces;
using DbOperationsWithEfcoreApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DbOperationsWithEfcoreApp.Repositories
{
    public class BookRepository : IBookRepository
    {
        private readonly AppDbContext _context;

        public BookRepository(AppDbContext context)
        {
            _context = context;
        }

        // 1. GetAllBooks (Aapka Eager Loading logic yahan shift ho gaya)
        public async Task<List<Book>> GetAllBooksAsync()
        {
            return await _context.Books
                .Include(b => b.BookLanguages).ThenInclude(bl => bl.Language)
                .Include(b => b.BookColors).ThenInclude(bc => bc.color)
                .Include(b => b.BookPrices).ThenInclude(bp => bp.Currency)
                .IgnoreQueryFilters()
                .ToListAsync();
        }
        public async Task AddBookPricesAsync(int bookId, List<CreateBookPriceDto> prices)
        {
            if (prices == null || !prices.Any())
                return;

            var bookPrices = prices.Select(p => new BookPrice
            {
                BookId = bookId,
                amount = p.Amount,  // Apne model ke hisaab se 'amount' ya 'Amount'
                CurrencyId = p.CurrencyId,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            await _context.BookPrices.AddRangeAsync(bookPrices);
            await _context.SaveChangesAsync();
        }

        // 2. GetBookById
        public async Task<Book?> GetBookByIdAsync(int id)
        {
            return await _context.Books
                .Include(b => b.BookLanguages).ThenInclude(bl => bl.Language)
                .Include(b => b.BookColors).ThenInclude(bc => bc.color)
                .Include(b => b.BookPrices).ThenInclude(bp => bp.Currency)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        // 3. GetBookByIdIgnoreFilters (Update ke liye)
        public async Task<Book?> GetBookByIdIgnoreFiltersAsync(int id)
        {
            return await _context.Books
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        // 4. Duplicate Check
        public async Task<bool> IsBookDuplicateAsync(string title, string description)
        {
            string cleanTitle = title.Trim().ToLower();
            string cleanDesc = description.Trim().ToLower();

            return await _context.Books
                .AnyAsync(x => x.Title.Trim().ToLower() == cleanTitle &&
                               x.Description.Trim().ToLower() == cleanDesc);
        }

        // 5. Validation Helpers
        public async Task<List<Language>> GetLanguagesByIdsAsync(List<int> ids)
        {
            return await _context.Languages.Where(x => ids.Contains(x.Id)).ToListAsync();
        }

        public async Task<List<Color>> GetColorsByIdsAsync(List<int> ids)
        {
            return await _context.Colors.Where(x => ids.Contains(x.Id)).ToListAsync();
        }

        // 6. Add Single Book
        public async Task AddBookAsync(Book book)
        {
            await _context.Books.AddAsync(book);
            await _context.SaveChangesAsync();
        }

        // 7. Add Bulk Books
        public async Task AddBooksRangeAsync(List<Book> books)
        {
            await _context.Books.AddRangeAsync(books);
            await _context.SaveChangesAsync();
        }

        // 8. Update Book
        public async Task UpdateBookAsync(Book book)
        {
            _context.Books.Update(book);
            await _context.SaveChangesAsync();
        }



        //patch book

        public async Task<bool> PatchBookAsync(int id, PatchBookDto dto)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
                return false;
            // Update only the fields that are not null in the DTO
            if (dto.Description != null)
                book.Description = dto.Description;
            // Add more fields here as needed
            await _context.SaveChangesAsync();
            return true;
        }

        // 9. Soft Delete (IsActive = false)
        public async Task SoftDeleteBookAsync(int id)
        {
            var book = await _context.Books.FirstOrDefaultAsync(x => x.Id == id);
            if (book != null)
            {
                book.IsActive = false;
                book.UpdatedAt = System.DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
        public async Task ReplaceBookRelationsAsync(int bookId, List<int> languageIds, List<int> colorIds)
        {
            // 1. Purane relations hatao
            var oldLanguages = await _context.BookLanguages.Where(x => x.BookId == bookId).ToListAsync();
            _context.BookLanguages.RemoveRange(oldLanguages);

            var oldColors = await _context.BookColors.Where(x => x.BookId == bookId).ToListAsync();
            _context.BookColors.RemoveRange(oldColors);

            // 2. Naye relations add karo
            var newBookLanguages = languageIds.Select(langId => new BookLanguage { BookId = bookId, LanguageId = langId }).ToList();
            await _context.BookLanguages.AddRangeAsync(newBookLanguages);

            var newBookColors = colorIds.Select(colorId => new BookColor { BookId = bookId, ColorId = colorId }).ToList();
            await _context.BookColors.AddRangeAsync(newBookColors);

            // 3. Save changes
            await _context.SaveChangesAsync();
        }
        public async Task UpdateBookPricesAsync(int bookId, List<UpdateBookPriceDto> newPrices)
        {
            // 1. Is book ki purani saari prices dhundho
            var oldPrices = await _context.BookPrices.Where(bp => bp.BookId == bookId).ToListAsync();

            // 2. Purani prices ko delete kar do
            if (oldPrices.Any())
            {
                _context.BookPrices.RemoveRange(oldPrices);
            }

            // 3. Agar nayi prices aayi hain, toh unko add kar do
            if (newPrices != null && newPrices.Any())
            {
                var pricesToAdd = newPrices.Select(p => new BookPrice
                {
                    BookId = bookId,
                    amount = p.Amount,       // Apne model ke hisaab se 'amount' ya 'Amount' likhein
                    CurrencyId = p.CurrencyId
                }).ToList();

                await _context.BookPrices.AddRangeAsync(pricesToAdd);
            }

            // Changes database me save karo
            await _context.SaveChangesAsync();
        }

        // 10. Update PDF Path
        public async Task UpdateBookPdfPathAsync(int id, string filePath)
        {
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                book.PdfFilePath = filePath;
                await _context.SaveChangesAsync();
            }
        }
    }
}