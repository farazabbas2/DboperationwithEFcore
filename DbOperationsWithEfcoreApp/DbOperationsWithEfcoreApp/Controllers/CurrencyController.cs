using DbOperationsWithEfcoreApp.Data;
using DbOperationsWithEfcoreApp.Dtos;
using DbOperationsWithEfcoreApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
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

             var result = await _appDbContext.Currency
                //use when select only specific coloumns 
               // Select(x=> new{  CurrencyId = x.id ,Name = x.Title, }).
               .ToListAsync();
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





        // delete data based on id
        // delete data based on id (Soft Delete)
        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveCurrency(int id)
        {

            var currency = await _appDbContext.Currency
          .IgnoreQueryFilters()
          .FirstOrDefaultAsync(c => c.id == id);
            if (currency == null)
            {
                return NotFound(new { message = "Currency does not exist." });
            }

       
            if (currency.isDeleted == false) 
            {
                return Conflict(new { message = "Currency is already deleted." });
            }
        
            currency.isDeleted = false; 
            await _appDbContext.SaveChangesAsync();

            // 6. Success message return karein
            return Ok(new { message = "Currency deleted successfully." });
        }





        //if we want to get the multiple records based on ids 
        [HttpPost("all")]
        public async Task<IActionResult> GetCurrencyByIdasync([FromBody] CurrencyRequestDto request)
        {


            if(request==null || request.Ids==null ||
                request.Ids.Count == 0)
            {
                return BadRequest("please provide at least one ID");
            }

            var result = await _appDbContext.
                Currency.Where(x => request.Ids.Contains(x.id)).
                //select specific coloumns by creating the object
                Select(x => new Currency()
                {
                    id = x.id,
                    Title=x.Title
                })
                .ToListAsync();
            return Ok(result);
        }
        // POST: api/currencies
        [HttpPost]
        public async Task<IActionResult> CreateCurrency([FromBody] CreateCurrencyDto createDto)
        {
            // ✅ FIX: Trim() aur ToLower() use kiya taaki spaces aur case sensitivity ki wajah se duplicate na bane
            string cleanTitle = createDto.Title.Trim().ToLower();
            string cleanDesc = createDto.description.Trim().ToLower();

            var existingResult = await _appDbContext.Currency
                .FirstOrDefaultAsync(x =>
                    x.Title.Trim().ToLower() == cleanTitle &&
                    x.description.Trim().ToLower() == cleanDesc);

            if (existingResult != null)
            {
                return Conflict(new
                {
                    message = "Currency with this Title and description already exists"
                });
            }

            var currency = new Currency
            {
                Title = createDto.Title.Trim(),
                description = createDto.description.Trim(),
                isDeleted = true
            };

            _appDbContext.Currency.Add(currency);
            await _appDbContext.SaveChangesAsync();

            return Ok(new
            {
                message = "Currency created successfully.",
                data = new
                {
                    currency.Title,
                    currency.description
                }
            });
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCurrency( int id,[FromBody] UpdateCurrencyDto updateDto)
        {

            var currency = await _appDbContext.Currency
       .IgnoreQueryFilters()
       .FirstOrDefaultAsync(c => c.id == id);



            if (currency == null)
            {
                return NotFound(new { message = "Currency not found." });
            }

            if (currency.isDeleted==false)
            {
                return BadRequest(new { message = "Currency is already deleted and cannot be updated." });
            }


            var existingRecord = await _appDbContext.Currency
           .FirstOrDefaultAsync(x => x.Title == updateDto.Title && x.id != id);

            // 3. Agar existingRecord null nahi hai, iska matlab duplicate mil gaya
            if (existingRecord != null)
            {
                return Conflict(new { message = "Currency title already exists. Please choose a different title." });
            }


            currency.Title = updateDto.Title;
            currency.description = updateDto.description; // Agar aap description bhi update karna chahte hain



            await _appDbContext.SaveChangesAsync();

            // 6. Success response return karein
            return Ok(new
            {
                message = "Currency updated successfully.",
                data = new { currency.id, currency.Title, currency.description }
            });
        }
      
            }
        }


    

