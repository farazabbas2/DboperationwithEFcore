using DbOperationsWithEfcoreApp.Data;
using DbOperationsWithEfcoreApp.Interfaces;
using DbOperationsWithEfcoreApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DbOperationsWithEfcoreApp.Repositories
{
    public class ColorRepository : IColorRepository
    {
        private readonly AppDbContext _context;

        public ColorRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Color>> GetAllColorsAsync()
        {
            return await _context.Colors.ToListAsync();
        }

        public async Task<Color?> GetColorByIdAsync(int id)
        {
            return await _context.Colors.FirstOrDefaultAsync(c => c.Id == id);
        }

        // Yeh method controller ko clean rakhne ke liye hai (Duplicate check)
        public async Task<bool> IsColorNameDuplicateAsync(string name, int excludeId)
        {
            return await _context.Colors.AnyAsync(c => c.Name == name && c.Id != excludeId);
        }

        public async Task AddColorAsync(Color color)
        {
            await _context.Colors.AddAsync(color);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateColorAsync(Color color)
        {
            _context.Colors.Update(color);
            await _context.SaveChangesAsync();
        }

        public async Task SoftDeleteColorAsync(int id)
        {
            var color = await _context.Colors
              .IgnoreQueryFilters()
              .FirstOrDefaultAsync(x => x.Id == id);
            if (color != null)
            {
                color.IsActive = false;

                await _context.SaveChangesAsync();
            } 
        }
    }
}