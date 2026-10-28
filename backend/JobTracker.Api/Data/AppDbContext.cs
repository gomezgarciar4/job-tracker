using JobTracker.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace JobTracker.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Offer> Offers => Set<Offer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Offer>(entity =>
        {
            entity.Property(o => o.Company).HasMaxLength(150);
            entity.Property(o => o.Position).HasMaxLength(150);
            entity.Property(o => o.Url).HasMaxLength(500);
            entity.Property(o => o.Status)
                .HasConversion<string>()
                .HasMaxLength(20);
        });
    }
}

