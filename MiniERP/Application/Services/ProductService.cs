using Application.DTOs.Product;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class ProductService : IProductService
{
    private readonly IApplicationDbContext _context;

    public ProductService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProductDto>> GetAllAsync(string? search = null, int? categoryId = null)
    {
        var query = _context.Products.Include(p => p.Category).AsQueryable();

        // 🔍 Lọc theo Từ khóa tìm kiếm (Tên sản phẩm hoặc Mã SKU)
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p => p.ProductName.Contains(search) || p.SKU.Contains(search));
        }

        // 🏷️ Lọc theo Danh mục
        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        return await query.Select(p => new ProductDto
        {
            ProductId = p.ProductId,
            SKU = p.SKU,
            ProductName = p.ProductName,
            Description = p.Description,
            UnitPrice = p.UnitPrice,
            StockQuantity = p.StockQuantity,
            CategoryId = p.CategoryId,
            CategoryName = p.Category.CategoryName
        }).ToListAsync();
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var p = await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.ProductId == id);

        if (p == null) return null;

        return new ProductDto
        {
            ProductId = p.ProductId,
            SKU = p.SKU,
            ProductName = p.ProductName,
            Description = p.Description,
            UnitPrice = p.UnitPrice,
            StockQuantity = p.StockQuantity,
            CategoryId = p.CategoryId,
            CategoryName = p.Category.CategoryName
        };
    }

    public async Task<ProductDto> CreateAsync(CreateUpdateProductDto dto)
    {
        // 1. Kiểm tra SKU đã tồn tại chưa
        if (await _context.Products.AnyAsync(p => p.SKU == dto.SKU))
            throw new Exception("Mã SKU sản phẩm đã tồn tại!");

        // 2. Kiểm tra CategoryId có hợp lệ không
        var category = await _context.Categories.FindAsync(dto.CategoryId);
        if (category == null)
            throw new Exception("Danh mục sản phẩm không tồn tại!");

        var product = new Product
        {
            SKU = dto.SKU,
            ProductName = dto.ProductName,
            Description = dto.Description,
            UnitPrice = dto.UnitPrice,
            StockQuantity = dto.StockQuantity,
            CategoryId = dto.CategoryId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return new ProductDto
        {
            ProductId = product.ProductId,
            SKU = product.SKU,
            ProductName = product.ProductName,
            Description = product.Description,
            UnitPrice = product.UnitPrice,
            StockQuantity = product.StockQuantity,
            CategoryId = product.CategoryId,
            CategoryName = category.CategoryName
        };
    }

    public async Task<bool> UpdateAsync(int id, CreateUpdateProductDto dto)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return false;

        // Nếu đổi SKU thì phải kiểm tra xem SKU mới có bị trùng không
        if (product.SKU != dto.SKU && await _context.Products.AnyAsync(p => p.SKU == dto.SKU))
            throw new Exception("Mã SKU mới đã bị trùng với sản phẩm khác!");

        product.SKU = dto.SKU;
        product.ProductName = dto.ProductName;
        product.Description = dto.Description;
        product.UnitPrice = dto.UnitPrice;
        product.StockQuantity = dto.StockQuantity;
        product.CategoryId = dto.CategoryId;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return false;

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return true;
    }
}