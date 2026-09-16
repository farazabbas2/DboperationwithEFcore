using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using Studentst.Data;
using Studentst.Profiles;
using Studentst.Validators;
using System.Globalization;
using System.Text;
using AutoMapper;

namespace Studentst
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
           
           


            // 1. JWT Authentication Configuration
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
                        ValidAudience = builder.Configuration["JwtSettings:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]))
                    };
                });

            builder.Services.AddAuthorization();
           
            // 2. Controllers & FluentValidation Configuration
            builder.Services.AddControllers()
                    .AddJsonOptions(options =>
                    {
                        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                    })

                .AddFluentValidation(options =>
                {
                    options.RegisterValidatorsFromAssemblyContaining<Program>();
                    options.ValidatorOptions.LanguageManager.Culture = new CultureInfo("en-US");
                })
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.InvalidModelStateResponseFactory = (context) =>
                    {
                        var errors = context.ModelState
                            .Where(x => x.Value.Errors.Count > 0)
                            .SelectMany(x => x.Value.Errors.Select(error => new
                            {
                                propertyName = x.Key,
                                errorMessage = error.ErrorMessage
                            }))
                            .ToList();

                        var errorResponse = new
                        {
                            data = new
                            {
                                isValid = false,
                                errors = errors
                            }
                        };

                        return new UnprocessableEntityObjectResult(errorResponse);
                    };
                });

            // 3. Configure Swagger with JWT Bearer Support
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Studentst API", Version = "v1" });

                // Define Bearer Authentication
                var securityScheme = new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Description = "Enter JWT Bearer token. Example: Bearer eyJhbGci...",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                };

                c.AddSecurityDefinition("Bearer", securityScheme);

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            // 4. Configure AppDbContext
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            // Ye line honi chahiye:
            builder.Services.AddAutoMapper(typeof(Program).Assembly);


            var app = builder.Build();
            app.UseMiddleware<Studentst.Middlewares.CustomExceptionMiddleware>();

            // 5. Configure Middleware Pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            // IMPORTANT: UseAuthentication MUST come before UseAuthorization
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}