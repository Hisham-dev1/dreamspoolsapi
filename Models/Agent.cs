namespace DreamsPools.API.Models;

public class Agent : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsAvailable { get; set; } = true;
    public string? FcmToken { get; set; }
    public double AverageRating { get; set; } = 0;
    public int TotalAppointments { get; set; } = 0;

    // Navigation
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
}
