using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Security;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly IConfiguration _config;

    public JwtTokenGenerator(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateToken(User user)
    {
        var secretKey = _config["JwtSettings:Secret"]
            ?? "DEFAULT_SUPER_SECRET_KEY_FOR_MINIERP_SYSTEM_2026"; // Khóa dự phòng nếu thiếu config
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Đóng gói thông tin User vào Token (Claims)
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role?.RoleName ?? "Sales")
        };

        var token = new JwtSecurityToken(
            issuer: _config["JwtSettings:Issuer"] ?? "MiniERP",
            audience: _config["JwtSettings:Audience"] ?? "MiniERPUsers",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8), // Token có hiệu lực 8 tiếng
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}