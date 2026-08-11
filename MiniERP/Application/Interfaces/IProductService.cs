using Application.DTOs.Product;

namespace Application.Interfaces;

public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetAllAsync(string? search = null, int? categoryId = null);
    Task<ProductDto?> GetByIdAsync(int id);
    Task<ProductDto> CreateAsync(CreateUpdateProductDto dto);
    Task<bool> UpdateAsync(int id, CreateUpdateProductDto dto);
    Task<bool> DeleteAsync(int id);
}