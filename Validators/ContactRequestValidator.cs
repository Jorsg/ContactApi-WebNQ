using FluentValidation;
using NataliaQuintero.ContactApi.Models;

namespace NataliaQuintero.ContactApi.Validators;

/// <summary>
/// Validator for ContactRequest using FluentValidation
/// </summary>
public class ContactRequestValidator : AbstractValidator<ContactRequest>
{
    public ContactRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MinimumLength(2).WithMessage("Name must be at least 2 characters")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(255).WithMessage("Email cannot exceed 255 characters");

        RuleFor(x => x.Subject)
            .NotEmpty().WithMessage("Subject is required")
            .MinimumLength(3).WithMessage("Subject must be at least 3 characters")
            .MaximumLength(200).WithMessage("Subject cannot exceed 200 characters");

        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Message is required")
            .MinimumLength(10).WithMessage("Message must be at least 10 characters")
            .MaximumLength(5000).WithMessage("Message cannot exceed 5000 characters");

        RuleFor(x => x.Language)
            .Must(lang => string.IsNullOrEmpty(lang) || new[] { "es", "en", "fr" }.Contains(lang?.ToLower()))
            .WithMessage("Language must be 'es', 'en', or 'fr'");
    }
}
