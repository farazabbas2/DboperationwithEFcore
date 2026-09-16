using DbOperationsWithEfcoreApp.Data;
using DbOperationsWithEfcoreApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DbOperationsWithEfcoreApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LanguageController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;

        public LanguageController(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }
        [HttpGet("")]
        public async Task<IActionResult> GetallLanguages()
        {
            var result=await _appDbContext.Languages.ToListAsync();
            return Ok(result);
        }

        [HttpGet("{id} int")]
        public async Task<IActionResult> GetLanguaugeById([FromRoute] int id)
        {
            var result = await _appDbContext.Languages.FindAsync(id);
            return Ok(result);
        }


        [HttpGet("{name}")]
        public async Task<IActionResult> GetLanguaugeByName([FromRoute] string name, [FromQuery] string? description)
        {
            var result = await _appDbContext.Languages
                .Where(x => x.Title == name &&
                           (string.IsNullOrEmpty(description) || x.Description == description))
                .ToListAsync();

            // ✅ Check karo ki list khali toh nahi hai
            if (result == null || !result.Any())
            {
                // 404 Not Found return karna
                return StatusCode(404, new
                {
                    success = false,
                    message = $"Language with name '{name}' not found."
                });
            }

            // 200 OK return karna
            return StatusCode(200, new
            {
                success = true,
                message = "Data fetched successfully",
                data = result
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveLanguage(int id)
        {
            var result = await _appDbContext.Languages.FindAsync(id);
            if(result==null)
            {
                return NotFound();
            }

            _appDbContext.Languages.Remove(result);
            _appDbContext.SaveChangesAsync();
            return Ok("Language deleted Successfully");
        }
    }
}
