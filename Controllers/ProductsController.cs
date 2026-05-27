using DreamsPools.API.Data;
using DreamsPools.API.Helpers;
using DreamsPools.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DreamsPools.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _db;
    public ProductsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? categoryId,
        [FromQuery] string? search,
        [FromQuery] bool? featured,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = _db.Products
            .Include(p => p.Category)
            .Where(p => p.IsActive)
            .AsQueryable();

        if (categoryId.HasValue) query = query.Where(p => p.CategoryId == categoryId.Value);
        if (!string.IsNullOrEmpty(search)) query = query.Where(p => p.Name.Contains(search));
        if (featured.HasValue) query = query.Where(p => p.IsFeatured == featured.Value);

        var total = await query.CountAsync();
        var products = await query
            .OrderByDescending(p => p.IsFeatured)
            .ThenByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new
            {
                p.Id, p.Name, p.Description, p.Price,
                p.VatAmount, p.PriceWithVat,
                p.ImageUrl, p.StockQuantity, p.IsFeatured, p.AverageRating,
                CategoryName = p.Category.Name
            })
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(products, total: total));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _db.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

        if (product == null) return NotFound(ApiResponse<string>.Fail("المنتج غير موجود"));

        return Ok(ApiResponse<object>.Ok(new
        {
            product.Id, product.Name, product.Description,
            product.Price, product.VatAmount, product.PriceWithVat,
            product.ImageUrl, product.StockQuantity, product.AverageRating,
            CategoryName = product.Category.Name
        }));
    }

    // Admin CRUD
    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
    {
        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            StockQuantity = dto.StockQuantity,
            CategoryId = dto.CategoryId,
            IsFeatured = dto.IsFeatured,
            ImageUrl = dto.ImageUrl
        };
        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(new { product.Id }, "تم إضافة المنتج"));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateProductDto dto)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null) return NotFound();

        product.Name = dto.Name;
        product.Description = dto.Description;
        product.Price = dto.Price;
        product.StockQuantity = dto.StockQuantity;
        product.CategoryId = dto.CategoryId;
        product.IsFeatured = dto.IsFeatured;
        product.ImageUrl = dto.ImageUrl;

        await _db.SaveChangesAsync();
        return Ok(ApiResponse<string>.Ok("تم التحديث"));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null) return NotFound();
        product.IsDeleted = true;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<string>.Ok("تم الحذف"));
    }
}

public class CreateProductDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public int CategoryId { get; set; }
    public bool IsFeatured { get; set; }
    public string? ImageUrl { get; set; }
}

// Categories
[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _db;
    public CategoriesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _db.Categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new { c.Id, c.Name, c.Description, c.ImageUrl })
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(categories));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
    {
        var cat = new Category { Name = dto.Name, Description = dto.Description, ImageUrl = dto.ImageUrl, DisplayOrder = dto.DisplayOrder };
        _db.Categories.Add(cat);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(new { cat.Id }, "تم إضافة التصنيف"));
    }

    // ── في نهاية CategoriesController أضف هذين الـ endpoints ──
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateCategoryDto dto)
    {
        var cat = await _db.Categories.FindAsync(id);
        if (cat == null) return NotFound(ApiResponse<string>.Fail("التصنيف غير موجود"));
        cat.Name = dto.Name;
        cat.Description = dto.Description;
        cat.ImageUrl = dto.ImageUrl;
        cat.DisplayOrder = dto.DisplayOrder;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<string>.Ok("تم التعديل"));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Delete(int id)
    {
        var cat = await _db.Categories.FindAsync(id);
        if (cat == null) return NotFound();
        cat.IsActive = false;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<string>.Ok("تم الحذف"));
    }

}

public class CreateCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
}