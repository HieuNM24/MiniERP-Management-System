using Domain.Entities;

namespace Application.Interfaces;

public interface IAuditLogService
{
    Task<IEnumerable<AuditLog>> GetLogsAsync();
}