using DbOperationsWithEfcoreApp.Data;
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
            //jo name denge whi return karega
            //  var result = await _appDbContext.Languages.FirstOrDefaultAsync(x => x.Title == name && (string.IsNullOrEmpty(description) || x.Description==description));

            //us name ke sare record /duplicate entry bhi return kar degaa
            var result = await _appDbContext.Languages.Where(x => x.Title == name && (string.IsNullOrEmpty(description) || x.Description == description)).ToListAsync();
            return Ok(result);
        }
    }
}
