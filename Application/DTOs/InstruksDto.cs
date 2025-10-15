using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    /// <summary>
    /// DTO representing an Instruks document (including versioning metadata).
    /// </summary>
    public class InstruksDto
    {
        /// <summary>Current version id (server-generated).</summary>
        public Guid Id { get; set; }

        /// <summary>Stable document series id (server-generated on first version).</summary>
        public Guid DocumentId { get; set; }

        /// <summary>Monotonic version number (1, 2, ...).</summary>
        public int VersionNumber { get; set; }

        /// <summary>Flag indicating if this is the latest version.</summary>
        public bool IsLatest { get; set; }

        /// <summary>Title shown in listings and headers.</summary>
        [Required, StringLength(160)]
        [RegularExpression(@"^[^<>]*$", ErrorMessage = "Title must not contain raw HTML.")]
        public string Title { get; set; } = string.Empty;

        /// <summary>Optional short summary.</summary>
        [StringLength(400)]
        [RegularExpression(@"^[^<>]*$", ErrorMessage = "Description must not contain raw HTML.")]
        public string? Description { get; set; }

        /// <summary>
        /// Rich HTML content. This is required and will be sanitized server-side before storage.
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        [StringLength(200_000, ErrorMessage = "Content is too large.")]
        public string? Content { get; set; }

        /// <summary>Owning category id.</summary>
        [Required]
        public Guid CategoryId { get; set; }

        /// <summary>Creation timestamp (UTC; server-set).</summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>Last update timestamp (UTC; server-set).</summary>
        public DateTime? UpdatedAt { get; set; }
    }
}