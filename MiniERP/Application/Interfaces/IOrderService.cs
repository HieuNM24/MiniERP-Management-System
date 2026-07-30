using Application.DTOs.Order;

namespace Application.Interfaces;

public interface IOrderService
{
    Task<OrderDto> CreateOrderAsync(CreateOrderDto dto, int userId);
    Task<IEnumerable<OrderDto>> GetAllAsync();
    Task<OrderDto?> GetByIdAsync(int id);
    Task<bool> UpdateStatusAsync(int id, string status);
}