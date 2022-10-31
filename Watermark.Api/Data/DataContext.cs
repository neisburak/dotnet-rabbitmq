using Watermark.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Watermark.Api.Data;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>().Property(o => o.Price).HasPrecision(18, 2);

        base.OnModelCreating(modelBuilder);
    }

    public DbSet<Product> Products => Set<Product>();
}