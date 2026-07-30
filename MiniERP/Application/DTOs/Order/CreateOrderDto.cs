namespace Application.DTOs.Order;

public class CreateOrderDto
{
    public List<CreateOrderDetailDto> Items { get; set; } = new();
}