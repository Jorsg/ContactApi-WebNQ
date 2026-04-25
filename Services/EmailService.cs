using NataliaQuintero.ContactApi.Models;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Net;
using System.Net.Mail;

namespace NataliaQuintero.ContactApi.Services;

/// <summary>
/// Email service implementation supporting SendGrid and SMTP
/// </summary>
public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly string _emailProvider;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _emailProvider = _configuration["EmailSettings:Provider"] ?? "SendGrid";
    }

    public async Task<bool> SendContactEmailAsync(ContactRequest request)
    {
        try
        {
            _logger.LogInformation("Sending contact email from {Email} using {Provider}", request.Email, _emailProvider);

            return _emailProvider.ToLower() switch
            {
                "sendgrid" => await SendViaSendGridAsync(request),
                "smtp" => await SendViaSmtpAsync(request),
                _ => throw new NotSupportedException($"Email provider '{_emailProvider}' is not supported")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending contact email from {Email}", request.Email);
            throw;
        }
    }

    private async Task<bool> SendViaSendGridAsync(ContactRequest request)
    {
        var apiKey = _configuration["EmailSettings:SendGrid:ApiKey"];
        
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException("SendGrid API key is not configured");
        }

        var client = new SendGridClient(apiKey);
        
        var fromEmail = _configuration["EmailSettings:FromEmail"] ?? "noreply@nataliaquintero.com";
        var fromName = _configuration["EmailSettings:FromName"] ?? "Natalia Quintero Website";
        var toEmail = _configuration["EmailSettings:ToEmail"] ?? "contact@nataliaquintero.com";
        var toName = _configuration["EmailSettings:ToName"] ?? "Natalia Quintero";

        var from = new EmailAddress(fromEmail, fromName);
        var to = new EmailAddress(toEmail, toName);
        var replyTo = new EmailAddress(request.Email, request.Name);

        var subject = $"[Website Contact] {request.Subject}";
        
        var htmlContent = GenerateHtmlEmail(request);
        var plainTextContent = GeneratePlainTextEmail(request);

        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
        msg.ReplyTo = replyTo;

        // Add custom headers
        msg.AddHeader("X-Contact-Language", request.Language ?? "unknown");
        msg.AddHeader("X-Submitted-At", request.SubmittedAt.ToString("o"));

        var response = await client.SendEmailAsync(msg);

        if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Accepted)
        {
            _logger.LogInformation("Email sent successfully via SendGrid to {ToEmail}", toEmail);
            return true;
        }
        else
        {
            var responseBody = await response.Body.ReadAsStringAsync();
            _logger.LogError("SendGrid returned status {StatusCode}: {Response}", response.StatusCode, responseBody);
            return false;
        }
    }

    private async Task<bool> SendViaSmtpAsync(ContactRequest request)
    {
        var smtpHost = _configuration["EmailSettings:Smtp:Host"];
        var smtpPort = int.Parse(_configuration["EmailSettings:Smtp:Port"] ?? "587");
        var smtpUsername = _configuration["EmailSettings:Smtp:Username"];
        var smtpPassword = _configuration["EmailSettings:Smtp:Password"];
        var enableSsl = bool.Parse(_configuration["EmailSettings:Smtp:EnableSsl"] ?? "true");

        // DEBUG LOGS
        _logger.LogInformation("SMTP DEBUG - Host: {Host}, Username: {Username}, Password Length: {PasswordLength}, EnableSsl: {EnableSsl}", 
            smtpHost ?? "NULL", smtpUsername ?? "NULL", smtpPassword?.Length ?? 0, enableSsl);

        if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpUsername) || string.IsNullOrEmpty(smtpPassword))
        {
            throw new InvalidOperationException("SMTP settings are not properly configured");
        }

        var fromEmail = _configuration["EmailSettings:FromEmail"] ?? smtpUsername;
        var fromName = _configuration["EmailSettings:FromName"] ?? "Natalia Quintero Website";
        var toEmail = _configuration["EmailSettings:ToEmail"] ?? smtpUsername;

        using var smtpClient = new SmtpClient(smtpHost, smtpPort)
        {
            Credentials = new NetworkCredential(smtpUsername, smtpPassword),
            EnableSsl = enableSsl,
            Timeout = 10000  // 10 seconds timeout
        };

        using var mailMessage = new MailMessage
        {
            From = new MailAddress(fromEmail, fromName),
            Subject = $"[Website Contact] {request.Subject}",
            Body = GenerateHtmlEmail(request),
            IsBodyHtml = true
        };

        mailMessage.To.Add(new MailAddress(toEmail));
        mailMessage.ReplyToList.Add(new MailAddress(request.Email, request.Name));

        try
        {
            await smtpClient.SendMailAsync(mailMessage);
            _logger.LogInformation("Email sent successfully via SMTP to {ToEmail}", toEmail);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email via SMTP to {ToEmail}. Error: {ErrorMessage}", toEmail, ex.Message);
            throw;  // Relanza la excepción para que se maneje en SendContactEmailAsync
        }
    }

    private string GenerateHtmlEmail(ContactRequest request)
    {
        var languageLabel = request.Language?.ToUpper() ?? "Unknown";
        
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: #1F3B57; color: white; padding: 20px; text-align: center; }}
        .content {{ background: #F9F7F1; padding: 30px; }}
        .field {{ margin-bottom: 20px; }}
        .label {{ font-weight: bold; color: #2F5B4A; margin-bottom: 5px; }}
        .value {{ padding: 10px; background: white; border-left: 3px solid #D1A741; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>New Contact Form Submission</h2>
            <p>From Natalia Quintero Website</p>
        </div>
        <div class='content'>
            <div class='field'>
                <div class='label'>Name:</div>
                <div class='value'>{request.Name}</div>
            </div>
            <div class='field'>
                <div class='label'>Email:</div>
                <div class='value'>{request.Email}</div>
            </div>
            <div class='field'>
                <div class='label'>Subject:</div>
                <div class='value'>{request.Subject}</div>
            </div>
            <div class='field'>
                <div class='label'>Message:</div>
                <div class='value'>{request.Message.Replace("\n", "<br>")}</div>
            </div>
            <div class='field'>
                <div class='label'>Language:</div>
                <div class='value'>{languageLabel}</div>
            </div>
            <div class='field'>
                <div class='label'>Submitted At:</div>
                <div class='value'>{request.SubmittedAt:yyyy-MM-dd HH:mm:ss} UTC</div>
            </div>
        </div>
        <div class='footer'>
            <p>This email was sent from the contact form on your website.</p>
            <p>Reply directly to this email to respond to {request.Name}.</p>
        </div>
    </div>
</body>
</html>";
    }

    private string GeneratePlainTextEmail(ContactRequest request)
    {
        return $@"
NEW CONTACT FORM SUBMISSION
From: Natalia Quintero Website

Name: {request.Name}
Email: {request.Email}
Subject: {request.Subject}

Message:
{request.Message}

Language: {request.Language?.ToUpper() ?? "Unknown"}
Submitted At: {request.SubmittedAt:yyyy-MM-dd HH:mm:ss} UTC

---
This email was sent from the contact form on your website.
Reply directly to this email to respond to {request.Name}.
";
    }
  
}
