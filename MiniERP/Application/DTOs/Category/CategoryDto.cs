namespace Application.DTOs.Category;

public class CategoryDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int TotalProducts { get; set; } // Đếm số lượng sản phẩm thuộc danh mục này
}