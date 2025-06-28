using ECommerce.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Api.Infrastructure.Data;

public class ECommerceDbContext(DbContextOptions<ECommerceDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Product configuration
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
            entity.Property(e => e.CreatedAt).IsRequired();
        });

        // Order configuration
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CustomerEmail).IsRequired().HasMaxLength(255);
            entity.Property(e => e.ShippingAddress).IsRequired().HasMaxLength(500);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Status).HasConversion<int>();
            entity.Property(e => e.CreatedAt).IsRequired();

            entity.HasMany(e => e.Items)
                  .WithOne(e => e.Order)
                  .HasForeignKey(e => e.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // OrderItem configuration
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");

            entity.HasOne(e => e.Product)
                  .WithMany(e => e.OrderItems)
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Seed data
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Seed Products
        modelBuilder.Entity<Product>().HasData(
            new Product
            {
                Id = 1,
                Name = "Laptop Pro 15",
                Description = "High-performance laptop with 15-inch display",
                Price = 1299.99m,
                StockQuantity = 50,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = 2,
                Name = "Wireless Mouse",
                Description = "Ergonomic wireless mouse with precision tracking",
                Price = 29.99m,
                StockQuantity = 200,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = 3,
                Name = "USB-C Hub",
                Description = "Multi-port USB-C hub with HDMI and USB 3.0",
                Price = 49.99m,
                StockQuantity = 75,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = 4,
                Name = "Bluetooth Headphones",
                Description = "Noise-cancelling wireless headphones",
                Price = 199.99m,
                StockQuantity = 30,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = 5,
                Name = "External SSD 1TB",
                Description = "Portable high-speed external SSD storage",
                Price = 129.99m,
                StockQuantity = 0, // Out of stock
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        );

        // Seed Orders
        modelBuilder.Entity<Order>().HasData(
            new Order
            {
                Id = 1,
                CustomerId = 1,
                CustomerEmail = "john.doe@example.com",
                ShippingAddress = "123 Main St, Anytown, USA 12345",
                Status = OrderStatus.Delivered,
                TotalAmount = 1379.97m,
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new Order
            {
                Id = 2,
                CustomerId = 2,
                CustomerEmail = "jane.smith@example.com",
                ShippingAddress = "456 Oak Ave, Another City, USA 67890",
                Status = OrderStatus.Processing,
                TotalAmount = 79.98m,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            }
        );

        // Seed OrderItems
        modelBuilder.Entity<OrderItem>().HasData(
            new OrderItem { Id = 1, OrderId = 1, ProductId = 1, Quantity = 1, UnitPrice = 1299.99m },
            new OrderItem { Id = 2, OrderId = 1, ProductId = 2, Quantity = 1, UnitPrice = 29.99m },
            new OrderItem { Id = 3, OrderId = 1, ProductId = 3, Quantity = 1, UnitPrice = 49.99m },
            new OrderItem { Id = 4, OrderId = 2, ProductId = 2, Quantity = 2, UnitPrice = 29.99m },
            new OrderItem { Id = 5, OrderId = 2, ProductId = 4, Quantity = 1, UnitPrice = 199.99m }
        );
    }
}
