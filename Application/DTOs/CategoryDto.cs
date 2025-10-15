using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    /// <summary>
    /// Category used to organize Instruks.
    /// </summary>
    public class CategoryDto
    {
        /// <summary>Category identifier (server-generated).</summary>
        public Guid Id { get; set; }

        /// <summary>Category display name.</summary>
        [Required, StringLength(80)]
        [RegularExpression(@"^[^<>]*$", ErrorMessage = "Name must not contain raw HTML.")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Optional parent category id for hierarchical grouping.
        /// </summary>
        public Guid? ParentId { get; set; }
    }
}