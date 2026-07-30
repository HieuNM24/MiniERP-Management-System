namespace Application.DTOs.Order;

public class CreateOrderDto
{
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public List<CreateOrderDetailDto> Items { get; set; } = new();
}