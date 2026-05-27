namespace DreamsPools.API.Models;

public class Coupon : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }   // % or fixed amount
    public decimal? MinOrderAmount { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public int? UsageLimit { get; set; }         // null = unlimited
    public int UsageCount { get; set; } = 0;
    public DateTime? ExpiryDate { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}

public enum DiscountType
{
    Percentage = 1,  // نسبة مئوية
    FixedAmount = 2  // مبلغ ثابت
}

public class Rating : BaseEntity
{
    public int Stars { get; set; }   // 1-5
    public string? Comment { get; set; }
    public RatingType Type { get; set; }

    // Foreign Keys
    public int UserId { get; set; }
    public int? AgentId { get; set; }
    public int? ProductId { get; set; }
    public int? AppointmentId { get; set; }

    // Navigation
    public User User { get; set; } = null!;
    public Agent? Agent { get; set; }
    public Product? Product { get; set; }
    public Appointment? Appointment { get; set; }
}

public enum RatingType
{
    Agent = 1,
    Product = 2
}

public class Notification : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public bool IsRead { get; set; } = false;
    public string? Data { get; set; }  // JSON extra data (orderId, appointmentId...)

    // Foreign Keys
    public int? UserId { get; set; }
    public int? AgentId { get; set; }

    // Navigation
    public User? User { get; set; }
    public Agent? Agent { get; set; }
}

public enum NotificationType
{
    AppointmentStatusChanged = 1,
    OrderStatusChanged = 2,
    NewOrder = 3,
    NewAppointment = 4,
    General = 5
}

// إعدادات التطبيق
public class AppSettings : BaseEntity
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
}
