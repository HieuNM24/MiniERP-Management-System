using Application.DTOs.Auth;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class AuthService : IAuthService
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IPasswordHasher _passwordHasher; // <--- Inject Interface PasswordHasher

    public AuthService(
        IApplicationDbContext context,
        IJwtTokenGenerator jwtTokenGenerator,
        IPasswordHasher passwordHasher)
    {
        _context = context;
        _jwtTokenGenerator = jwtTokenGenerator;
        _passwordHasher = passwordHasher;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        // 1. Kiểm tra Username hoặc Email đã tồn tại chưa
        if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            throw new Exception("Tên đăng nhập đã tồn tại!");

        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            throw new Exception("Email đã được sử dụng!");

        // 2. Hash mật khẩu thông qua Interface (Không phụ thuộc BCrypt)
        string passwordHash = _passwordHasher.HashPassword(request.Password);

        // 3. Tạo User entity mới
        var user = new User
        {
            Username = request.Username,
            PasswordHash = passwordHash,
            FullName = request.FullName,
            Email = request.Email,
            RoleId = request.RoleId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleId == user.RoleId);

        // 4. Sinh JWT Token và trả về
        var token = _jwtTokenGenerator.GenerateToken(user);

        return new AuthResponseDto
        {
            Username = user.Username,
            RoleName = role?.RoleName ?? "Sales",
            Token = token
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        // 1. Tìm User theo Username
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Username == request.Username);

        if (user == null)
            throw new Exception("Tên đăng nhập hoặc mật khẩu không chính xác!");

        // 2. Kiểm tra mật khẩu thông qua Interface
        bool isPasswordValid = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash);
        if (!isPasswordValid)
            throw new Exception("Tên đăng nhập hoặc mật khẩu không chính xác!");

        if (!user.IsActive)
            throw new Exception("Tài khoản đã bị khóa!");

        // 3. Sinh JWT Token và trả về
        var token = _jwtTokenGenerator.GenerateToken(user);

        return new AuthResponseDto
        {
            Username = user.Username,
            RoleName = user.Role.RoleName,
            Token = token
        };
    }
}