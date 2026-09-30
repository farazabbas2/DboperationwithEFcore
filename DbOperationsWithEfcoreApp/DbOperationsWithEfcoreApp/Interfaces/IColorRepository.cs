using DbOperationsWithEfcoreApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DbOperationsWithEfcoreApp.Interfaces
{
    public interface IColorRepository
    {
        Task<List<Color>> GetAllColorsAsync();
        Task<Color?> GetColorByIdAsync(int id);
        Task<bool> IsColorNameDuplicateAsync(string name, int excludeId);
        Task AddColorAsync(Color color);
        Task UpdateColorAsync(Color color);

        Task SoftDeleteColorAsync(int id);
    }
}