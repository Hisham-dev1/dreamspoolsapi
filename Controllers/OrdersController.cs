using DreamsPools.API.Data;
using DreamsPools.API.DTOs.Orders;
using DreamsPools.API.Helpers;
using DreamsPools.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DreamsPools.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _db;

    public OrdersController(AppDbContext db) => _db = db;

    // Customer: Place order
    [HttpPost]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> Create(CreateOrderDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // Get VAT & delivery fee settings
        var vatSetting = await _db.AppSettings.FirstOrDefaultAsync(s => s.Key == "VatPercentage");
        var deliverySetting = await _db.AppSettings.FirstOrDefaultAsync(s => s.Key == "DeliveryFee");
        decimal vatRate = decimal.Parse(vatSetting?.Value ?? "15") / 100;
        decimal deliveryFee = decimal.Parse(deliverySetting?.Value ?? "20");

        // Validate products
        var productIds = dto.Items.Select(i => i.ProductId).ToList();
        var products = await _db.Products
            .Where(p => productIds.Contains(p.Id) && p.IsActive)
            .ToListAsync();

        if (products.Count != dto.Items.Count)
            return BadRequest(ApiResponse<string>.Fail("بعض المنتجات غير متوفرة"));

        // Calculate amounts
        decimal subTotal = 0;
        var orderItems = new List<OrderItem>();

        foreach (var item in dto.Items)
        {
            var product = products.First(p => p.Id == item.ProductId);
            var itemTotal = product.Price * item.Quantity;
            var itemVat = itemTotal * vatRate;

            subTotal += itemTotal;
            orderItems.Add(new OrderItem
            {
                ProductId = product.Id,
                Quantity = item.Quantity,
                UnitPrice = product.Price,
                VatAmount = itemVat,
                TotalPrice = itemTotal + itemVat
            });
        }

        decimal vatAmount = subTotal * vatRate;
        decimal discountAmount = 0;

        // Apply coupon
        Coupon? coupon = null;
        if (!string.IsNullOrEmpty(dto.CouponCode))
        {
            coupon = await _db.Coupons.FirstOrDefaultAsync(c =>
                c.Code == dto.CouponCode && c.IsActive &&
                (c.ExpiryDate == null || c.ExpiryDate > DateTime.UtcNow) &&
                (c.UsageLimit == null || c.UsageCount < c.UsageLimit));

            if (coupon == null)
                return BadRequest(ApiResponse<string>.Fail("كود الخصم غير صالح أو منتهي الصلاحية"));

            discountAmount = coupon.DiscountType == DiscountType.Percentage
                ? subTotal * (coupon.DiscountValue / 100)
                : coupon.DiscountValue;

            if (coupon.MaxDiscountAmount.HasValue)
                discountAmount = Math.Min(discountAmount, coupon.MaxDiscountAmount.Value);

            coupon.UsageCount++;
        }

        decimal totalAmount = subTotal + vatAmount + deliveryFee - discountAmount;

        var order = new Order
        {
            UserId = userId,
            Status = OrderStatus.Pending,
            SubTotal = subTotal,
            VatAmount = vatAmount,
            DeliveryFee = deliveryFee,
            DiscountAmount = discountAmount,
            TotalAmount = totalAmount,
            Notes = dto.Notes,
            PaymentMethod = dto.PaymentMethod,
            District = dto.District,
            Street = dto.Street,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            CouponId = coupon?.Id,
            Items = orderItems,
            OrderNumber = "TEMP"
        };

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        order.OrderNumber = NumberGenerator.GenerateOrderNumber(order.Id);

        // Create invoice
        var invoice = new Invoice
        {
            OrderId = order.Id,
            SubTotal = subTotal,
            VatAmount = vatAmount,
            TotalAmount = totalAmount,
            InvoiceNumber = "TEMP"
        };
        _db.Invoices.Add(invoice);
        await _db.SaveChangesAsync();

        invoice.InvoiceNumber = NumberGenerator.GenerateInvoiceNumber(invoice.Id);

        // Create transaction
        var transaction = new Transaction
        {
            OrderId = order.Id,
            UserId = userId,
            Type = TransactionType.OrderRevenue,
            Amount = subTotal,
            VatAmount = vatAmount,
            Description = $"إيراد طلب {order.OrderNumber}",
            TransactionNumber = "TEMP"
        };
        _db.Transactions.Add(transaction);
        await _db.SaveChangesAsync();

        transaction.TransactionNumber = NumberGenerator.GenerateTransactionNumber(transaction.Id);
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<string>.Ok(order.OrderNumber, "تم إنشاء الطلب بنجاح"));
    }

    // Customer: My orders
    [HttpGet("my")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> GetMyOrders()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var orders = await _db.Orders
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Include(o => o.Agent)
            .Include(o => o.Invoice)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return Ok(ApiResponse<List<OrderResponseDto>>.Ok(orders.Select(MapToDto).ToList()));
    }

    // Admin: All orders
    [HttpGet]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> GetAll([FromQuery] OrderStatus? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var query = _db.Orders
            .Include(o => o.User)
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Include(o => o.Agent)
            .Include(o => o.Invoice)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(o => o.Status == status.Value);

        var total = await query.CountAsync();
        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(ApiResponse<List<OrderResponseDto>>.Ok(orders.Select(MapToDto).ToList(), total: total));
    }

    // Admin: Update order status
    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateOrderStatusDto dto)
    {
        var order = await _db.Orders
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return NotFound(ApiResponse<string>.Fail("الطلب غير موجود"));

        order.Status = dto.Status;
        if (dto.AgentId.HasValue) order.AgentId = dto.AgentId;

        await _db.SaveChangesAsync();

        // TODO: Send push notification

        return Ok(ApiResponse<string>.Ok("تم تحديث حالة الطلب"));
    }

    private static OrderResponseDto MapToDto(Order o) => new()
    {
        Id = o.Id,
        OrderNumber = o.OrderNumber,
        Status = o.Status,
        StatusText = o.Status switch
        {
            OrderStatus.Pending => "تحت المراجعة",
            OrderStatus.Confirmed => "تم القبول",
            OrderStatus.InProgress => "جاري التحضير",
            OrderStatus.OnTheWay => "في الطريق إليك",
            OrderStatus.Delivered => "تم التسليم",
            OrderStatus.Cancelled => "ملغي",
            _ => ""
        },
        SubTotal = o.SubTotal,
        VatAmount = o.VatAmount,
        DeliveryFee = o.DeliveryFee,
        DiscountAmount = o.DiscountAmount,
        TotalAmount = o.TotalAmount,
        AgentName = o.Agent?.FullName,
        AgentPhone = o.Agent?.PhoneNumber,
        InvoiceNumber = o.Invoice?.InvoiceNumber ?? "",
        Items = o.Items.Select(i => new OrderItemResponseDto
        {
            ProductName = i.Product?.Name ?? "",
            ProductImage = i.Product?.ImageUrl,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            TotalPrice = i.TotalPrice
        }).ToList(),
        CreatedAt = o.CreatedAt
    };
}
