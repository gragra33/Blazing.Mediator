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

        // Seed initial data - Generate 100 sample users for comprehensive streaming tests
        var sampleUsers = GenerateSampleUsers(100);
        modelBuilder.Entity<User>().HasData(sampleUsers);
    }

    /// <summary>
    /// Generates sample user data for testing streaming functionality.
    /// </summary>
    /// <param name="count">Number of users to generate</param>
    /// <returns>Array of User entities</returns>
    private static User[] GenerateSampleUsers(int count)
    {
        var firstNames = new[]
        {
            "John", "Jane", "Bob", "Alice", "Charlie", "Diana", "Frank", "Grace", "Henry", "Ivy",
            "Jack", "Karen", "Leo", "Mia", "Nick", "Olivia", "Paul", "Quinn", "Rachel", "Sam",
            "Tom", "Uma", "Victor", "Wendy", "Xavier", "Yara", "Zoe", "Adam", "Beth", "Carl",
            "Emma", "Felix", "Gina", "Hugo", "Iris", "Jake", "Luna", "Max", "Nina", "Oscar",
            "Petra", "Quentin", "Rosa", "Steve", "Tina", "Ulrich", "Vera", "Will", "Xara", "Yale"
        };

        var lastNames = new[]
        {
            "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez",
            "Hernandez", "Lopez", "Gonzalez", "Wilson", "Anderson", "Thomas", "Taylor", "Moore", "Jackson", "Martin",
            "Lee", "Perez", "Thompson", "White", "Harris", "Sanchez", "Clark", "Ramirez", "Lewis", "Robinson",
            "Walker", "Young", "Allen", "King", "Wright", "Scott", "Torres", "Nguyen", "Hill", "Flores",
            "Green", "Adams", "Nelson", "Baker", "Hall", "Rivera", "Campbell", "Mitchell", "Carter", "Roberts"
        };

        var domains = new[] { "example.com", "demo.org", "test.net", "sample.co", "mock.io" };
        var random = new Random(42); // Fixed seed for consistent data
        var baseDate = DateTime.UtcNow.AddDays(-365); // Start from a year ago

        var users = new User[count];
        for (int i = 0; i < count; i++)
        {
            var firstName = firstNames[random.Next(firstNames.Length)];
            var lastName = lastNames[random.Next(lastNames.Length)];
            var domain = domains[random.Next(domains.Length)];
            var email = $"{firstName.ToLower()}.{lastName.ToLower()}@{domain}";
            
            // Make email unique by adding number for duplicates
            if (i > 0 && users.Take(i).Any(u => u.Email == email))
            {
                email = $"{firstName.ToLower()}.{lastName.ToLower()}{i}@{domain}";
            }

            users[i] = new User
            {
                Id = i + 1,
                Name = $"{firstName} {lastName}",
                Email = email,
                CreatedAt = baseDate.AddDays(random.NextDouble() * 365), // Random date within the year
                IsActive = random.NextDouble() > 0.15 // 85% chance of being active
            };
        }

        return users;
    }
}