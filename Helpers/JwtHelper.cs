using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace DreamsPools.API.Helpers;

public class JwtHelper
{
    private readonly IConfiguration _config;

    public JwtHelper(IConfiguration config) => _config = config;

    public string GenerateToken(int id, string email, string role, string fullName)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, id.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, role),
            new Claim(ClaimTypes.Name, fullName)
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(30),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

// Helper for API responses
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public int? TotalCount { get; set; }

    public static ApiResponse<T> Ok(T data, string message = "تمت العملية بنجاح", int? total = null)
        => new() { Success = true, Message = message, Data = data, TotalCount = total };

    public static ApiResponse<T> Fail(string message)
        => new() { Success = false, Message = message };
}

// Number generator for orders, appointments, invoices
public static class NumberGenerator
{
    public static string GenerateOrderNumber(int id)
        => $"DP-{DateTime.UtcNow.Year}-{id:D5}";

    public static string GenerateAppointmentNumber(int id)
        => $"DP-APT-{DateTime.UtcNow.Year}-{id:D5}";

    public static string GenerateInvoiceNumber(int id)
        => $"DP-INV-{DateTime.UtcNow.Year}-{id:D5}";

    public static string GenerateTransactionNumber(int id)
        => $"DP-TRX-{DateTime.UtcNow.Year}-{id:D5}";
}
