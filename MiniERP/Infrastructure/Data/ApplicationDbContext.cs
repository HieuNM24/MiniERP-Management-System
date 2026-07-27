using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using System.Data;

namespace Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // Khai báo các Bảng (DbSet) đại diện cho Database
    public DbSet<Role> Roles { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderDetail> OrderDetails { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 1. Cấu hình kiểu dữ liệu DECIMAL để tránh cảnh báo warning của EF Core
        modelBuilder.Entity<Product>()
            .Property(p => p.UnitPrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Order>()
            .Property(o => o.TotalAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<OrderDetail>()
            .Property(od => od.UnitPrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<OrderDetail>()
            .Property(od => od.SubTotal)
            .HasPrecision(18, 2);

        // 2. Cấu hình Ràng buộc Khóa ngoại & Tên chỉ mục (Index) độc nhất
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Product>()
            .HasIndex(p => p.SKU)
            .IsUnique();

        modelBuilder.Entity<Order>()
            .HasIndex(o => o.OrderCode)
            .IsUnique();

        // 3. Seed dữ liệu mẫu ban đầu cho Vai trò (Roles)
        modelBuilder.Entity<Role>().HasData(
            new Role { RoleId = 1, RoleName = "Admin", Description = "Quản trị viên hệ thống" },
            new Role { RoleId = 2, RoleName = "InventoryManager", Description = "Quản lý kho" },
            new Role { RoleId = 3, RoleName = "Sales", Description = "Nhân viên kinh doanh" }
        );
    }
}