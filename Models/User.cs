namespace DreamsPools.API.Models;

public class User : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public string? FcmToken { get; set; } // Firebase push notifications

    // Navigation
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<Address> Addresses { get; set; } = new List<Address>();
    public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
