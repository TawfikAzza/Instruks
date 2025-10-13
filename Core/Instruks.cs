using System.ComponentModel.DataAnnotations;
namespace Domain;

public class Instruks
{
    public Guid Id { get; set; }                 // unique per version
    public Guid DocumentId { get; set; }         // stable group id for all versions of the same document
    public int VersionNumber { get; set; }       // 1..N within a DocumentId
    public bool IsLatest { get; set; }           // quick filter for current version

    [Timestamp]                                  // optimistic concurrency (optional but recommended)
    public byte[]? RowVersion { get; set; }

    // existing fields
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public string? Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // relations
    public Guid CategoryId { get; set; }
    public virtual Category? Category { get; set; } = null!;
    
    // convenience
    public Guid? PreviousVersionId { get; set; } // optional traceability chain
}