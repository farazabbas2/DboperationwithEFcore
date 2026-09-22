using DbOperationsWithEfcoreApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DbOperationsWithEfcoreApp.Controllers
{
    [Route("api/colors")]
    [ApiController]
    public class ColorController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;

        public ColorController(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllColors()
        {
            var result = await _appDbContext.Colors.ToListAsync();
            return Ok(result);
        }
    }
}
