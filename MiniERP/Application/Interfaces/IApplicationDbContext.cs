using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Role> Roles { get; set; }
    DbSet<User> Users { get; set; }
    DbSet<Category> Categories { get; set; }
    DbSet<Product> Products { get; set; }
    DbSet<Order> Orders { get; set; }
    DbSet<OrderDetail> OrderDetails { get; set; }
    DbSet<AuditLog> AuditLogs { get; set; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}