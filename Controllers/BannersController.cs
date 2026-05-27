using System.Threading.Tasks;
using DreamsPools.API.Data;
using DreamsPools.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DreamsPools.API.Helpers; // 💡 استبدل Helpers باسم المجلد الصحيح لديك إذا كان مختلفاً
namespace DreamsPools.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BannersController : ControllerBase
{
    private readonly AppDbContext _db;
    public BannersController(AppDbContext db) => _db = db;

    // العميل يشوف البانرات الظاهرة فقط
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var banners = await _db.Banners
            .Where(b => b.IsVisible && !b.IsDeleted)
            .OrderBy(b => b.DisplayOrder)
            .Select(b => new {
                b.Id,
                b.Title,
                b.ImageUrl,
                b.Type,
                b.ProductId,
                b.IsVisible
            })
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(banners));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Create([FromBody] BannerDto dto)
    {
        var banner = new Banner
        {
            Title = dto.Title,
            ImageUrl = dto.ImageUrl,
            Type = dto.Type,
            ProductId = dto.ProductId,
            IsVisible = dto.IsVisible,
            DisplayOrder = dto.DisplayOrder
        };
        _db.Banners.Add(banner);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(new { banner.Id }, "تم إضافة الإعلان"));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Update(int id, [FromBody] BannerDto dto)
    {
        var banner = await _db.Banners.FindAsync(id);
        if (banner == null) return NotFound();
        banner.Title = dto.Title; banner.ImageUrl = dto.ImageUrl;
        banner.Type = dto.Type; banner.ProductId = dto.ProductId;
        banner.IsVisible = dto.IsVisible; banner.DisplayOrder = dto.DisplayOrder;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<string>.Ok("تم التعديل"));
    }

    [HttpPut("{id}/visibility")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> ToggleVisibility(int id, [FromBody] VisibilityDto dto)
    {
        var banner = await _db.Banners.FindAsync(id);
        if (banner == null) return NotFound();
        banner.IsVisible = dto.IsVisible;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<string>.Ok(dto.IsVisible ? "تم الإظهار" : "تم الإخفاء"));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Delete(int id)
    {
        var banner = await _db.Banners.FindAsync(id);
        if (banner == null) return NotFound();
        banner.IsDeleted = true;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<string>.Ok("تم الحذف"));
    }
}

public class BannerDto
{
    public string Title { get; set; } = "";
    public string? ImageUrl { get; set; }
    public int Type { get; set; }
    public int? ProductId { get; set; }
    public bool IsVisible { get; set; } = true;
    public int DisplayOrder { get; set; }
}

public class VisibilityDto
{
    public bool IsVisible { get; set; }
}