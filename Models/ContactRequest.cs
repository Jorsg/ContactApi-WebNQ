namespace NataliaQuintero.ContactApi.Models;

/// <summary>
/// Contact form request model
/// </summary>
public class ContactRequest
{
    /// <summary>
    /// Full name of the person contacting
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Email address for reply
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Subject of the message
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Message content
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Language of the form (es, en, fr)
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    /// Timestamp when the form was submitted
    /// </summary>
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// API response model
/// </summary>
public class ContactResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, string>? Errors { get; set; }
}
