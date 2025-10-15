using System.Text.RegularExpressions;
using Application.DTOs;
using FluentValidation;

namespace Application.Validation
{
    /// <summary>
    /// FluentValidation rules for <see cref="InstruksDto"/>.
    /// <list type="bullet">
    /// <item><description><c>Title</c> is required, max 160 chars, and must not contain raw HTML (no angle brackets).</description></item>
    /// <item><description><c>Description</c> optional, max 400 chars, no raw HTML (no angle brackets).</description></item>
    /// <item><description><c>Content</c> is required (rich HTML allowed; sanitized server-side).</description></item>
    /// <item><description><c>CategoryId</c> is required.</description></item>
    /// </list>
    /// These rules apply for both create and update operations since the same DTO is used.
    /// </summary>
    public sealed class InstruksDtoValidator : AbstractValidator<InstruksDto>
    {
        // crude "no angle brackets" regex for plain-text fields
        private static readonly Regex NoAngleBrackets = new(@"^[^<>]*$", RegexOptions.Compiled);

        /// <summary>
        /// Initializes a new instance of the <see cref="InstruksDtoValidator"/> class.
        /// </summary>
        public InstruksDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(160).WithMessage("Title must be at most 160 characters.")
                .Matches(NoAngleBrackets).WithMessage("Title must not contain raw HTML.");

            RuleFor(x => x.Description)
                .MaximumLength(400).WithMessage("Description must be at most 400 characters.")
                .Matches(NoAngleBrackets).WithMessage("Description must not contain raw HTML.")
                .When(x => x.Description is not null);

            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("Content (HTML) is required.")
                // Avoid excessively large payloads (tune as needed)
                .MaximumLength(200_000).WithMessage("Content is too large.");

            RuleFor(x => x.CategoryId)
                .NotEmpty().WithMessage("CategoryId is required.");
        }
    }
}
