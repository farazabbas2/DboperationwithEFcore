using DbOperationsWithEfcoreApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DbOperationsWithEfcoreApp.Interfaces
{
public interface ILanguageRepository
    {
        Task<List<Language>> GetAllLanguagesAsync();
        Task<Language?> GetLanguageByIdAsync(int id);
        Task<Language?> GetLanguageByIdIgnoreFiltersAsync(int id);
        Task<Language?> GetLanguageByNameAsync(string name, string? description);
        Task<bool> IsLanguageNameDuplicateAsync(string name, int excludeId);
        Task AddLanguageAsync(Language language);
        Task UpdateLanguageAsync(Language language);
        Task SoftDeleteLanguageAsync(int id);
        Task<List<Language>> GetLanguagesByIds(List<int> ids);
        Task<bool> LanguageExistsWithSameName(string name, int id);
    }
}