using DreamsPools.API.Data;
using DreamsPools.API.DTOs.Auth;
using DreamsPools.API.Helpers;
using DreamsPools.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DreamsPools.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly JwtHelper _jwt;

    public AuthController(AppDbContext db, JwtHelper jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    // ===================== CUSTOMER =====================
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
            return BadRequest(ApiResponse<string>.Fail("البريد الإلكتروني مستخدم مسبقاً"));

        var user = new User
        {
            FullName = dto.FullName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var token = _jwt.GenerateToken(user.Id, user.Email, "Customer", user.FullName);
        return Ok(ApiResponse<AuthResponseDto>.Ok(new AuthResponseDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = "Customer",
            Token = token
        }, "تم إنشاء الحساب بنجاح"));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email && u.IsActive);
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Unauthorized(ApiResponse<string>.Fail("البريد الإلكتروني أو كلمة المرور غير صحيحة"));

        var token = _jwt.GenerateToken(user.Id, user.Email, "Customer", user.FullName);
        return Ok(ApiResponse<AuthResponseDto>.Ok(new AuthResponseDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = "Customer",
            Token = token
        }));
    }

    // ===================== AGENT =====================
    [HttpPost("agent/login")]
    public async Task<IActionResult> AgentLogin(AgentLoginDto dto)
    {
        var agent = await _db.Agents.FirstOrDefaultAsync(a => a.Email == dto.Email && a.IsActive);
        if (agent == null || !BCrypt.Net.BCrypt.Verify(dto.Password, agent.PasswordHash))
            return Unauthorized(ApiResponse<string>.Fail("البريد الإلكتروني أو كلمة المرور غير صحيحة"));

        var token = _jwt.GenerateToken(agent.Id, agent.Email, "Agent", agent.FullName);
        return Ok(ApiResponse<AuthResponseDto>.Ok(new AuthResponseDto
        {
            Id = agent.Id,
            FullName = agent.FullName,
            Email = agent.Email,
            Role = "Agent",
            Token = token
        }));
    }

    // ===================== ADMIN =====================
    [HttpPost("admin/login")]
    public async Task<IActionResult> AdminLogin(AdminLoginDto dto)
    {
        var admin = await _db.Admins.FirstOrDefaultAsync(a => a.Email == dto.Email && a.IsActive);
        if (admin == null || !BCrypt.Net.BCrypt.Verify(dto.Password, admin.PasswordHash))
            return Unauthorized(ApiResponse<string>.Fail("البريد الإلكتروني أو كلمة المرور غير صحيحة"));

        var token = _jwt.GenerateToken(admin.Id, admin.Email, admin.Role.ToString(), admin.FullName);
        return Ok(ApiResponse<AuthResponseDto>.Ok(new AuthResponseDto
        {
            Id = admin.Id,
            FullName = admin.FullName,
            Email = admin.Email,
            Role = admin.Role.ToString(),
            Token = token
        }));
    }

    // Update FCM Token
    [Authorize]
    [HttpPut("fcm-token")]
    public async Task<IActionResult> UpdateFcmToken(UpdateFcmTokenDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var role = User.FindFirstValue(ClaimTypes.Role);

        if (role == "Customer")
        {
            var user = await _db.Users.FindAsync(userId);
            if (user != null) { user.FcmToken = dto.FcmToken; await _db.SaveChangesAsync(); }
        }
        else if (role == "Agent")
        {
            var agent = await _db.Agents.FindAsync(userId);
            if (agent != null) { agent.FcmToken = dto.FcmToken; await _db.SaveChangesAsync(); }
        }

        return Ok(ApiResponse<string>.Ok("تم التحديث"));
    }
}
