using System.Security.Claims;
using Application.DTOs.Order;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _orderService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _orderService.GetByIdAsync(id);
        if (result == null) return NotFound(new { message = "Không tìm thấy đơn hàng!" });
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderDto dto)
    {
        try
        {
            // 💡 Lấy UserId tự động từ JWT Token của người dùng đang đăng nhập
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Token không hợp lệ hoặc hết hạn!" });
            }

            var result = await _orderService.CreateOrderAsync(dto, userId);
            return CreatedAtAction(nameof(GetById), new { id = result.OrderId }, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Admin,InventoryManager")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusDto dto)
    {
        try
        {
            var success = await _orderService.UpdateStatusAsync(id, dto.Status);
            if (!success) return NotFound(new { message = "Không tìm thấy đơn hàng!" });
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}