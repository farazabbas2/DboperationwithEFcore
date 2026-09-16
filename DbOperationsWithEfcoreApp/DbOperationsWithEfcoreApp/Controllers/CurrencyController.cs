using DbOperationsWithEfcoreApp.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DbOperationsWithEfcoreApp.Controllers
{
    [Route("api/currencies")]
    [ApiController]
    public class CurrencyController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;

        public CurrencyController(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }
        [HttpGet()]
       // public IActionResult GetAllCurrrencies()
       public async Task<IActionResult> GetAllCurrencies()
        {
            // var result = _appDbContext.Currency.ToList();
            // var result = (from Currency in _appDbContext.Currency
            //select Currency).ToList();

             var result = await _appDbContext.Currency.ToListAsync();
            // var result = (from Currency in _appDbContext.Currency
            //select Currency).ToList();
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        // public IActionResult GetAllCurrrencies()
        public async Task<IActionResult> GetCurrencyByidAsync([FromRoute] int id)
        {
           
            var result = await _appDbContext.Currency.FindAsync(id);
          
            return Ok(result);
        }

        [HttpGet("{name}")]
        public async Task<IActionResult> GetCurrrencyByName([FromRoute] string name, [FromQuery] string? description)
        {
            var result = await _appDbContext.Currency.FirstOrDefaultAsync(x => x.Title == name && (string.IsNullOrEmpty(description) || x.description == description));
            return Ok(result);
        }
    }
}
