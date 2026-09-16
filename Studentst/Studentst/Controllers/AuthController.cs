using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Studentst.Data;
using Studentst.DTOs;
using Studentst.Models;
using FluentValidation;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Studentst.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        //validators inject
        private readonly IValidator<RegisterDto> _registerValidator;
        private readonly IValidator<LoginDto> _loginValidator;

        public AuthController(
            AppDbContext context,
            IConfiguration configuration,
            IValidator<RegisterDto> registerValidator,
            IValidator<LoginDto> loginValidator)
        {
            _context = context;
            _configuration = configuration;
            _registerValidator = registerValidator;
            _loginValidator = loginValidator;
        }

        // SIGNUP API
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            // 1. Validation Check
            var validationResult = await _registerValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return UnprocessableEntity(new
                {
                    data = validationResult.Errors,
                });
            }

            // 2. Check if user already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(x => x.Email == dto.Email || x.Username == dto.Username);

            if (existingUser != null)
            {
                return UnprocessableEntity(new
                {
                    data = new
                    {
                        isValid = false,
                        errors = new[]
                        {
                    new { propertyName = "Email", errorMessage = "User with this Email or Username already exists." }
                }
                    }
                });
            }

            // 3. Hash the password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            // 4. Create User
            var newUser = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = passwordHash,
                Role = "User"
            };

            await _context.Users.AddAsync(newUser);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "User registered successfully",
                data = new { id = newUser.id, username = newUser.Username, email = newUser.Email }
            });
        }

        //  LOGIN API
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            // 1. Validation Check
            var validationResult = await _loginValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return UnprocessableEntity(new
                {
                    data = validationResult.Errors,
                });
            }

            // 2. Find user by email
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == dto.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                // Security best practice: Don't reveal if email exists or password is wrong
                return UnprocessableEntity(new
                {
                    data = new
                    {
                        isValid = false,
                        errors = new[]
                        {
                            new { propertyName = "Credentials", errorMessage = "Invalid email or password." }
                        }
                    }
                });
            }

            // 3. Generate JWT Token
            var token = GenerateJwtToken(user);

            return Ok(new
            {
                success = true,
                message = "Login successful",
                data = new
                {
                    token = token,
                    user = new { id = user.id, username = user.Username, email = user.Email, role = user.Role }
                }
            });
        }

        // elper Method to Generate JWT Token (Same as before)
        private string GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:ExpiryInMinutes"])),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}