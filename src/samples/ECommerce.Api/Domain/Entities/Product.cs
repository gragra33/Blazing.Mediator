namespace ECommerce.Api.Domain.Entities;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    // Domain methods
    public static Product Create(string name, string description, decimal price, int stockQuantity)
    {
        return new Product
        {
            Name = name,
            Description = description,
            Price = price,
            StockQuantity = stockQuantity,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateStock(int quantity)
    {
        StockQuantity = quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool HasSufficientStock(int requestedQuantity)
    {
        return IsActive && StockQuantity >= requestedQuantity;
    }

    public void ReserveStock(int quantity)
    {
        if (!HasSufficientStock(quantity))
            throw new InvalidOperationException($"Insufficient stock for product {Name}");

        StockQuantity -= quantity;
        UpdatedAt = DateTime.UtcNow;
    }
}