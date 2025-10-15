using System.Text.RegularExpressions;
using Application.DTOs;
using FluentValidation;

namespace Application.Validation
{
    /// <summary>
    /// FluentValidation rules for <see cref="CategoryDto"/>.
    /// <para>
    /// Assumes <c>Name</c> and optional <c>Description</c> fields.
    /// If your DTO uses different property names (e.g., <c>Title</c>),
    /// simply swap the property references below.
    /// </para>
    /// <list type="bullet">
    /// <item><description><c>Name</c> is required, max 80 chars, no raw HTML.</description></item>
    /// <item><description><c>Description</c> optional, max 400 chars, no raw HTML.</description></item>
    /// </list>
    /// </summary>
    public sealed class CategoryDtoValidator : AbstractValidator<CategoryDto>
    {
        private static readonly Regex NoAngleBrackets = new(@"^[^<>]*$", RegexOptions.Compiled);

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryDtoValidator"/> class.
        /// </summary>
        public CategoryDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(80).WithMessage("Name must be at most 80 characters.")
                .Matches(NoAngleBrackets).WithMessage("Name must not contain raw HTML.");
        }
    }
}