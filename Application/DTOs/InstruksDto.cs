namespace Application.DTOs;

public class InstruksDto
{
    public Guid Id { get; set; }             // current version id
    public Guid DocumentId { get; set; }     // stable doc id
    public int VersionNumber { get; set; }
    public bool IsLatest { get; set; }

    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public string? Content { get; set; }
    public Guid CategoryId { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
