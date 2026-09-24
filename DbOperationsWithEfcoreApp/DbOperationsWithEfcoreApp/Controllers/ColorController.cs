using DbOperationsWithEfcoreApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using DbOperationsWithEfcoreApp.Dtos;
using DbOperationsWithEfcoreApp.Models;

namespace DbOperationsWithEfcoreApp.Controllers
{
    [Route("api/colors")]
    [ApiController]
    public class ColorController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;
        private readonly IValidator<CreateColorDto> _createNewColorValidator;
        public ColorController(AppDbContext appDbContext, IValidator<CreateColorDto> createNewColorValidator)
        {
            _appDbContext = appDbContext;
            _createNewColorValidator = createNewColorValidator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllColors()
        {
            var result = await _appDbContext.Colors
                .Select(c => new
            {
                c.Id,
                c.Name,
                c.IsActive
            }).ToListAsync();
            return Ok(result);
        }
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetColorById([FromRoute] int id)
        {
            var color = await _appDbContext.Colors.FirstOrDefaultAsync(c => c.Id == id);

            if (color  == null)
            {
                return NotFound("ID not found.");
            }

            if (color.IsActive == false)
            {
                return NotFound("This color has been deleted.");
            }

            return Ok(color);
        }
        [HttpPost]
        public async Task<IActionResult> AddColor([FromBody] CreateColorDto createDto)
        {
            // 1. Validation check
            var validationResult = await _createNewColorValidator.ValidateAsync(createDto);
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

            // 2. Entity banana (CreateColorDto se Color entity)
            var color = new Color
            {
                Name = createDto.Name,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            // 3. Database mein save karna
            await _appDbContext.Colors.AddAsync(color);
            await _appDbContext.SaveChangesAsync();

            // 4. Response DTO banana (ColorResponseDto)
            var responseDto = new ColorResponseDto
            {
                id = color.Id,
                Name = color.Name,
               
            };

            // 5. Return karna ✅ (Ye missing tha!)
            return Ok(new
            {
                success = true,
                message = "Color added successfully.",
                data = responseDto
            });
        }


    }
}
