using NataliaQuintero.ContactApi.Models;
using NataliaQuintero.ContactApi.Services;
using NataliaQuintero.ContactApi.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Explicitly configure configuration sources to load environment variables
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
	options.SwaggerDoc("v1", new OpenApiInfo
	{
		Title = "Natalia Quintero - Contact API",
		Version = "v1",
		Description = "API para envío de contactos - Contact form backend",
		Contact = new OpenApiContact
		{
			Name = "Natalia Quintero",
			Email = "noreply@example.com",
			Url = new Uri("https://nataliaquintero.ca")
		}
	});

	// Include XML comments if the project is configured to generate them
	try
	{
		var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
		var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
		if (File.Exists(xmlPath))
		{
			options.IncludeXmlComments(xmlPath);
		}
	}
	catch { /* ignore if reflection/path fails */ }
});

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
		"https://nataliaquintero.ca",
		"https://www.nataliaquintero.ca",
		"http://nataliaquintero.ca",
		"http://www.nataliaquintero.ca"
	};
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowWebsite", policy =>
	{
		policy
			.WithOrigins(
				"http://127.0.0.1:5500",
				"http://localhost:5500",
				"https://nataliaquintero.ca",
				"https://www.nataliaquintero.ca"
			)
			.AllowAnyMethod()
			.AllowAnyHeader()
			.AllowCredentials()
			.WithExposedHeaders("Content-Disposition"); // Expose any custom headers if needed
	});
});

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Configure the HTTP request pipeline
// Enable Swagger only in Development
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

// Use HTTPS redirection
//app.UseHttpsRedirection();

// Use CORS - MUST be before UseAuthorization
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
