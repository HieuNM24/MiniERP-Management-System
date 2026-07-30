using Application.DTOs.Category;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Bắt buộc phải đăng nhập và gửi kèm JWT Token
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _categoryService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _categoryService.GetByIdAsync(id);
        if (result == null) return NotFound(new { message = "Không tìm thấy danh mục" });
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,InventoryManager")] // Chỉ Admin hoặc InventoryManager mới có quyền tạo
    public async Task<IActionResult> Create([FromBody] CreateUpdateCategoryDto dto)
    {
        var result = await _categoryService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.CategoryId }, result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,InventoryManager")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateUpdateCategoryDto dto)
    {
        var success = await _categoryService.UpdateAsync(id, dto);
        if (!success) return NotFound(new { message = "Không tìm thấy danh mục" });
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")] // Bắt buộc quyền Admin mới được xóa
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var success = await _categoryService.DeleteAsync(id);
            if (!success) return NotFound(new { message = "Không tìm thấy danh mục" });
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}