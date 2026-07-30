using Application.DTOs.Order;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class OrderService : IOrderService
{
    private readonly IApplicationDbContext _context;

    public OrderService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OrderDto> CreateOrderAsync(CreateOrderDto dto, int userId)
    {
        if (dto.Items == null || !dto.Items.Any())
            throw new Exception("Đơn hàng phải chứa ít nhất 1 sản phẩm!");

        // 1. Sinh mã đơn hàng tự động (VD: ORD-20260730-XXXX)
        string orderCode = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..4].ToUpper()}";

        var order = new Order
        {
            OrderCode = orderCode,
            OrderDate = DateTime.UtcNow,
            CustomerName = dto.CustomerName,
            CustomerPhone = dto.CustomerPhone,
            Status = "PENDING",
            CreatedBy = userId, // Khóa ngoại trỏ đến User
            CreatedAt = DateTime.UtcNow,
            TotalAmount = 0,
            OrderDetails = new List<OrderDetail>()
        };

        decimal calculatedTotal = 0;

        // 2. Duyệt từng mặt hàng trong đơn
        foreach (var item in dto.Items)
        {
            var product = await _context.Products.FindAsync(item.ProductId);
            if (product == null)
                throw new Exception($"Không tìm thấy sản phẩm có ID: {item.ProductId}");

            // ⚠️ Kiểm tra số lượng tồn kho
            if (product.StockQuantity < item.Quantity)
                throw new Exception($"Sản phẩm '{product.ProductName}' chỉ còn {product.StockQuantity} trong kho (bạn đặt {item.Quantity})!");

            // 📉 Trừ số lượng tồn kho
            product.StockQuantity -= item.Quantity;

            // 💰 Tính tiền theo giá trong Database
            decimal lineTotal = product.UnitPrice * item.Quantity;
            calculatedTotal += lineTotal;

            order.OrderDetails.Add(new OrderDetail
            {
                ProductId = product.ProductId,
                Quantity = item.Quantity,
                UnitPrice = product.UnitPrice,
                SubTotal = lineTotal // Khớp với thuộc tính SubTotal
            });
        }

        order.TotalAmount = calculatedTotal;

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(order.OrderId)
            ?? throw new Exception("Có lỗi xảy ra khi tạo đơn hàng!");
    }

    public async Task<IEnumerable<OrderDto>> GetAllAsync()
    {
        return await _context.Orders
            .Include(o => o.CreatedByUser) // Include đúng Navigation Property
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
            .OrderByDescending(o => o.OrderDate)
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
    }

    public async Task<OrderDto?> GetByIdAsync(int id)
    {
        var order = await _context.Orders
            .Include(o => o.CreatedByUser)
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
            .FirstOrDefaultAsync(o => o.OrderId == id);

        if (order == null) return null;

        return new OrderDto
        {
            OrderId = order.OrderId,
            OrderCode = order.OrderCode,
            OrderDate = order.OrderDate,
            CustomerName = order.CustomerName,
            CustomerPhone = order.CustomerPhone,
            Status = order.Status,
            TotalAmount = order.TotalAmount,
            CreatedByUsername = order.CreatedByUser.Username,
            Details = order.OrderDetails.Select(od => new OrderDetailDto
            {
                OrderDetailId = od.OrderDetailId,
                ProductId = od.ProductId,
                ProductName = od.Product.ProductName,
                Quantity = od.Quantity,
                UnitPrice = od.UnitPrice,
                SubTotal = od.SubTotal
            }).ToList()
        };
    }
}