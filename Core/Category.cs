namespace Domain;

public class Category {
    public Guid Id { get; set; }
    public Guid? ParentId { get; set; }
    public string Name { get; set; }
    // Navigation: One Category to Many Instruks
    public virtual ICollection<Instruks> InstruksItems { get; set; }
}