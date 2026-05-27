using DreamsPools.API.Models;

namespace DreamsPools.API.DTOs.Appointments;

public class CreateAppointmentDto
{
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string ProblemDescription { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string? Street { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public class AppointmentResponseDto
{
    public int Id { get; set; }
    public string AppointmentNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string ProblemDescription { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public AppointmentStatus Status { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public DateTime? ScheduledDate { get; set; }
    public string? ScheduledTime { get; set; }
    public string? AgentName { get; set; }
    public string? AgentPhone { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UpdateAppointmentStatusDto
{
    public AppointmentStatus Status { get; set; }
    public int? AgentId { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public string? ScheduledTime { get; set; }
    public string? CancellationReason { get; set; }
    public string? AdminNotes { get; set; }
}
