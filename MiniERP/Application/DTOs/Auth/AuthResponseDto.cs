namespace Application.DTOs.Auth;

public class AuthResponseDto
{
    
    public string Username { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty; // Chuỗi JWT Token
}