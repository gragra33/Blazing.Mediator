using Microsoft.EntityFrameworkCore;
using OpenTelemetryExample.Domain.Entities;
using System.Text.Json;

namespace OpenTelemetryExample.Infrastructure.Data;

/// <summary>
/// In-memory database context for the OpenTelemetry example.
/// </summary>
public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<TelemetryMetric> TelemetryMetrics { get; set; } = null!;
    public DbSet<TelemetryTrace> TelemetryTraces { get; set; } = null!;
    public DbSet<TelemetryActivity> TelemetryActivities { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    }

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

        // Configure TelemetryMetric entity
        modelBuilder.Entity<TelemetryMetric>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RequestType).IsRequired().HasMaxLength(200);
            entity.Property(e => e.RequestName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Category).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Duration).IsRequired();
            entity.Property(e => e.IsSuccess).IsRequired();
            entity.Property(e => e.ErrorMessage).HasMaxLength(500);
            entity.Property(e => e.Timestamp).IsRequired();
            entity.Property(e => e.HandlerName).HasMaxLength(200);
            entity.Property(e => e.Tags)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null) ?? new());
        });

        // Configure TelemetryTrace entity
        modelBuilder.Entity<TelemetryTrace>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TraceId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.SpanId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.OperationName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.StartTime).IsRequired();
            entity.Property(e => e.Duration).IsRequired();
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ExceptionType).HasMaxLength(200);
            entity.Property(e => e.ExceptionMessage).HasMaxLength(500);
            entity.Property(e => e.RequestType).IsRequired().HasMaxLength(200);
            entity.Property(e => e.HandlerName).HasMaxLength(200);
            entity.Property(e => e.Tags)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null) ?? new());
        });

        // Configure TelemetryActivity entity
        modelBuilder.Entity<TelemetryActivity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ActivityId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.OperationName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.StartTime).IsRequired();
            entity.Property(e => e.Duration).IsRequired();
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Kind).IsRequired().HasMaxLength(50);
            entity.Property(e => e.RequestType).IsRequired().HasMaxLength(200);
            entity.Property(e => e.HandlerName).HasMaxLength(200);
            entity.Property(e => e.IsSuccess).IsRequired();
            entity.Property(e => e.Tags)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null) ?? new());
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