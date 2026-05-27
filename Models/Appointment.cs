namespace DreamsPools.API.Models;

public class Appointment : BaseEntity
{
    public string AppointmentNumber { get; set; } = string.Empty; // DP-APT-2024-0001
    public string CustomerName { get; set; } = string.Empty;    // يُجلب تلقائياً ويمكن تعديله
    public string CustomerPhone { get; set; } = string.Empty;   // يُجلب تلقائياً ويمكن تعديله
    public string ProblemDescription { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;         // اسم الحي
    public string? Street { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.UnderReview;
    public DateTime? ScheduledDate { get; set; }  // يُحدد من لوحة التحكم
    public string? ScheduledTime { get; set; }    // يُحدد من لوحة التحكم
    public string? CancellationReason { get; set; }
    public string? AdminNotes { get; set; }

    // Foreign Keys
    public int UserId { get; set; }
    public int? AgentId { get; set; }

    // Navigation
    public User User { get; set; } = null!;
    public Agent? Agent { get; set; }
    public Rating? Rating { get; set; }
}

public enum AppointmentStatus
{
    UnderReview = 1,   // تحت المراجعة
    Accepted = 2,      // تم القبول
    OnTheWay = 3,      // في الطريق إليك
    Completed = 4,     // مكتمل
    Cancelled = 5      // ملغي
}
