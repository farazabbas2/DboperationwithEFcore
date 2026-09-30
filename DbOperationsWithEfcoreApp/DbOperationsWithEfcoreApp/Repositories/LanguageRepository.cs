using DbOperationsWithEfcoreApp.Data;
using DbOperationsWithEfcoreApp.Interfaces;
using DbOperationsWithEfcoreApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DbOperationsWithEfcoreApp.Repositories
{
    public class LanguageRepository:ILanguageRepository
    {

        private readonly AppDbContext _context;
        public LanguageRepository(AppDbContext context)
        {
            _context = context;
        }


        public async Task<List<Language>> GetAllLanguagesAsync()
        {
            return await  _context.Languages.ToListAsync();
        }

        public async Task<Language?> GetLanguageByIdAsync(int id)
        {
            return await _context.Languages.FirstOrDefaultAsync(l => l.Id == id);
        }
        public async Task<Language?> GetLanguageByNameAsync(
     string name,
     string? description)
        {
            string cleanName = name.Trim().ToLower();

            return await _context.Languages
                .FirstOrDefaultAsync(x =>
                    x.Name.ToLower() == cleanName &&
                    (string.IsNullOrEmpty(description) ||
                     x.Description == description));
        }

        public async Task<bool> IsLanguageNameDuplicateAsync(string name, int excludeId)
        {
            return await _context.Languages.AnyAsync(l => l.Name==name &&l.Id!= excludeId);
        }

        public async Task AddLanguageAsync(Language language)
        {
             await _context.AddAsync(language);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> LanguageExistsWithSameName(string name, int id)
        {
            return await _context.Languages
                .AnyAsync(l => l.Name == name && l.Id != id);
        }
        public async Task UpdateLanguageAsync(Language language)
        {
            _context.Languages.Update(language);

            await _context.SaveChangesAsync();
        }
        public async Task<List<Language>> GetLanguagesByIds(List<int> ids)
        {
            return await _context.Languages
                .Where(x => ids.Contains(x.Id))
                .ToListAsync();
        }
        public async Task<Language?> GetLanguageByIdIgnoreFiltersAsync(int id)
        {
            return await _context.Languages
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);
        }
        public async Task SoftDeleteLanguageAsync(int id)
        {
            var language = await _context.Languages
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (language != null)
            {
                language.IsActive = false;

                await _context.SaveChangesAsync();
            }
        }
    }
}
