using Microsoft.EntityFrameworkCore;
using UserManagement.Api.Domain.Entities;

namespace UserManagement.Api.Infrastructure.Data;

/// <inheritdoc />
public class UserManagementDbContext : DbContext
{
    /// <inheritdoc />
    public UserManagementDbContext(DbContextOptions<UserManagementDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Represents the DbSet for Users in the application.
    /// </summary>
    public DbSet<User> Users { get; set; }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.CreatedAt).IsRequired();
        });

        // Seed data
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                DateOfBirth = new DateTime(1990, 1, 15),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                Id = 2,
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@example.com",
                DateOfBirth = new DateTime(1985, 6, 20),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                Id = 3,
                FirstName = "Bob",
                LastName = "Johnson",
                Email = "bob.johnson@example.com",
                DateOfBirth = new DateTime(1992, 3, 10),
                IsActive = false,
                CreatedAt = DateTime.UtcNow
            }
        );
    }
}
