using DbOperationsWithEfcoreApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using DbOperationsWithEfcoreApp.Dtos;
using DbOperationsWithEfcoreApp.Models;
using Microsoft.Extensions.FileProviders;
using System.Data.SqlTypes;
using Microsoft.AspNetCore.Authorization;

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
        [Authorize]
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

        [Authorize]
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

        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
        [HttpPut("id")]
        public async Task<IActionResult> UpdateColor (int id, [FromBody] UpdateColorDto updateColor)
        {
            var Color = await _appDbContext.Colors.FirstOrDefaultAsync(c => c.Id == id);

            if(Color==null)
            {
                return NotFound(new { message = "color not found" });
            }

            if(Color.IsActive==false)
            {
                return BadRequest(new { message = "color is already deleted" });
            }
            var exisitngRecord = await _appDbContext.Colors.FirstOrDefaultAsync(c => c.Name == updateColor.name && c.Id != id);
            if(exisitngRecord!=null)
            {
                return Conflict(new {message= "color with this name is already exists" });
            }
            Color.Name = updateColor.name;
            await _appDbContext.SaveChangesAsync();
            return Ok(new
            {
                message = "color updated successfully",
                data = new { Color.Name }
            });
        }


    }
}
