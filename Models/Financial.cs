namespace DreamsPools.API.Models;

// كل حركة مالية في النظام
public class Transaction : BaseEntity
{
    public string TransactionNumber { get; set; } = string.Empty;
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public decimal VatAmount { get; set; }
    public string? Description { get; set; }
    public string? ReferenceNumber { get; set; }

    // Foreign Keys
    public int? OrderId { get; set; }
    public int? UserId { get; set; }
    public int? AgentId { get; set; }

    // Navigation
    public Order? Order { get; set; }
    public User? User { get; set; }
    public Agent? Agent { get; set; }
}

public enum TransactionType
{
    OrderRevenue = 1,   // إيراد من طلب
    DeliveryFee = 2,    // رسوم توصيل
    AgentPayout = 3,    // مدفوعات للمندوب
    Refund = 4,         // استرداد مبلغ
    Expense = 5         // مصروف
}

// فاتورة ضريبية لكل طلب
public class Invoice : BaseEntity
{
    public string InvoiceNumber { get; set; } = string.Empty; // DP-INV-2024-0001
    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
    public decimal SubTotal { get; set; }
    public decimal VatAmount { get; set; }   // 15%
    public decimal TotalAmount { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Active;

    // Foreign Keys
    public int OrderId { get; set; }

    // Navigation
    public Order Order { get; set; } = null!;
}

public enum InvoiceStatus
{
    Active = 1,
    Cancelled = 2
}

// مصروفات تشغيلية يدخلها الأدمن
public class Expense : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public ExpenseCategory Category { get; set; }
    public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;
    public string? ReceiptImageUrl { get; set; }

    // Foreign Keys
    public int AdminId { get; set; }

    // Navigation
    public Admin Admin { get; set; } = null!;
}

public enum ExpenseCategory
{
    AgentSalary = 1,    // رواتب المندوبين
    Marketing = 2,       // تسويق
    Maintenance = 3,     // صيانة
    Supplies = 4,        // مستلزمات
    Other = 5            // أخرى
}
