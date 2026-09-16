using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Studentst.Data;
using Studentst.DTOs;
using Studentst.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Studentst.Controllers
{
    [Route("api/Department")]
    [ApiController]
    public class DepartmentController : ControllerBase
    {
       
        private readonly AppDbContext _appdbContext;
        private readonly IValidator<CreateDepartmentDto> _createValidator;
        private readonly IValidator<UpdateDepartmentDto> _updateValidator;


        public DepartmentController(
              AppDbContext appdbContext,
              IValidator<CreateDepartmentDto> createValidator,
            IValidator<UpdateDepartmentDto> updateValidator)
        {
            _appdbContext = appdbContext;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetallDepartment()

        {
            var result = await _appdbContext.Department.Include(d=>d.Student)
                .ToListAsync();
            return Ok(result);
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDepartmentById([FromRoute] int id)

        {
            var result = await _appdbContext.Department.FindAsync(id);
            if(result ==null)
            {
                return NotFound("Department id not found");
            }
            return Ok(result);
        }
        [Authorize(Roles = "Admin")]

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDepartment(int id, UpdateDepartmentDto department)
        {
            // 1. Validation Check
            var validationResult = await _updateValidator.ValidateAsync(department);

            if (!validationResult.IsValid)
            {
                return UnprocessableEntity(new
                {
                    data = validationResult.Errors,

                });
            }

            // 2. Check if Department exists
            var existingDepartment = await _appdbContext.Department
                .FirstOrDefaultAsync(x => x.id == id);

            if (existingDepartment == null)
            {
                // ✅ 404 Not Found sabse accurate hai jab resource hi na mile
                return NotFound(new
                {
                    success = false,
                    message = $"Department with ID {id} does not exist.",
                    data = new { requestedId = id }
                });
            }

            // 3. Duplicate Name Check (Case-Insensitive, excluding current department)
            var isDuplicate = await _appdbContext.Department
                .AnyAsync(x => x.name.ToLower() == department.name.ToLower() && x.id != id);

            if (isDuplicate)
            {
                return UnprocessableEntity(new
                {
                    data = new
                    {
                        isValid = false,
                        errors = new[]
                        {
                            new
                            {
                                propertyName = "name",
                                errorMessage = "A department with this name already exists."
                            }
                        }
                    }
                });
            }

            // 4. Update Department Properties
            existingDepartment.name = department.name;
            existingDepartment.description = department.description;

            // 5. Save Changes
            await _appdbContext.SaveChangesAsync();

            // 6. Success Response
            return Ok(new
            {
                success = true,
                message = "Department updated successfully",
                data = new
                {
                    id = existingDepartment.id,
                    name = existingDepartment.name,
                    description = existingDepartment.description
                }
            });
        }

        [Authorize(Roles = "Admin")]

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            var department=await _appdbContext.Department.FirstOrDefaultAsync(x => x.id == id);

            if(department != null){
                return NotFound();
            }

            _appdbContext.Department.Remove(department);
            await _appdbContext.SaveChangesAsync();
            return Ok();

        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
            public async Task<IActionResult> CreateDepartment(CreateDepartmentDto department)
            {
                // 1. Validation Check
                var validationResult = await _createValidator.ValidateAsync(department);

                if (!validationResult.IsValid)
                {
                return UnprocessableEntity(new
                {
                    data = validationResult.Errors,

                });
                }

                // 2. Duplicate Name Check (Case-Insensitive)
                var existingDepartment = await _appdbContext.Department
                    .FirstOrDefaultAsync(x => x.name.ToLower() == department.name.ToLower());

            if (existingDepartment != null)
            {
                return StatusCode(409, new
                {

                    success = false,
                    message = "Department Already exists",
                    data = new { /* ... */ }
                }
                    );

                }

                // 3. Create Department
                var newDepartment = new Department
                {
                    name = department.name,
                    description = department.description
                };

                // 4. Save Department
                await _appdbContext.Department.AddAsync(newDepartment);
                await _appdbContext.SaveChangesAsync();

                // 5. Success Response
                return Ok(new
                {
                    success = true,
                    message = "Department created successfully",
                    data = new
                    {
                        id = newDepartment.id,
                        name = newDepartment.name,
                        description = newDepartment.description
                    }
                });
            }
        }
    
}

