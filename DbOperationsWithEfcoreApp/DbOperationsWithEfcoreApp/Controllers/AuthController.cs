using DbOperationsWithEfcoreApp.Data;
using DbOperationsWithEfcoreApp.Dtos;
using DbOperationsWithEfcoreApp.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;



namespace DbOperationsWithEfcoreApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {

        private readonly IValidator<RegisterDto> _registerValidator;
        private readonly IValidator<LoginDto> _loginValidator;
        private readonly IConfiguration _configuration;

        private readonly AppDbContext _context;
        public AuthController(AppDbContext appDbContext, IValidator<RegisterDto> registerValidator, IValidator<LoginDto> loginValidator, IConfiguration configuration)
        {
            _registerValidator = registerValidator;
            _loginValidator = loginValidator;
            _context = appDbContext;
            _configuration = configuration;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            var validationResult = await _registerValidator.ValidateAsync(registerDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Validation failed.",
                    errors = validationResult.Errors.Select(e => new
                    {
                        field = e.PropertyName,
                        error = e.ErrorMessage
                    })
                });
            }

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == registerDto.Email.ToLower());
            if (existingUser != null)
            {
                return Conflict(new
                {
                    success = false,
                    messag = "this email is already registered"
                });
            }

            var newUser = new User
            {
                Name=registerDto.Name,
                Email = registerDto.Email,
                Role = "User",
                passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password)

            };
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "User registered successfully!" });




        }
        [Authorize(Roles ="Admin")]

        [HttpPost("force-unlock")]
        public async Task<IActionResult> ForceUnlock([FromBody] UnlockRequestDto dto)
        {
            // 1. User ko email se dhundho
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower());

            if (user == null)
            {
                return NotFound(new { success = false, message = "User not found." });
            }

     
            user.FailedLoginAttempts = 0;
            user.LockoutEnd = null; 

            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Account forcefully unlocked successfully!",
                user = new { user.Email, user.FailedLoginAttempts, user.LockoutEnd }
            });
        }


        public class UnlockRequestDto
        {
            public string Email { get; set; }
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            // 1. Fluent Validation
            var validationResult = await _loginValidator.ValidateAsync(loginDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Validation failed",
                    errors = validationResult.Errors.Select(e => new
                    {
                        field = e.PropertyName,
                        error = e.ErrorMessage
                    })
                });
            }

            // 2. User ko sirf Email se dhundho
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == loginDto.Email.Trim().ToLower());

            if (user == null)
            {
                return Unauthorized(new { success = false, message = "Invalid email or password." });
            }

            // 3. Check karo: Kya account abhi LOCKED hai?
            if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
            {
                var minutesLeft = Math.Ceiling((user.LockoutEnd.Value - DateTime.UtcNow).TotalMinutes);
                return StatusCode(423, new
                {
                    success = false,
                    message = $"Account is temporarily locked. Please try again in {minutesLeft} minutes."
                });
            }

            // 4. Password Verify karo
            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.passwordHash))
            {
                // ❌ Galat Password: Failed attempts count badhao
                user.FailedLoginAttempts++;

                // Agar 5 baar galat kiya, to 15 minute ke liye lock kar do
                if (user.FailedLoginAttempts >= 5)
                {
                    user.LockoutEnd = DateTime.UtcNow.AddMinutes(1);
                    await _context.SaveChangesAsync();

                    return StatusCode(423, new
                    {
                        success = false,
                        message = "Account locked due to too many failed attempts. Try again after 15 minutes."
                    });
                }

                // Attempt count save karo aur user ko batao ke kitne chances bache hain
                await _context.SaveChangesAsync();
                int attemptsLeft = 5 - user.FailedLoginAttempts;

                return Unauthorized(new
                {
                    success = false,
                    message = $"Invalid email or password. {attemptsLeft} attempt(s) remaining before account lock."
                });
            }

            // 5. ✅ SUCCESSFUL LOGIN: Failed attempts ko RESET (0) kar do aur lock hata do
            user.FailedLoginAttempts = 0;
            user.LockoutEnd = null;
            await _context.SaveChangesAsync();

            // JWT Token generate karo
            var token = GenerateJwtToken(user);

            // 6. Success Response
            return Ok(new
            {
                success = true,
                message = "Login successful!",
                token = token,
                user = new
                {
                    id = user.Id,
                    email = user.Email,
                    name = user.Name,
                    role = user.Role
                }
            });
        }



        private string GenerateJwtToken(User user)
        {
            // appsettings.json se secret key lo
            var secretKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));

            var credentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(System.Security.Claims.ClaimTypes.Role, user.Role),
                new Claim("role", user.Role),
                new Claim("name", user.Name)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7), // Token 7 din valid rahega
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }




    }
    
     
}
