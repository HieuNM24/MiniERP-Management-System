using Application.DTOs.Product;
using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _productService.GetAllAsync(search, categoryId);
        return Ok(result);
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(Products);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _productService.GetByIdAsync(id);
        if (result == null) return NotFound(new { message = "Không tìm thấy sản phẩm" });
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,InventoryManager")]
    public async Task<IActionResult> Create([FromBody] CreateUpdateProductDto dto)
    {
        try
        {
            var result = await _productService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.ProductId }, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,InventoryManager")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateUpdateProductDto dto)
    {
        try
        {
            var success = await _productService.UpdateAsync(id, dto);
            if (!success) return NotFound(new { message = "Không tìm thấy sản phẩm" });
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,InventoryManager")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _productService.DeleteAsync(id);
        if (!success) return NotFound(new { message = "Không tìm thấy sản phẩm" });
        return NoContent();
    }
}