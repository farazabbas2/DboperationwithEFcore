using DbOperationsWithEfcoreApp.Data;
using DbOperationsWithEfcoreApp.Dtos;
using DbOperationsWithEfcoreApp.Interfaces;
using DbOperationsWithEfcoreApp.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using DbOperationsWithEfcoreApp.Repositories;

namespace DbOperationsWithEfcoreApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LanguageController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;
        private readonly ILanguageRepository _languageRepository;
        private readonly IValidator<CreateLanguageDto> _createLanguageValidator;
        private readonly IValidator<UpdateLanguageDto> _updateLanguageValidator;

        public LanguageController(AppDbContext appDbContext, IValidator<CreateLanguageDto> createLanguageValidator, IValidator<UpdateLanguageDto> updateLanguageValidator,ILanguageRepository languageRepository)
        {
            _appDbContext = appDbContext;
            _createLanguageValidator = createLanguageValidator;
            _updateLanguageValidator = updateLanguageValidator;
            _languageRepository = languageRepository;
        }
        [Authorize]
        [HttpGet("")]
        public async Task<IActionResult> GetallLanguages()
        {
            var result = await _languageRepository.GetAllLanguagesAsync();
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetLanguaugeById([FromRoute] int id)
        {
            var result = await _languageRepository.GetLanguageByIdAsync(id);

            if (result == null)
            {
                return NotFound(new { message = "Language not found." });
            }

            return Ok(result);
        }

        [Authorize]
        [HttpGet("{name}")]
        public async Task<IActionResult> GetLanguaugeByName([FromRoute] string name, [FromQuery] string? description)
        {
            string cleanName = name.Trim().ToLower();
            var result = await _languageRepository.GetLanguageByNameAsync(name, description);
                


            //check exist or not 
            if (result == null)
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
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveLanguage(int id)
        {
            var result = await _languageRepository.GetLanguageByIdAsync(id);

            if (result == null)
            {
                return NotFound(new { message = "Language does not exist." });
            }

            // IsActive == false (0) ka matlab Inactive/Deleted hai
            if (result.IsActive == false)
            {
                return Conflict(new { message = "Language is already deleted." });
            }

            await _languageRepository.SoftDeleteLanguageAsync(id);
            return Ok(new { message = "Language deleted Successfully" });
        }
        [Authorize(Roles = "Admin")]
        [HttpPost("alllang")]
        public async Task<IActionResult> GetLanguageByIds([FromBody] LanguaeRequestDto request)
        {


            if (request == null || request.Ids == null ||
                request.Ids.Count == 0)
            {
                return BadRequest("please provide at least one ID");
            }


            var result = await _languageRepository
                .GetLanguagesByIds(request.Ids);
            return Ok(result);
        }

        //
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Updatelanguage(int id, [FromBody] UpdateLanguageDto languageDto)
        {

            var validationResult = await _updateLanguageValidator.ValidateAsync(languageDto);
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

            var language = await _languageRepository.GetLanguageByIdIgnoreFiltersAsync(id);

            if(language==null)
            {
                return NotFound(new { message = "Language does not exist." });
            }

            // IsActive == false (0) ka matlab Inactive/Deleted hai
            if (language.IsActive == false)
            {
                return Conflict(new { message = "Language is already deleted." });
            }

            var exisitngrecord = await _appDbContext.Languages.FirstOrDefaultAsync(c => c.Name == languageDto.Title && c.Id != id);
            if(exisitngrecord != null)
            {
                return Conflict(new { message = "Language with the same title already exists." });
            }

            language.Name = languageDto.Title;
            language.Description = languageDto.Description;
            await _languageRepository.UpdateLanguageAsync(language);

             return Ok(new { message = "Language updated successfully." });

        }
        //post method 
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateLanguage([FromBody] CreateLanguageDto languageDto)
        {

            var validationResult = await _createLanguageValidator.ValidateAsync(languageDto);
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
            string cleanTitle = languageDto.Title.Trim().ToLower();
            var existingRecord = await _languageRepository.GetLanguageByNameAsync(languageDto.Title, languageDto.Description);

            if (existingRecord != null)
            {
                return Conflict(new { message = "Language with the same title already exists." });
            }

            // Naya record create hone par IsActive = true (1 = Active) hoga
            var language = new Language
            {
                Name = languageDto.Title,
                Description = languageDto.Description,
                IsActive = true
            };
            await _languageRepository.AddLanguageAsync(language);
   
            return Ok(new { message = "Language created successfully.", data = language });

        }

    }
   
}
