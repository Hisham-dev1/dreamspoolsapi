using DreamsPools.API.Data;
using DreamsPools.API.DTOs.Dashboard;
using DreamsPools.API.Helpers;
using DreamsPools.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DreamsPools.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _db;
    public DashboardController(AppDbContext db) => _db = db;

    // لوحة التحكم الرئيسية
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary([FromQuery] string period = "month")
    {
        var (from, to) = GetDateRange(period);

        var orders = await _db.Orders
            .Where(o => o.CreatedAt >= from && o.CreatedAt <= to && o.Status != OrderStatus.Cancelled)
            .ToListAsync();

        var expenses = await _db.Expenses
            .Where(e => e.ExpenseDate >= from && e.ExpenseDate <= to)
            .SumAsync(e => e.Amount);

        var totalRevenue = orders.Sum(o => o.SubTotal);
        var totalVat = orders.Sum(o => o.VatAmount);
        var totalDelivery = orders.Sum(o => o.DeliveryFee);
        var totalDiscount = orders.Sum(o => o.DiscountAmount);
        var grossRevenue = totalRevenue + totalVat + totalDelivery - totalDiscount;
        var netProfit = grossRevenue - expenses;

        var summary = new DashboardSummaryDto
        {
            TotalRevenue = grossRevenue,
            TotalVat = totalVat,
            TotalExpenses = expenses,
            NetProfit = netProfit,
            DeliveryFeesRevenue = totalDelivery,
            TotalOrders = orders.Count,
            TotalAppointments = await _db.Appointments.CountAsync(a => a.CreatedAt >= from && a.CreatedAt <= to),
            PendingAppointments = await _db.Appointments.CountAsync(a => a.Status == AppointmentStatus.UnderReview),
            ActiveAgents = await _db.Agents.CountAsync(a => a.IsActive),
            TotalCustomers = await _db.Users.CountAsync(u => u.IsActive)
        };

        return Ok(ApiResponse<DashboardSummaryDto>.Ok(summary));
    }

    // تقرير الإيرادات
    [HttpGet("revenue")]
    public async Task<IActionResult> GetRevenueReport([FromQuery] string period = "month")
    {
        var (from, to) = GetDateRange(period);

        var orders = await _db.Orders
            .Where(o => o.CreatedAt >= from && o.CreatedAt <= to && o.Status != OrderStatus.Cancelled)
            .ToListAsync();

        var expenses = await _db.Expenses
            .Where(e => e.ExpenseDate >= from && e.ExpenseDate <= to)
            .SumAsync(e => e.Amount);

        var subTotal = orders.Sum(o => o.SubTotal);
        var vatAmount = orders.Sum(o => o.VatAmount);
        var delivery = orders.Sum(o => o.DeliveryFee);
        var discounts = orders.Sum(o => o.DiscountAmount);
        var totalRevenue = subTotal + vatAmount + delivery - discounts;

        // Chart data by day/week/month
        var chartData = await GetChartData(from, to, period);

        var report = new RevenueReportDto
        {
            Period = period,
            SubTotal = subTotal,
            VatAmount = vatAmount,
            DeliveryFees = delivery,
            Discounts = discounts,
            TotalRevenue = totalRevenue,
            TotalExpenses = expenses,
            NetProfit = totalRevenue - expenses,
            OrdersCount = orders.Count,
            ChartData = chartData
        };

        return Ok(ApiResponse<RevenueReportDto>.Ok(report));
    }

    // تقرير الضريبة (VAT)
    [HttpGet("vat-report")]
    public async Task<IActionResult> GetVatReport([FromQuery] int month, [FromQuery] int year)
    {
        var from = new DateTime(year, month, 1);
        var to = from.AddMonths(1).AddDays(-1);

        var invoices = await _db.Invoices
            .Include(i => i.Order).ThenInclude(o => o.User)
            .Where(i => i.InvoiceDate >= from && i.InvoiceDate <= to && i.Status == InvoiceStatus.Active)
            .OrderBy(i => i.InvoiceDate)
            .ToListAsync();

        var report = new VatReportDto
        {
            Period = $"{month}/{year}",
            TaxableAmount = invoices.Sum(i => i.SubTotal),
            VatAmount = invoices.Sum(i => i.VatAmount),
            InvoicesCount = invoices.Count,
            Items = invoices.Select(i => new VatReportItem
            {
                InvoiceNumber = i.InvoiceNumber,
                CustomerName = i.Order.User.FullName,
                Date = i.InvoiceDate,
                SubTotal = i.SubTotal,
                VatAmount = i.VatAmount,
                Total = i.TotalAmount
            }).ToList()
        };

        return Ok(ApiResponse<VatReportDto>.Ok(report));
    }

    // أداء المندوبين
    [HttpGet("agents-performance")]
    public async Task<IActionResult> GetAgentsPerformance([FromQuery] string period = "month")
    {
        var (from, to) = GetDateRange(period);

        var agents = await _db.Agents.Where(a => a.IsActive).ToListAsync();
        var result = new List<AgentPerformanceDto>();

        foreach (var agent in agents)
        {
            var appointments = await _db.Appointments
                .Where(a => a.AgentId == agent.Id && a.CreatedAt >= from && a.CreatedAt <= to)
                .ToListAsync();

            var payout = await _db.Transactions
                .Where(t => t.AgentId == agent.Id && t.Type == TransactionType.AgentPayout && t.CreatedAt >= from && t.CreatedAt <= to)
                .SumAsync(t => t.Amount);

            result.Add(new AgentPerformanceDto
            {
                AgentId = agent.Id,
                AgentName = agent.FullName,
                TotalAppointments = appointments.Count,
                CompletedAppointments = appointments.Count(a => a.Status == AppointmentStatus.Completed),
                AverageRating = agent.AverageRating,
                TotalPayout = payout
            });
        }

        return Ok(ApiResponse<List<AgentPerformanceDto>>.Ok(result));
    }

    // المنتجات الأكثر مبيعاً
    [HttpGet("top-products")]
    public async Task<IActionResult> GetTopProducts([FromQuery] string period = "month", [FromQuery] int top = 10)
    {
        var (from, to) = GetDateRange(period);

        var topProducts = await _db.OrderItems
            .Include(i => i.Product)
            .Include(i => i.Order)
            .Where(i => i.Order.CreatedAt >= from && i.Order.CreatedAt <= to && i.Order.Status != OrderStatus.Cancelled)
            .GroupBy(i => new { i.ProductId, i.Product.Name, i.Product.ImageUrl })
            .Select(g => new TopProductDto
            {
                ProductId = g.Key.ProductId,
                ProductName = g.Key.Name,
                ImageUrl = g.Key.ImageUrl,
                TotalSold = g.Sum(i => i.Quantity),
                TotalRevenue = g.Sum(i => i.TotalPrice)
            })
            .OrderByDescending(p => p.TotalSold)
            .Take(top)
            .ToListAsync();

        return Ok(ApiResponse<List<TopProductDto>>.Ok(topProducts));
    }

    private static (DateTime from, DateTime to) GetDateRange(string period) => period switch
    {
        "today" => (DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(1)),
        "week" => (DateTime.UtcNow.AddDays(-7), DateTime.UtcNow),
        "month" => (new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1), DateTime.UtcNow),
        "year" => (new DateTime(DateTime.UtcNow.Year, 1, 1), DateTime.UtcNow),
        _ => (new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1), DateTime.UtcNow)
    };

    private async Task<List<RevenueChartPoint>> GetChartData(DateTime from, DateTime to, string period)
    {
        var orders = await _db.Orders
            .Where(o => o.CreatedAt >= from && o.CreatedAt <= to && o.Status != OrderStatus.Cancelled)
            .ToListAsync();

        var expenses = await _db.Expenses
            .Where(e => e.ExpenseDate >= from && e.ExpenseDate <= to)
            .ToListAsync();

        if (period == "year")
        {
            return Enumerable.Range(1, 12).Select(month => new RevenueChartPoint
            {
                Label = new DateTime(from.Year, month, 1).ToString("MMM"),
                Revenue = orders.Where(o => o.CreatedAt.Month == month).Sum(o => o.TotalAmount),
                Expenses = expenses.Where(e => e.ExpenseDate.Month == month).Sum(e => e.Amount),
                Profit = orders.Where(o => o.CreatedAt.Month == month).Sum(o => o.TotalAmount)
                       - expenses.Where(e => e.ExpenseDate.Month == month).Sum(e => e.Amount)
            }).ToList();
        }

        return Enumerable.Range(0, (to - from).Days + 1).Select(day =>
        {
            var date = from.AddDays(day);
            var rev = orders.Where(o => o.CreatedAt.Date == date.Date).Sum(o => o.TotalAmount);
            var exp = expenses.Where(e => e.ExpenseDate.Date == date.Date).Sum(e => e.Amount);
            return new RevenueChartPoint { Label = date.ToString("dd/MM"), Revenue = rev, Expenses = exp, Profit = rev - exp };
        }).ToList();
    }
}
