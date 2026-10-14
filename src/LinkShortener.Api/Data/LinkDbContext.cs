using Microsoft.EntityFrameworkCore;

namespace LinkShortener.Api.Data;

public sealed class LinkDbContext(DbContextOptions<LinkDbContext> options) : DbContext(options)
{
    public DbSet<Link> Links => Set<Link>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Link>(entity =>
        {
            entity.HasKey(link => link.Id);
            entity.HasIndex(link => link.Code).IsUnique();
            entity.Property(link => link.Code).IsRequired().HasMaxLength(20);
            entity.Property(link => link.TargetUrl).IsRequired().HasMaxLength(2048);
        });
    }
}
