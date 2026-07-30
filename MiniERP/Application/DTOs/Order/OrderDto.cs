namespace Application.DTOs.Order;

public class OrderDto
{
    public int OrderId { get; set; }
    public string OrderCode { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "PENDING";
    public string CreatedByUsername { get; set; } = string.Empty;
    public List<OrderDetailDto> Details { get; set; } = new();
}