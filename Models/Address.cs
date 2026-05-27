namespace DreamsPools.API.Models;

public class Address : BaseEntity
{
    public string Title { get; set; } = string.Empty; // Home, Work...
    public string District { get; set; } = string.Empty; // الحي
    public string? Street { get; set; }
    public string? BuildingNumber { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public bool IsDefault { get; set; } = false;

    // Foreign Keys
    public int UserId { get; set; }

    // Navigation
    public User User { get; set; } = null!;
}
