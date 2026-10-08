using DbOperationsWithEfcoreApp.Data;
using DbOperationsWithEfcoreApp.Interfaces;
using DbOperationsWithEfcoreApp.Mappings;
using DbOperationsWithEfcoreApp.Middlewares;
using DbOperationsWithEfcoreApp.Repositories;
using DbOperationsWithEfcoreApp.Validators;
using FluentValidation;
using DbOperationsWithEfcoreApp.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using Serilog; // ✅ Ye pehle se hai
using System.Text;
using System.Text.Json.Serialization;
using DbOperationsWithEfcoreApp.Services.FriendService;
using DbOperationsWithEfcoreApp.Services.ActivityService;

namespace DbOperationsWithEfcoreApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // =================================================================
            // 1. SERILOG CONFIGURATION (App start hone se PEHLE)
            // =================================================================
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information() // Information, Warning, Error logs save karega
                .WriteTo.Console()          // Console par bhi dikhayega
                .WriteTo.File("Logs/app-log-.txt", rollingInterval: RollingInterval.Day) // Har din nayi file banegi
                .CreateLogger();

            try
            {
                Log.Information("🚀 Application starting up...");

                var builder = WebApplication.CreateBuilder(args);

                // =================================================================
                // 2. SERILOG KO DEPENDENCY INJECTION SE CONNECT KARNA
                // =================================================================
                builder.Host.UseSerilog(); // ✅ YE LINE BAHUT ZARURI HAI

                // 1. Authentication & JWT
                builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,
                            ValidIssuer = builder.Configuration["Jwt:Issuer"],
                            ValidAudience = builder.Configuration["Jwt:Audience"],
                            IssuerSigningKey = new SymmetricSecurityKey(
                                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"] ?? "DefaultSecretKey")),
                            RoleClaimType = System.Security.Claims.ClaimTypes.Role
                        };
                    });
                builder.Services.AddHttpContextAccessor();
                builder.Services.AddScoped<IBookRepository, BookRepository>();
                builder.Services.AddScoped<IColorRepository, ColorRepository>();

                builder.Services.AddScoped<ILanguageRepository, LanguageRepository>();
                // Repositories
                // Repositories
                builder.Services.AddScoped<IFriendRepository, FriendRepository>();
                builder.Services.AddScoped<IActivityRepository, ActivityRepository>();
                // Services
                builder.Services.AddScoped<IFriendService, FriendService>();
                builder.Services.AddScoped<IActivityService, ActivityService>();

                // Services
            
                builder.Services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlServer(builder.Configuration.GetConnectionString("AppDb"))
                );

                // 3. Controllers + JSON Options
                builder.Services.AddControllers()
                    .AddJsonOptions(options =>
                    {
                        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                    });
                builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

                // 4. CORS
                builder.Services.AddCors(options =>
                {
                    options.AddPolicy("AllowReactApp", policy =>
                    {
                        policy.WithOrigins("http://localhost:5173")
                              .AllowAnyHeader()
                              .AllowAnyMethod();
                    });
                });

                // 5. Fluent Validation
                builder.Services.AddValidatorsFromAssemblyContaining<Program>();

                // 6. Swagger / OpenAPI Configuration
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen(options =>
                {
                    options.SwaggerDoc("v1", new OpenApiInfo
                    {
                        Title = "DbOperations API",
                        Version = "v1"
                    });

                    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        Name = "Authorization",
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        BearerFormat = "JWT",
                        In = ParameterLocation.Header,
                        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\""
                    });

                    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

                var app = builder.Build();

                // Middleware Pipeline
                app.UseCors("AllowReactApp");

                // ✅ Custom Logging Middlewares (Ye pehle se aapke code me the, ye sahi jagah par hain)
                app.UseMiddleware<ExceptionHandlingMiddleware>();
                app.UseMiddleware<RequestLoggingMiddleware>();

                // Swagger Middleware
                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }

                // app.UseHttpsRedirection(); // Disabled in dev so Authorization headers are not stripped by 307 redirects

                // IMPORTANT: Authentication must come BEFORE Authorization
                app.UseAuthentication();
                app.UseAuthorization();

                app.MapControllers();
                app.UseStaticFiles();

                Log.Information("✅ Application started successfully!");
                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "❌ Application failed to start!");
            }
            finally
            {
                // App band hote waqt logs ko properly file me flush (save) karna
                Log.CloseAndFlush();
            }
        }
    }
}