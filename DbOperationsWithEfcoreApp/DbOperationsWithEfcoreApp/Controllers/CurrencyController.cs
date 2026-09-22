using DbOperationsWithEfcoreApp.Data;
using DbOperationsWithEfcoreApp.Dtos;
using DbOperationsWithEfcoreApp.Models;
using Color = DbOperationsWithEfcoreApp.Models.Color;
using FluentValidation;
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
        private readonly IValidator<CreateCurrencyDto> _createCurrencyValidator;

        public CurrencyController(AppDbContext appDbContext , IValidator<CreateCurrencyDto> createCurrencyValidator)
        {
            _appDbContext = appDbContext;
            _createCurrencyValidator = createCurrencyValidator;
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
            string cleanName = name.Trim().ToLower();
            var result = await _appDbContext.Currency.FirstOrDefaultAsync(x => x.Title.ToLower() == cleanName && (string.IsNullOrEmpty(description) || x.description == description));
            if (result == null)
            {
                return NotFound(new { message = $"Currency with title '{name}' not found." });
            }
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

            // IsActive == false (0) ka matlab Inactive/Deleted hai
            if (currency.IsActive == false) 
            {
                return Conflict(new { message = "Currency is already deleted." });
            }
        
            // Soft delete: IsActive ko false (0) karein
            currency.IsActive = false; 
            await _appDbContext.SaveChangesAsync();

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

            var validationResult = await _createCurrencyValidator.ValidateAsync(createDto);

            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Validation failed.",
                    errors = validationResult.Errors.Select(e => new
                    {
                        propertyName = e.PropertyName,
                        errorMessage = e.ErrorMessage
                    })
                });
            }
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

            // Naya record create hone par IsActive = true (1 = Active) hoga
            var currency = new Currency
            {
                Title = createDto.Title.Trim(),
                description = createDto.description.Trim(),
                IsActive = true
            };

            _appDbContext.Currency.Add(currency);
            await _appDbContext.SaveChangesAsync();

            return Ok(new
            {
                message = "Currency created successfully.",
                data = new
                {
                    currency.id,
                    currency.Title,
                    currency.description,
                    currency.IsActive
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

            // IsActive == false matlab Inactive/Deleted hai
            if (currency.IsActive == false)
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


    

