using Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {
    }

    // Define DbSets for your entities here
    // public DbSet<Product> Products { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)  {
        modelBuilder.Entity<Product>()
            .HasOne(p => p.Order)
            .WithMany(o => o.products)
            .HasForeignKey(p => p.OrderId)
            .OnDelete(DeleteBehavior.Cascade); // 👈 required for cascade delete

        modelBuilder.Entity<Product>()
            .Property(p => p.Id)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<Order>()
            .Property(o => o.Id)
            .ValueGeneratedOnAdd();
    } 
    
    public DbSet<Product> ProductTable { get; set; }
    public DbSet<Order> OrderTable { get; set; }
}