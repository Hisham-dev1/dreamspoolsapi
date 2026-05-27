using DreamsPools.API.Models;

namespace DreamsPools.API.DTOs.Orders;

public class CreateOrderDto
{
    public List<OrderItemDto> Items { get; set; } = new();
    public string District { get; set; } = string.Empty;
    public string? Street { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? Notes { get; set; }
    public string? CouponCode { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
}

public class OrderItemDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

public class OrderResponseDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal VatAmount { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? AgentName { get; set; }
    public string? AgentPhone { get; set; }
    public List<OrderItemResponseDto> Items { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
}

public class OrderItemResponseDto
{
    public string ProductName { get; set; } = string.Empty;
    public string? ProductImage { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}

public class UpdateOrderStatusDto
{
    public OrderStatus Status { get; set; }
    public int? AgentId { get; set; }
}
