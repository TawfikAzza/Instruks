using Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : IdentityDbContext<IdentityUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Category> CategoryTable { get; set; }
    public DbSet<Instruks> InstruksTable { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder); 

        modelBuilder.Entity<Category>()
            .HasMany(c => c.InstruksItems)
            .WithOne(i => i.Category)
            .HasForeignKey(i => i.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Category>()
            .HasOne<Category>()
            .WithMany()
            .HasForeignKey(c => c.ParentId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<Instruks>(b =>
        {
            b.HasIndex(x => new { x.DocumentId, x.VersionNumber }).IsUnique();
            b.HasIndex(x => new { x.CategoryId, x.IsLatest }); 
            b.Property(x => x.IsLatest).HasDefaultValue(true);
        });

    }
}