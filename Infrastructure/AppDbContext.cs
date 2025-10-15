using Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Entity Framework Core database context for the application,
/// extending <see cref="IdentityDbContext{TUser}"/> to include ASP.NET Core Identity tables
/// alongside domain entities such as <see cref="Category"/> and <see cref="Instruks"/>.
/// </summary>
/// <remarks>
/// Model configuration defines:
/// <list type="bullet">
///   <item><description>
///     <strong>Category → Instruks (1..many)</strong> with cascade delete:
///     deleting a <see cref="Category"/> removes its related <see cref="Instruks"/> rows.
///   </description></item>
///   <item><description>
///     <strong>Category → Parent</strong> (self-reference) with <see cref="DeleteBehavior.Restrict"/>:
///     a parent category cannot be deleted while children exist.
///   </description></item>
///   <item><description>
///     <strong>Indexes on <see cref="Instruks"/></strong>:
///     a unique index on (DocumentId, VersionNumber) to ensure a single row per version,
///     and a non-unique index on (CategoryId, IsLatest) to speed up “latest by category” queries.
///   </description></item>
///   <item><description>
///     <strong>Default values</strong>: <see cref="Instruks.IsLatest"/> defaults to <c>true</c>.
///   </description></item>
/// </list>
/// </remarks>
public class AppDbContext : IdentityDbContext<IdentityUser>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AppDbContext"/> class.
    /// </summary>
    /// <param name="options">The EF Core context options.</param>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    /// <summary>
    /// Table for <see cref="Category"/> entities.
    /// </summary>
    public DbSet<Category> CategoryTable { get; set; }

    /// <summary>
    /// Table for <see cref="Instruks"/> entities, including versioned rows.
    /// </summary>
    public DbSet<Instruks> InstruksTable { get; set; }

    /// <summary>
    /// Configures entity relationships, indexes, and defaults.
    /// </summary>
    /// <param name="modelBuilder">The model builder used to configure entity mappings.</param>
    /// <remarks>
    /// Invokes <see cref="IdentityDbContext.OnModelCreating(ModelBuilder)"/> first to ensure
    /// Identity mappings are configured, then applies domain-specific configuration.
    /// </remarks>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Category (1) → Instruks (many), cascade on delete
        modelBuilder.Entity<Category>()
            .HasMany(c => c.InstruksItems)
            .WithOne(i => i.Category)
            .HasForeignKey(i => i.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        // Category self-reference: ParentId (restrict on delete)
        modelBuilder.Entity<Category>()
            .HasOne<Category>()
            .WithMany()
            .HasForeignKey(c => c.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Instruks indexes + defaults
        modelBuilder.Entity<Instruks>(b =>
        {
            // Ensure only one row per (DocumentId, VersionNumber)
            b.HasIndex(x => new { x.DocumentId, x.VersionNumber }).IsUnique();

            // Speed up queries filtering by category and latest flag
            b.HasIndex(x => new { x.CategoryId, x.IsLatest });

            // Default: a newly created row is considered latest
            b.Property(x => x.IsLatest).HasDefaultValue(true);
        });
    }
}
