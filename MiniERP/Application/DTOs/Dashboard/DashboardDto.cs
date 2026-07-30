using Application.DTOs.Order;
using Application.DTOs.Product;

namespace Application.DTOs.Dashboard;

public class DashboardDto
{
    public decimal TotalRevenue { get; set; }
    public int TotalOrders { get; set; }
    public int TotalProducts { get; set; }
    public int LowStockProductsCount { get; set; }
    public List<ProductDto> LowStockProducts { get; set; } = new();
    public List<OrderDto> RecentOrders { get; set; } = new();
}