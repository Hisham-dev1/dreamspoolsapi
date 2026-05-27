namespace DreamsPools.API.Models;

public class Admin : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public AdminRole Role { get; set; } = AdminRole.Admin;
}

public enum AdminRole
{
    SuperAdmin = 1,
    Admin = 2
}
