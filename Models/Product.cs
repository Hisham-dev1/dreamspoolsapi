namespace DreamsPools.API.Models;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal VatAmount => Price * 0.15m; // VAT 15%
    public decimal PriceWithVat => Price + VatAmount;
    public int StockQuantity { get; set; } = 0;
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; } = false;
    public double AverageRating { get; set; } = 0;

    // Foreign Keys
    public int CategoryId { get; set; }

    // Navigation
    public Category Category { get; set; } = null!;
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
}
