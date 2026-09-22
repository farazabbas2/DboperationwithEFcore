using DbOperationsWithEfcoreApp.Data;
using DbOperationsWithEfcoreApp.Middlewares;
using DbOperationsWithEfcoreApp.Validators;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace DbOperationsWithEfcoreApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. DbContext Registration (Ensure karein ki appsettings.json mein "AppDb" naam ka connection string ho)
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("AppDb"))
            );



            // 2. Controllers + JSON Options (Merge kar diya, duplicate hata diya)
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowReactApp", policy =>
                {
                    policy.WithOrigins("http://localhost:5173") // Vite ka default port
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            // 3. Fluent Validation
            builder.Services.AddValidatorsFromAssemblyContaining<Program>();

            // 4. Swagger
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Program.cs mein builder.Services.AddControllers() ke baad:
         

    

            // Middleware section mein:
            app.UseCors("AllowReactApp"); // Ye line app.UseHttpsRedirection() se PEHLE honi chahiye


            // Middleware
            app.UseMiddleware<ExceptionHandlingMiddleware>();

            // Swagger UI
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}