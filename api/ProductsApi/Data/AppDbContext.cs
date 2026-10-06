using Microsoft.EntityFrameworkCore;
using ProductsApi.Models;

namespace ProductsApi.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.HasDefaultSchema("catalogo");
        b.Entity<Product>(e =>
        {
            e.ToTable("Produtos");
            e.HasIndex(p => p.Sku).IsUnique();
            e.Property(p => p.Sku).HasMaxLength(40).IsRequired();
            e.Property(p => p.Name).HasMaxLength(150).IsRequired();
            e.Property(p => p.Price).HasPrecision(12, 2);
        });
    }
}
