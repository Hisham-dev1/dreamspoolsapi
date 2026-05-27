using System.Threading.Tasks;
using DreamsPools.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DreamsPools.API.Helpers; // 💡 تأكد من كتابة نفس السطر الذي نجح معك في ملف BannersController

namespace DreamsPools.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class CustomersController : ControllerBase
{
    private readonly AppDbContext _db;
    public CustomersController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var customers = await _db.Users
            .Select(u => new {
                u.Id,
                u.FullName,
                u.Email,
                u.PhoneNumber,
                u.IsActive,
                u.CreatedAt
            })
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(customers));
    }
}