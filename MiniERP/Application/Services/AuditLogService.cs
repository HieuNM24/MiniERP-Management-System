using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class AuditLogService : IAuditLogService
{
    private readonly IApplicationDbContext _context;

    public AuditLogService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AuditLog>> GetLogsAsync()
    {
        return await _context.AuditLogs
            .OrderByDescending(a => a.Timestamp)
            .Take(100) // Lấy 100 nhật ký mới nhất
            .ToListAsync();
    }
}