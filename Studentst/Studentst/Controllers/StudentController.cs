using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Studentst.Models;
using Studentst.Data;
using Studentst.DTOs;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;

namespace Studentst.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IValidator<StudentDto> _studentValidator;
        private readonly IValidator<UpdateStudentDto> _updateStudentValidator;


      



        public StudentController(
            AppDbContext context,
            IValidator<StudentDto> studentValidator,
            IValidator<UpdateStudentDto> updateStudentValidator)
        {
            _context = context;
            _studentValidator = studentValidator;
            _updateStudentValidator = updateStudentValidator;
          
        }
        [HttpPost]
        public async Task<IActionResult> AddStudent(StudentDto student)
        {
            // 1. Validation Check
            var validationResult = await _studentValidator.ValidateAsync(student);

            if (!validationResult.IsValid)
            {
                return UnprocessableEntity(new
                {
                    data = validationResult.Errors,
                });
            }

            // 2. Duplicate PhoneNo Check
            var existingRecord = await _context.Students
                .FirstOrDefaultAsync(x => x.PhoneNo == student.PhoneNo);
            if (existingRecord != null)
            {
                return StatusCode(409, new // 409 yahan manually set kiya hai
                {
                    success = false,
                    message = "User already exists",
                    data = new { /* ... */ }
                });
            
        }

            // 3. Department Validation
            var department = await _context.Department
                .FirstOrDefaultAsync(x => x.id == student.DepartmentId);

            if (department == null)
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
                        propertyName = "DepartmentId",
                        errorMessage = $"Department with ID {student.DepartmentId} does not exist."
                    }
                }
                    }
                });
            }

            // 4. Course Validation
            var course = await _context.course
                .FirstOrDefaultAsync(x => x.id == student.CourseId);

            if (course == null)
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
                        propertyName = "CourseId",
                        errorMessage = $"Course with ID {student.CourseId} does not exist."
                    }
                }
                    }
                });
            }

            // 5. Team Validation
            if (student.TeamId > 0)
            {
                var team = await _context.Team
                    .FirstOrDefaultAsync(x => x.id == student.TeamId);

                if (team == null)
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
                            propertyName = "TeamId",
                            errorMessage = $"Team with ID {student.TeamId} does not exist."
                        }
                    }
                        }
                    });
                }
            }

            // 6. Create Student
            var newStudent = new Student
            {
                Name = student.Name,
                age = student.Age,
                PhoneNo = student.PhoneNo,
                DepartmentId = student.DepartmentId,
                teamId = student.TeamId,
                CourseId = student.CourseId
            };

            // 7. Save Student
            await _context.Students.AddAsync(newStudent);
            await _context.SaveChangesAsync();

            // 8. Success Response
            return Ok(new
            {
                success = true,
                message = "Student created successfully",
                data = new
                {
                    id = newStudent.id,
                    name = newStudent.Name,
                    age = newStudent.age,
                    phoneNo = newStudent.PhoneNo,
                    departmentId = newStudent.DepartmentId,
                    teamId = newStudent.teamId,
                    courseId = newStudent.CourseId
                }
            });
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllStudents()
        {
            // Sirf wahi students lao jinka IsDeleted false hai
          

            var students = await _context.Students
               
                .Where(x => !x.isDeleted)
                .ToListAsync();

            return Ok(students);
        }

        [Authorize]
        [HttpGet("{id}")]

        public async Task<IActionResult> GetStudentById([FromRoute] int id)
        {
            var result = await _context.Students.FindAsync(id);

            if (result == null)
            {
                return NotFound("Student Not Found");
            }
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStudent(
            int id,
            UpdateStudentDto student)
        {
            // 1. Validation Check
            var validationResult = await _updateStudentValidator.ValidateAsync(student);

            if (!validationResult.IsValid)
            {
                return UnprocessableEntity(new
                {
                    data = new
                    {
                        isValid = false,
                        errors = validationResult.Errors
                            .Select(e => new
                            {
                                propertyName = e.PropertyName,
                                errorMessage = e.ErrorMessage,
                                attemptedValue = e.AttemptedValue,
                                customState = e.CustomState,
                                severity = e.Severity,
                                errorCode = e.ErrorCode,
                                formattedMessagePlaceholderValues =
                                    e.FormattedMessagePlaceholderValues
                            })
                            .ToList()
                    }
                });
            }

            // 2. Check if Student exists
           
            var existingStudent = await _context.Students
                .FirstOrDefaultAsync(x => x.id == id && !x.isDeleted); // !x.isDeleted add kar dein

            if (existingStudent == null)
            {
              return StatusCode(404, new // 409 yahan manually set kiya hai
                {
                    success = false,
                    message = "Student Not Found",
                    data = new 
                    {
                    requestedId = id // Frontend ko batane ke liye ki kaunsa ID nahi mila
        }
            });
            }

            // 3. Duplicate PhoneNo Check
            // Current student's own phone number is excluded
            var duplicatePhone = await _context.Students
                .FirstOrDefaultAsync(x =>
                    x.PhoneNo == student.PhoneNo &&
                    x.id != id);

            if (duplicatePhone != null)
            {
                return StatusCode(409, new // 409 yahan manually set kiya hai
                {
                    success = false,
                    message = "student with this  phone no  already exists",
                    data = new { /* ... */ }
                });
            }

            // 4. Department Validation
            var department = await _context.Department
                .FirstOrDefaultAsync(x => x.id == student.DepartmentId);

            if (department == null)
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
                        propertyName = "DepartmentId",
                        errorMessage =
                            $"Department with ID {student.DepartmentId} does not exist."
                    }
                }
                    }
                });
            }

            // 5. Course Validation
            var course = await _context.course
                .FirstOrDefaultAsync(x => x.id == student.CourseId);

            if (course == null)
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
                        propertyName = "CourseId",
                        errorMessage =
                            $"Course with ID {student.CourseId} does not exist."
                    }
                }
                    }
                });
            }

            // 6. Team Validation
            if (student.teamId > 0)
            {
                var team = await _context.Team
                    .FirstOrDefaultAsync(x => x.id == student.teamId);
                 

                if (team == null)
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
                            propertyName = "TeamId",
                            errorMessage =
                                $"Team with ID {student.teamId} does not exist."
                        }
                    }
                        }
                    });
                }
            }

            // 7. Update Student
            existingStudent.Name = student.Name;
            existingStudent.age = student.age;
            existingStudent.PhoneNo = student.PhoneNo;
            existingStudent.DepartmentId = student.DepartmentId;
            existingStudent.teamId = student.teamId;
            existingStudent.CourseId = student.CourseId;

            // 8. Save Changes
            await _context.SaveChangesAsync();

            // 9. Success Response
            return Ok(new
            {
                success = true,
                message = "Student updated successfully",
                data = new
                {
                    id = existingStudent.id,
                    name = existingStudent.Name,
                    age = existingStudent.age,
                    phoneNo = existingStudent.PhoneNo,
                    departmentId = existingStudent.DepartmentId,
                    teamId = existingStudent.teamId,
                    courseId = existingStudent.CourseId
                }
            });
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveStudent(int id)
        {
            // check student exist or not
            var student = await _context.Students
                .FirstOrDefaultAsync(x => x.id == id && !x.isDeleted);

            if (student == null)
            {
                return NotFound(new { message = "Student not found or already deleted." });
            }

            // using is deleted instead of remove 
            student.isDeleted = true;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Student soft deleted successfully" });
        }

    }


}
