using DreamsPools.API.Data;
using DreamsPools.API.Helpers;
using DreamsPools.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DreamsPools.API.Controllers;

// ===================== AGENTS =====================
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class AgentsController : ControllerBase
{
    private readonly AppDbContext _db;
    public AgentsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var agents = await _db.Agents
            .Where(a => !a.IsDeleted)
            .Select(a => new
            {
                a.Id, a.FullName, a.Email, a.PhoneNumber,
                a.IsActive, a.IsAvailable, a.AverageRating, a.TotalAppointments
            })
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(agents));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAgentDto dto)
    {
        if (await _db.Agents.AnyAsync(a => a.Email == dto.Email))
            return BadRequest(ApiResponse<string>.Fail("البريد الإلكتروني مستخدم مسبقاً"));

        var agent = new Agent
        {
            FullName = dto.FullName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };

        _db.Agents.Add(agent);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(new { agent.Id }, "تم إضافة المندوب"));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateAgentDto dto)
    {
        var agent = await _db.Agents.FindAsync(id);
        if (agent == null) return NotFound();

        agent.FullName = dto.FullName;
        agent.PhoneNumber = dto.PhoneNumber;
        agent.IsActive = dto.IsActive;

        if (!string.IsNullOrEmpty(dto.NewPassword))
            agent.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

        await _db.SaveChangesAsync();
        return Ok(ApiResponse<string>.Ok("تم التحديث"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var agent = await _db.Agents.FindAsync(id);
        if (agent == null) return NotFound();
        agent.IsDeleted = true;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<string>.Ok("تم الحذف"));
    }
}

public class CreateAgentDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class UpdateAgentDto
{
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? NewPassword { get; set; }
}

// ===================== EXPENSES =====================
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class ExpensesController : ControllerBase
{
    private readonly AppDbContext _db;
    public ExpensesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? period)
    {
        var query = _db.Expenses.Include(e => e.Admin).AsQueryable();

        if (period == "month")
        {
            var start = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            query = query.Where(e => e.ExpenseDate >= start);
        }

        var expenses = await query.OrderByDescending(e => e.ExpenseDate).ToListAsync();
        var total = expenses.Sum(e => e.Amount);

        return Ok(ApiResponse<object>.Ok(new
        {
            Total = total,
            Items = expenses.Select(e => new
            {
                e.Id, e.Title, e.Description, e.Amount, e.Category,
                CategoryText = e.Category switch
                {
                    ExpenseCategory.AgentSalary => "رواتب المندوبين",
                    ExpenseCategory.Marketing => "تسويق",
                    ExpenseCategory.Maintenance => "صيانة",
                    ExpenseCategory.Supplies => "مستلزمات",
                    _ => "أخرى"
                },
                e.ExpenseDate,
                AddedBy = e.Admin.FullName
            })
        }));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateExpenseDto dto)
    {
        var adminId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var expense = new Expense
        {
            Title = dto.Title,
            Description = dto.Description,
            Amount = dto.Amount,
            Category = dto.Category,
            ExpenseDate = dto.ExpenseDate,
            AdminId = adminId
        };

        _db.Expenses.Add(expense);
        await _db.SaveChangesAsync();

        // Record as transaction
        var transaction = new Transaction
        {
            Type = TransactionType.Expense,
            Amount = dto.Amount,
            VatAmount = 0,
            Description = dto.Title,
            TransactionNumber = "TEMP"
        };
        _db.Transactions.Add(transaction);
        await _db.SaveChangesAsync();
        transaction.TransactionNumber = NumberGenerator.GenerateTransactionNumber(transaction.Id);
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new { expense.Id }, "تم إضافة المصروف"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var expense = await _db.Expenses.FindAsync(id);
        if (expense == null) return NotFound();
        _db.Expenses.Remove(expense);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<string>.Ok("تم الحذف"));
    }
}

public class CreateExpenseDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public ExpenseCategory Category { get; set; }
    public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;
}

// ===================== NOTIFICATIONS =====================
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly AppDbContext _db;
    public NotificationsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetMyNotifications()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var role = User.FindFirstValue(ClaimTypes.Role);

        var query = _db.Notifications.AsQueryable();

        if (role == "Customer") query = query.Where(n => n.UserId == userId);
        else if (role == "Agent") query = query.Where(n => n.AgentId == userId);

        var notifications = await query
            .OrderByDescending(n => n.CreatedAt)
            .Take(50)
            .Select(n => new { n.Id, n.Title, n.Body, n.Type, n.IsRead, n.CreatedAt, n.Data })
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(notifications));
    }

    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var notification = await _db.Notifications.FindAsync(id);
        if (notification == null) return NotFound();
        notification.IsRead = true;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<string>.Ok("تم التحديث"));
    }
}

// ===================== APP SETTINGS =====================
[ApiController]
[Route("api/[controller]")]
public class SettingsController : ControllerBase
{
    private readonly AppDbContext _db;
    public SettingsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var settings = await _db.AppSettings
            .Select(s => new { s.Key, s.Value, s.Description })
            .ToListAsync();
        return Ok(ApiResponse<object>.Ok(settings));
    }

    [HttpPut("{key}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Update(string key, [FromBody] string value)
    {
        var setting = await _db.AppSettings.FirstOrDefaultAsync(s => s.Key == key);
        if (setting == null) return NotFound();
        setting.Value = value;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<string>.Ok("تم التحديث"));
    }
}
