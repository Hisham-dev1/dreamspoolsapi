using DreamsPools.API.Data;
using DreamsPools.API.DTOs.Appointments;
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
public class AppointmentsController : ControllerBase
{
    private readonly AppDbContext _db;

    public AppointmentsController(AppDbContext db) => _db = db;

    // Customer: Book appointment
    [HttpPost]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> Create(CreateAppointmentDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _db.Users.FindAsync(userId);
        if (user == null) return Unauthorized();

        var appointment = new Appointment
        {
            UserId = userId,
            CustomerName = dto.CustomerName,
            CustomerPhone = dto.CustomerPhone,
            ProblemDescription = dto.ProblemDescription,
            District = dto.District,
            Street = dto.Street,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            Status = AppointmentStatus.UnderReview,
            AppointmentNumber = "TEMP" // Will update after save
        };

        _db.Appointments.Add(appointment);
        await _db.SaveChangesAsync();

        appointment.AppointmentNumber = NumberGenerator.GenerateAppointmentNumber(appointment.Id);
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<AppointmentResponseDto>.Ok(MapToDto(appointment, null), "تم إرسال طلب الموعد بنجاح"));
    }

    // Customer: Get my appointments
    [HttpGet("my")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> GetMyAppointments()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var appointments = await _db.Appointments
            .Include(a => a.Agent)
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        return Ok(ApiResponse<List<AppointmentResponseDto>>.Ok(
            appointments.Select(a => MapToDto(a, a.Agent)).ToList()));
    }

    // Customer: Get single appointment
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var role = User.FindFirstValue(ClaimTypes.Role);

        var appointment = await _db.Appointments
            .Include(a => a.Agent)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (appointment == null) return NotFound(ApiResponse<string>.Fail("الموعد غير موجود"));

        // Security: customer can only see their own
        if (role == "Customer" && appointment.UserId != userId)
            return Forbid();

        return Ok(ApiResponse<AppointmentResponseDto>.Ok(MapToDto(appointment, appointment.Agent)));
    }

    // Customer: Cancel appointment
    [HttpPut("{id}/cancel")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> Cancel(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var appointment = await _db.Appointments.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

        if (appointment == null) return NotFound(ApiResponse<string>.Fail("الموعد غير موجود"));
        if (appointment.Status != AppointmentStatus.UnderReview)
            return BadRequest(ApiResponse<string>.Fail("لا يمكن إلغاء الموعد بعد قبوله"));

        appointment.Status = AppointmentStatus.Cancelled;
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<string>.Ok("تم إلغاء الموعد"));
    }

    // Admin: Get all appointments
    [HttpGet]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> GetAll([FromQuery] AppointmentStatus? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var query = _db.Appointments
            .Include(a => a.User)
            .Include(a => a.Agent)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(a => a.Status == status.Value);

        var total = await query.CountAsync();
        var appointments = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(ApiResponse<List<AppointmentResponseDto>>.Ok(
            appointments.Select(a => MapToDto(a, a.Agent)).ToList(), total: total));
    }

    // Admin: Update appointment status (accept, assign agent, cancel...)
    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateAppointmentStatusDto dto)
    {
        var appointment = await _db.Appointments
            .Include(a => a.User)
            .Include(a => a.Agent)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (appointment == null) return NotFound(ApiResponse<string>.Fail("الموعد غير موجود"));

        appointment.Status = dto.Status;
        appointment.AdminNotes = dto.AdminNotes;

        if (dto.AgentId.HasValue)
            appointment.AgentId = dto.AgentId;

        if (dto.ScheduledDate.HasValue)
            appointment.ScheduledDate = dto.ScheduledDate;

        if (!string.IsNullOrEmpty(dto.ScheduledTime))
            appointment.ScheduledTime = dto.ScheduledTime;

        if (!string.IsNullOrEmpty(dto.CancellationReason))
            appointment.CancellationReason = dto.CancellationReason;

        await _db.SaveChangesAsync();

        // TODO: Send push notification to customer
        // await _notificationService.SendAsync(appointment.User.FcmToken, ...);

        return Ok(ApiResponse<AppointmentResponseDto>.Ok(MapToDto(appointment, appointment.Agent), "تم تحديث حالة الموعد"));
    }

    // Agent: Get my assigned appointments
    [HttpGet("agent/my")]
    [Authorize(Roles = "Agent")]
    public async Task<IActionResult> GetAgentAppointments()
    {
        var agentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var appointments = await _db.Appointments
            .Include(a => a.User)
            .Where(a => a.AgentId == agentId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        return Ok(ApiResponse<List<AppointmentResponseDto>>.Ok(
            appointments.Select(a => MapToDto(a, null)).ToList()));
    }

    // Agent: Update appointment status (on the way, completed)
    [HttpPut("{id}/agent-update")]
    [Authorize(Roles = "Agent")]
    public async Task<IActionResult> AgentUpdateStatus(int id, [FromBody] AppointmentStatus status)
    {
        var agentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var appointment = await _db.Appointments
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Id == id && a.AgentId == agentId);

        if (appointment == null) return NotFound(ApiResponse<string>.Fail("الموعد غير موجود"));

        // Agent can only move to OnTheWay or Completed
        if (status != AppointmentStatus.OnTheWay && status != AppointmentStatus.Completed)
            return BadRequest(ApiResponse<string>.Fail("لا يمكنك تغيير الحالة لهذه القيمة"));

        appointment.Status = status;
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<string>.Ok("تم تحديث الحالة"));
    }

    private static AppointmentResponseDto MapToDto(Appointment a, Agent? agent) => new()
    {
        Id = a.Id,
        AppointmentNumber = a.AppointmentNumber,
        CustomerName = a.CustomerName,
        CustomerPhone = a.CustomerPhone,
        ProblemDescription = a.ProblemDescription,
        District = a.District,
        Status = a.Status,
        StatusText = a.Status switch
        {
            AppointmentStatus.UnderReview => "تحت المراجعة",
            AppointmentStatus.Accepted => "تم القبول",
            AppointmentStatus.OnTheWay => "في الطريق إليك",
            AppointmentStatus.Completed => "مكتمل",
            AppointmentStatus.Cancelled => "ملغي",
            _ => ""
        },
        ScheduledDate = a.ScheduledDate,
        ScheduledTime = a.ScheduledTime,
        AgentName = agent?.FullName,
        AgentPhone = agent?.PhoneNumber,
        CancellationReason = a.CancellationReason,
        CreatedAt = a.CreatedAt
    };
}
