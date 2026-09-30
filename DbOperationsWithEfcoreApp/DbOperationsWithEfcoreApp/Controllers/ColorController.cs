using DbOperationsWithEfcoreApp.Data;
using DbOperationsWithEfcoreApp.Dtos;
using DbOperationsWithEfcoreApp.Interfaces;
using DbOperationsWithEfcoreApp.Models;
using DbOperationsWithEfcoreApp.Repositories;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using System.Data.SqlTypes;


namespace DbOperationsWithEfcoreApp.Controllers
{
    [Route("api/colors")]
    [ApiController]
    public class ColorController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;
        private readonly IColorRepository _colorRepository;
        private readonly IValidator<CreateColorDto> _createNewColorValidator;
        public ColorController(AppDbContext appDbContext, IValidator<CreateColorDto> createNewColorValidator, IColorRepository colorRepository)
        {
            _appDbContext = appDbContext;
            _createNewColorValidator = createNewColorValidator;
            _colorRepository = colorRepository;
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllColors()
        {
            var colors = await _colorRepository.GetAllColorsAsync();

            // 2. Controller ka kaam: Data ko shape karna (Projection)
            var result = colors.Select(c => new
            {
                c.Id,
                c.Name,
                c.IsActive
            }).ToList();

            return Ok(result);
        }

        [Authorize]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetColorById([FromRoute] int id)
        {
            var color = await _colorRepository.GetColorByIdAsync(id);

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

      
            await _colorRepository.AddColorAsync(color);

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
            var color = await _colorRepository.GetColorByIdAsync(id);

            if (color==null)
            {
                return NotFound(new { message = "color not found" });
            }

            if(color.IsActive==false)
            {
                return BadRequest(new { message = "color is already deleted" });
            }
            var exisitngRecord = await _appDbContext.Colors.FirstOrDefaultAsync(c => c.Name == updateColor.name && c.Id != id);
            if(exisitngRecord!=null)
            {
                return Conflict(new {message= "color with this name is already exists" });
            }
            color.Name = updateColor.name;
            await _colorRepository.UpdateColorAsync(color);
            return Ok(new
            {
                message = "color updated successfully",
                data = new { color.Name }
            });
        }


        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteColor(int id)
        {
            var existingRecord = await _colorRepository.GetColorByIdAsync(id);
            if (existingRecord == null)
                return NotFound(new { success = false, message = "Book id Not found" });

            if (existingRecord.IsActive == false)
                return Conflict(new { success = false, message = "Book is already deleted" });

            await _colorRepository.SoftDeleteColorAsync(id);

            return Ok(new { message = "Book Deleted Successfully" });
        }

    }
}
