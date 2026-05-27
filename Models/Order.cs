namespace DreamsPools.API.Models;

public class Order : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty; // DP-2024-0001
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public decimal SubTotal { get; set; }       // قبل الضريبة
    public decimal VatAmount { get; set; }       // الضريبة 15%
    public decimal DeliveryFee { get; set; }     // رسوم التوصيل
    public decimal DiscountAmount { get; set; } = 0; // الخصم
    public decimal TotalAmount { get; set; }     // الإجمالي بعد كل شيء
    public string? Notes { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

    // Delivery Address
    public string District { get; set; } = string.Empty;
    public string? Street { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    // Foreign Keys
    public int UserId { get; set; }
    public int? AgentId { get; set; }
    public int? CouponId { get; set; }

    // Navigation
    public User User { get; set; } = null!;
    public Agent? Agent { get; set; }
    public Coupon? Coupon { get; set; }
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public Invoice? Invoice { get; set; }
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}

public class OrderItem : BaseEntity
{
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal VatAmount { get; set; }
    public decimal TotalPrice { get; set; }

    // Foreign Keys
    public int OrderId { get; set; }
    public int ProductId { get; set; }

    // Navigation
    public Order Order { get; set; } = null!;
    public Product Product { get; set; } = null!;
}

public enum OrderStatus
{
    Pending = 1,       // تحت المراجعة
    Confirmed = 2,     // تم القبول
    InProgress = 3,    // جاري التحضير
    OnTheWay = 4,      // في الطريق إليك
    Delivered = 5,     // تم التسليم
    Cancelled = 6      // ملغي
}

public enum PaymentMethod
{
    Cash = 1,
    Online = 2
}

public enum PaymentStatus
{
    Pending = 1,
    Paid = 2,
    Refunded = 3,
    Failed = 4
}
