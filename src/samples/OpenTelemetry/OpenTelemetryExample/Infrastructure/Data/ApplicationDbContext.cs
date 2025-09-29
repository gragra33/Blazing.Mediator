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
    public DbSet<TelemetryLog> TelemetryLogs { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Don't use NoTracking by default as it can interfere with Add operations
        // optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
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
            entity.Property(e => e.ParentId).HasMaxLength(100);
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
            entity.Property(e => e.ParentId).HasMaxLength(100);
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

        // Configure TelemetryLog entity
        modelBuilder.Entity<TelemetryLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Timestamp).IsRequired();
            entity.Property(e => e.LogLevel).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Category).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Message).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.Exception).HasMaxLength(5000);
            entity.Property(e => e.TraceId).HasMaxLength(100);
            entity.Property(e => e.SpanId).HasMaxLength(100);
            entity.Property(e => e.Source).IsRequired().HasMaxLength(200);
            entity.Property(e => e.MachineName).HasMaxLength(200);
            entity.Property(e => e.ProcessId);
            entity.Property(e => e.ThreadId);
            entity.Property(e => e.EventId);
            entity.Property(e => e.Tags)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null) ?? new());
            entity.Property(e => e.Scopes)
                .HasConversion(
                    v => v != null ? JsonSerializer.Serialize(v, (JsonSerializerOptions?)null) : null,
                    v => v != null ? JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null) : null);
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
            "Green", "AdAMS", "Nelson", "Baker", "Hall", "Rivera", "Campbell", "Mitchell", "Carter", "Roberts"
        };

        var domains = new[] { "example.com", "demo.org", "test.net", "sample.co", "mock.io" };
        var random = System.Security.Cryptography.RandomNumberGenerator.Create(); // Use secure RNG
        var baseDate = DateTime.UtcNow.AddDays(-365); // Start from a year ago

        var users = new User[count];
        var randomBytes = new byte[4];
        for (int i = 0; i < count; i++)
        {
            random.GetBytes(randomBytes);
            var firstName = firstNames[BitConverter.ToUInt32(randomBytes, 0) % (uint)firstNames.Length];
            random.GetBytes(randomBytes);
            var lastName = lastNames[BitConverter.ToUInt32(randomBytes, 0) % (uint)lastNames.Length];
            random.GetBytes(randomBytes);
            var domain = domains[BitConverter.ToUInt32(randomBytes, 0) % (uint)domains.Length];
            var email = $"{firstName.ToUpperInvariant()}.{lastName.ToUpperInvariant()}@{domain}";

            // Make email unique by adding number for duplicates
            if (i > 0 && users.Take(i).Any(u => u.Email == email))
            {
                email = $"{firstName.ToUpperInvariant()}.{lastName.ToUpperInvariant()}{i}@{domain}";
            }

            random.GetBytes(randomBytes);
            var days = BitConverter.ToUInt32(randomBytes, 0) % 365;
            random.GetBytes(randomBytes);
            var isActive = (BitConverter.ToUInt32(randomBytes, 0) % 100) > 15; // 85% chance active

            users[i] = new User
            {
                Id = i + 1,
                Name = $"{firstName} {lastName}",
                Email = email,
                CreatedAt = baseDate.AddDays(days), // Random date within the year
                IsActive = isActive
            };
        }

        random.Dispose();
        return users;
    }
}
