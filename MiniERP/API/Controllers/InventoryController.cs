using System.Security.Claims;
using Application.DTOs.Inventory;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[ApiController]
[Route("api/inventory")]
[Authorize(Roles = "Admin,InventoryManager")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventory;
    public InventoryController(IInventoryService inventory) => _inventory = inventory;

    [HttpGet("balances")]
    public async Task<IActionResult> GetBalances([FromQuery] int? warehouseId, [FromQuery] int? productId) => Ok(await _inventory.GetBalancesAsync(warehouseId, productId));

    [HttpPost("receipts")]
    public Task<IActionResult> Receive(ReceiptDto dto) => Execute(async userId => Created("api/inventory/balances", await _inventory.ReceiveAsync(dto, userId)));

    [HttpPost("issues")]
    public Task<IActionResult> Issue(IssueDto dto) => Execute(async userId => Ok(await _inventory.IssueAsync(dto, userId)));

    [HttpPost("transfers")]
    public Task<IActionResult> Transfer(TransferDto dto) => Execute(async userId => { await _inventory.TransferAsync(dto, userId); return NoContent(); });

    private async Task<IActionResult> Execute(Func<int, Task<IActionResult>> action)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)) return Unauthorized();
        try { return await action(userId); }
        catch (DbUpdateConcurrencyException) { return Conflict(new { message = "Tồn kho vừa thay đổi; hãy tải lại và thử lại." }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }
}
