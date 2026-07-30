namespace Application.DTOs.Product;

public class CreateUpdateProductDto
{
    public string SKU { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int StockQuantity { get; set; }
    public int CategoryId { get; set; }
}