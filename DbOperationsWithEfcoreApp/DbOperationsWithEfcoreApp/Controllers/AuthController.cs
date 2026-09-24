using DbOperationsWithEfcoreApp.Data;
using DbOperationsWithEfcoreApp.Dtos;
using DbOperationsWithEfcoreApp.Models;
using FluentValidation;
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
                passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password)

            };
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "User registered successfully!" });




        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        
      {
              var validationResult = await _loginValidator.ValidateAsync(loginDto);
                if(!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "validation failed",
                    errors = validationResult.Errors.Select(e => new
                    {
                        field = e.PropertyName,
                        error = e.ErrorMessage
                    })
                });

       

            }
            var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == loginDto.Email.ToLower());
            if (user == null)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Invalid email or password."
                });
            }

            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.passwordHash))
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Invalid email or password."
                });
            }
            var token = GenerateJwtToken(user);

            return Ok(new
            {
                success = true,
                message = "Login successful!",
                token = token,
                user = new
                {
                    id = user.Id,
                    email = user.Email
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
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(2), // Token 2 ghante valid rahega
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }




    }
    
     
}
