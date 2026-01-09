using Microsoft.AspNetCore.Mvc;
using NataliaQuintero.ContactApi.Models;
using NataliaQuintero.ContactApi.Services;

namespace NataliaQuintero.ContactApi.Controllers;

/// <summary>
/// Contact form API controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ContactController : ControllerBase
{
    private readonly IEmailService _emailService;
    private readonly ILogger<ContactController> _logger;

    public ContactController(IEmailService emailService, ILogger<ContactController> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    /// <summary>
    /// Submit a contact form
    /// </summary>
    /// <param name="request">Contact form data</param>
    /// <returns>Success or error response</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ContactResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ContactResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ContactResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ContactResponse>> SubmitContact([FromBody] ContactRequest request)
    {
        try
        {
            _logger.LogInformation("Received contact form submission from {Email}", request.Email);

            // ModelState validation is handled by FluentValidation
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Any() == true)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors.First().ErrorMessage ?? "Validation error"
                    );

                _logger.LogWarning("Contact form validation failed for {Email}: {Errors}", 
                    request.Email, string.Join(", ", errors.Values));

                return BadRequest(new ContactResponse
                {
                    Success = false,
                    Message = "Validation failed",
                    Errors = errors
                });
            }

            // Send email
            var emailSent = await _emailService.SendContactEmailAsync(request);

            if (emailSent)
            {
                _logger.LogInformation("Contact form processed successfully for {Email}", request.Email);

                return Ok(new ContactResponse
                {
                    Success = true,
                    Message = GetSuccessMessage(request.Language)
                });
            }
            else
            {
                _logger.LogError("Failed to send email for contact form from {Email}", request.Email);

                return StatusCode(500, new ContactResponse
                {
                    Success = false,
                    Message = GetErrorMessage(request.Language)
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing contact form from {Email}", request.Email);

            return StatusCode(500, new ContactResponse
            {
                Success = false,
                Message = GetErrorMessage(request.Language)
            });
        }
    }

    /// <summary>
    /// Health check endpoint for the contact API
    /// </summary>
    [HttpGet("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult HealthCheck()
    {
        return Ok(new
        {
            status = "healthy",
            service = "contact-api",
            timestamp = DateTime.UtcNow
        });
    }

    private string GetSuccessMessage(string? language)
    {
        return language?.ToLower() switch
        {
            "es" => "¡Gracias por tu mensaje! Te responderé lo antes posible.",
            "fr" => "Merci pour votre message! Je vous répondrai dans les plus brefs délais.",
            "en" or _ => "Thank you for your message! I will respond as soon as possible."
        };
    }

    private string GetErrorMessage(string? language)
    {
        return language?.ToLower() switch
        {
            "es" => "Hubo un error al enviar tu mensaje. Por favor, intenta nuevamente o contáctame directamente por email.",
            "fr" => "Une erreur s'est produite lors de l'envoi de votre message. Veuillez réessayer ou me contacter directement par email.",
            "en" or _ => "There was an error sending your message. Please try again or contact me directly via email."
        };
    }
}
