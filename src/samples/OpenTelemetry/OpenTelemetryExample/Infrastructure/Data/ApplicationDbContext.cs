using Microsoft.EntityFrameworkCore;
using OpenTelemetryExample.Domain.Entities;

namespace OpenTelemetryExample.Infrastructure.Data;

/// <summary>
/// In-memory database context for the OpenTelemetry example.
/// </summary>
public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.IsActive).IsRequired();
        });

        // Seed initial data
        modelBuilder.Entity<User>().HasData(
            new User 
            { 
                Id = 1, 
                Name = "John Doe", 
                Email = "john@example.com", 
                CreatedAt = DateTime.UtcNow.AddDays(-30), 
                IsActive = true 
            },
            new User 
            { 
                Id = 2, 
                Name = "Jane Smith", 
                Email = "jane@example.com", 
                CreatedAt = DateTime.UtcNow.AddDays(-15), 
                IsActive = true 
            },
            new User 
            { 
                Id = 3, 
                Name = "Bob Johnson", 
                Email = "bob@example.com", 
                CreatedAt = DateTime.UtcNow.AddDays(-7), 
                IsActive = false 
            },
            new User 
            { 
                Id = 4, 
                Name = "Alice Brown", 
                Email = "alice@example.com", 
                CreatedAt = DateTime.UtcNow.AddDays(-2), 
                IsActive = true 
            },
            new User 
            { 
                Id = 5, 
                Name = "Charlie Wilson", 
                Email = "charlie@example.com", 
                CreatedAt = DateTime.UtcNow.AddDays(-1), 
                IsActive = true 
            }
        );
    }
}