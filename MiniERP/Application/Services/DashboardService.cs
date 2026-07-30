using Application.DTOs.Dashboard;
using Application.DTOs.Order;
using Application.DTOs.Product;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IApplicationDbContext _context;

    public DashboardService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardDto> GetDashboardStatsAsync()
    {
        // 1. Tính tổng doanh thu (Không tính các đơn bị CANCELLED)
        var totalRevenue = await _context.Orders
            .Where(o => o.Status != "CANCELLED")
            .SumAsync(o => o.TotalAmount);

        // 2. Tổng số đơn hàng & tổng sản phẩm
        var totalOrders = await _context.Orders.CountAsync();
        var totalProducts = await _context.Products.CountAsync();

        // 3. Sản phẩm cảnh báo tồn kho thấp (<= 10)
        var lowStockProducts = await _context.Products
            .Include(p => p.Category)
            .Where(p => p.StockQuantity <= 10)
            .Select(p => new ProductDto
            {
                ProductId = p.ProductId,
                SKU = p.SKU,
                ProductName = p.ProductName,
                UnitPrice = p.UnitPrice,
                StockQuantity = p.StockQuantity,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.CategoryName
            })
            .ToListAsync();

        // 4. Lấy 5 đơn hàng mới nhất
        var recentOrders = await _context.Orders
            .Include(o => o.CreatedByUser)
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
            .OrderByDescending(o => o.OrderDate)
            .Take(5)
            .Select(o => new OrderDto
            {
                OrderId = o.OrderId,
                OrderCode = o.OrderCode,
                OrderDate = o.OrderDate,
                CustomerName = o.CustomerName,
                CustomerPhone = o.CustomerPhone,
                Status = o.Status,
                TotalAmount = o.TotalAmount,
                CreatedByUsername = o.CreatedByUser.Username,
                Details = o.OrderDetails.Select(od => new OrderDetailDto
                {
                    OrderDetailId = od.OrderDetailId,
                    ProductId = od.ProductId,
                    ProductName = od.Product.ProductName,
                    Quantity = od.Quantity,
                    UnitPrice = od.UnitPrice,
                    SubTotal = od.SubTotal
                }).ToList()
            })
            .ToListAsync();

        return new DashboardDto
        {
            TotalRevenue = totalRevenue,
            TotalOrders = totalOrders,
            TotalProducts = totalProducts,
            LowStockProductsCount = lowStockProducts.Count,
            LowStockProducts = lowStockProducts,
            RecentOrders = recentOrders
        };
    }
}