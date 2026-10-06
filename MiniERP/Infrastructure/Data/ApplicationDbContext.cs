using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Text.Json;

namespace Infrastructure.Data;

public class ApplicationDbContext : DbContext, IApplicationDbContext
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
    public DbSet<Warehouse> Warehouses { get; set; }
    public DbSet<InventoryBalance> InventoryBalances { get; set; }
    public DbSet<InventoryLot> InventoryLots { get; set; }
    public DbSet<InventoryTransaction> InventoryTransactions { get; set; }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        OnBeforeSaveChanges();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void OnBeforeSaveChanges()
    {
        ChangeTracker.DetectChanges();
        var auditEntries = new List<AuditLog>();

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is InventoryTransaction && entry.State is EntityState.Modified or EntityState.Deleted)
                throw new InvalidOperationException("Sổ giao dịch kho là bất biến; hãy tạo giao dịch đảo thay vì sửa hoặc xóa.");

            // Bỏ qua bảng AuditLog hoặc các Entity không có thay đổi
            if (entry.Entity is AuditLog || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                continue;

            var oldValues = new Dictionary<string, object?>();
            var newValues = new Dictionary<string, object?>();
            string tableName = entry.Entity.GetType().Name;
            int? recordId = null;

            foreach (var property in entry.Properties)
            {
                // 💡 Đã xóa dòng if(property.IsMetadataProperty) bị lỗi ở đây
                string propertyName = property.Metadata.Name;

                // Lấy ID/Primary Key của bản ghi nếu có
                if (property.Metadata.IsPrimaryKey() && property.CurrentValue != null)
                {
                    if (int.TryParse(property.CurrentValue.ToString(), out int id))
                    {
                        recordId = id;
                    }
                }

                // Tách giá trị Cũ & Mới dựa trên hành động
                switch (entry.State)
                {
                    case EntityState.Added:
                        newValues[propertyName] = property.CurrentValue;
                        break;

                    case EntityState.Deleted:
                        oldValues[propertyName] = property.OriginalValue;
                        break;

                    case EntityState.Modified:
                        if (property.IsModified)
                        {
                            oldValues[propertyName] = property.OriginalValue;
                            newValues[propertyName] = property.CurrentValue;
                        }
                        break;
                }
            }

            var log = new AuditLog
            {
                Action = entry.State switch
                {
                    EntityState.Added => $"CREATE_{tableName.ToUpper()}",
                    EntityState.Modified => $"UPDATE_{tableName.ToUpper()}",
                    EntityState.Deleted => $"DELETE_{tableName.ToUpper()}",
                    _ => entry.State.ToString().ToUpper()
                },
                TableName = tableName,
                RecordId = recordId,
                OldValues = oldValues.Count == 0 ? null : JsonSerializer.Serialize(oldValues),
                NewValues = newValues.Count == 0 ? null : JsonSerializer.Serialize(newValues),
                Timestamp = DateTime.UtcNow
            };

            auditEntries.Add(log);
        }

        if (auditEntries.Any())
        {
            AuditLogs.AddRange(auditEntries);
        }
    }

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

        modelBuilder.Entity<InventoryLot>().Property(x => x.UnitCost).HasPrecision(18, 2);
        modelBuilder.Entity<InventoryTransaction>().Property(x => x.UnitCost).HasPrecision(18, 2);
        modelBuilder.Entity<InventoryBalance>().Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<InventoryLot>().Property(x => x.RowVersion).IsRowVersion();

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

        modelBuilder.Entity<Warehouse>().HasIndex(x => x.Code).IsUnique();
        modelBuilder.Entity<InventoryBalance>().HasIndex(x => new { x.WarehouseId, x.ProductId }).IsUnique();
        modelBuilder.Entity<InventoryLot>().HasIndex(x => new { x.WarehouseId, x.ProductId, x.LotNumber }).IsUnique();
        modelBuilder.Entity<InventoryTransaction>().HasIndex(x => new { x.WarehouseId, x.ProductId, x.CreatedAt });

        modelBuilder.Entity<InventoryBalance>().HasOne(x => x.Warehouse).WithMany(x => x.InventoryBalances).HasForeignKey(x => x.WarehouseId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<InventoryBalance>().HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<InventoryLot>().HasOne(x => x.Warehouse).WithMany().HasForeignKey(x => x.WarehouseId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<InventoryLot>().HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<InventoryTransaction>().HasOne(x => x.Warehouse).WithMany().HasForeignKey(x => x.WarehouseId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<InventoryTransaction>().HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<InventoryTransaction>().HasOne(x => x.InventoryLot).WithMany().HasForeignKey(x => x.InventoryLotId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Order>()
            .HasOne(x => x.CreatedByUser)
            .WithMany(x => x.Orders)
            .HasForeignKey(x => x.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // 3. Seed dữ liệu mẫu ban đầu cho Vai trò (Roles)
        modelBuilder.Entity<Role>().HasData(
            new Role { RoleId = 1, RoleName = "Admin", Description = "Quản trị viên hệ thống" },
            new Role { RoleId = 2, RoleName = "InventoryManager", Description = "Quản lý kho" },
            new Role { RoleId = 3, RoleName = "Sales", Description = "Nhân viên kinh doanh" }
        );

        modelBuilder.Entity<Warehouse>().HasData(new Warehouse { WarehouseId = 1, Code = "MAIN", Name = "Kho chính", IsActive = true });
    }
}
