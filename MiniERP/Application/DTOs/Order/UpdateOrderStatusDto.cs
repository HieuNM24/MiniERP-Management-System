namespace Application.DTOs.Order;

public class UpdateOrderStatusDto
{
    public string Status { get; set; } = string.Empty; // "APPROVED" hoặc "CANCELLED"
}