using DreamsPools.API.Models;

public class Banner : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    /// <summary>0 = عادي، 1 = منتج</summary>
    public int Type { get; set; } = 0;
    public int? ProductId { get; set; }
    public bool IsVisible { get; set; } = true;
    public int DisplayOrder { get; set; } = 0;

    // Navigation
    public Product? Product { get; set; }
}