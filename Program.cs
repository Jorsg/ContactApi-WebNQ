using NataliaQuintero.ContactApi.Models;
using NataliaQuintero.ContactApi.Services;
using NataliaQuintero.ContactApi.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<ContactRequestValidator>();

// Register Email Service
builder.Services.AddScoped<IEmailService, EmailService>();

// Configure CORS - IMPORTANT: Update with your actual domain
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
    ?? new[] {
        "http://127.0.0.1:5500",
        "http://localhost:5500",
        "http://localhost:8080",
        "https://nataliaquintero.ca"
    };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebsite", policy =>
    {
        //policy.WithOrigins(allowedOrigins)
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
        //.SetIsOriginAllowed(origin => true)
        //.AllowCredentials();
    });
});

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Use HTTPS redirection
//app.UseHttpsRedirection();

// Use CORS
app.UseCors("AllowWebsite");

// Use authorization
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
   .WithName("HealthCheck")
   .WithOpenApi();

app.Run();
