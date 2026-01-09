using NataliaQuintero.ContactApi.Models;

namespace NataliaQuintero.ContactApi.Services;

/// <summary>
/// Email service interface for sending contact form emails
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends a contact form email
    /// </summary>
    /// <param name="request">Contact request details</param>
    /// <returns>True if email was sent successfully</returns>
    Task<bool> SendContactEmailAsync(ContactRequest request);
}
