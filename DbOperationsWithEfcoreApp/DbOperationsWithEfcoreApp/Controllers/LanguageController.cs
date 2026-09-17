using DbOperationsWithEfcoreApp.Data;
using DbOperationsWithEfcoreApp.Dtos;
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

        [HttpGet("{id:int} ")]
        public async Task<IActionResult> GetLanguaugeById([FromRoute] int id)
        {
            var result = await _appDbContext.Languages.FindAsync(id);

            if (result == null)
            {
                return NotFound("id not found");
            }

            return Ok(new { success = true, data = result });
        }


        [HttpGet("{name}")]
        public async Task<IActionResult> GetLanguaugeByName([FromRoute] string name, [FromQuery] string? description)
        {
            var result = await _appDbContext.Languages
                .Where(x => x.Title == name &&
                           (string.IsNullOrEmpty(description) || x.Description == description))
                .ToListAsync();

            //check exist or not 
            if (result == null || !result.Any())
            {
                // 404 Not Found 
                return StatusCode(404, new
                {
                    success = false,
                    message = $"Language with name '{name}' not found."
                });
            }

              //return 200 ok 
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
            var result = await _appDbContext.Languages
          .IgnoreQueryFilters()
          .FirstOrDefaultAsync(c => c.id == id);
            if (result==null)
            {
                return NotFound(new { message = "Language does not exist." });
            }
            if(result.isDeleted==false)
            {
                return Conflict(new { message = "Language is already deleted." });
            }

            //(use for hard delete)

            //_appDbContext.Languages.Remove(result); 
            result.isDeleted = false;
            await _appDbContext.SaveChangesAsync();
            return Ok("Language deleted Successfully");
        }

        [HttpPost("alllang")]
        public async Task<IActionResult> GetLanguageByIds([FromBody] LanguaeRequestDto request)
        {


            if (request == null || request.Ids == null ||
                request.Ids.Count == 0)
            {
                return BadRequest("please provide at least one ID");
            }

            var result = await _appDbContext.
                Languages.Where(x => request.Ids.Contains(x.id)).
                ToListAsync();
            return Ok(result);
        }
    }
}
