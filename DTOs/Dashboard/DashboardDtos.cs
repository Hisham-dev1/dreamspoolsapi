namespace DreamsPools.API.DTOs.Dashboard;

public class DashboardSummaryDto
{
    public decimal TotalRevenue { get; set; }
    public decimal TotalVat { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal NetProfit { get; set; }
    public decimal DeliveryFeesRevenue { get; set; }
    public int TotalOrders { get; set; }
    public int TotalAppointments { get; set; }
    public int PendingAppointments { get; set; }
    public int ActiveAgents { get; set; }
    public int TotalCustomers { get; set; }
}

public class RevenueReportDto
{
    public string Period { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal VatAmount { get; set; }
    public decimal DeliveryFees { get; set; }
    public decimal Discounts { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal NetProfit { get; set; }
    public int OrdersCount { get; set; }
    public List<RevenueChartPoint> ChartData { get; set; } = new();
}

public class RevenueChartPoint
{
    public string Label { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public decimal Expenses { get; set; }
    public decimal Profit { get; set; }
}

public class AgentPerformanceDto
{
    public int AgentId { get; set; }
    public string AgentName { get; set; } = string.Empty;
    public int TotalAppointments { get; set; }
    public int CompletedAppointments { get; set; }
    public double AverageRating { get; set; }
    public decimal TotalPayout { get; set; }
}

public class TopProductDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int TotalSold { get; set; }
    public decimal TotalRevenue { get; set; }
}

public class VatReportDto
{
    public string Period { get; set; } = string.Empty;
    public decimal TaxableAmount { get; set; }
    public decimal VatAmount { get; set; }
    public int InvoicesCount { get; set; }
    public List<VatReportItem> Items { get; set; } = new();
}

public class VatReportItem
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public decimal SubTotal { get; set; }
    public decimal VatAmount { get; set; }
    public decimal Total { get; set; }
}
