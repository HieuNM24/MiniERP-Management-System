using Application.DTOs.Inventory;

namespace Application.Interfaces;

public interface IInventoryService
{
    Task<InventoryBalanceDto> ReceiveAsync(ReceiptDto dto, int userId);
    Task<InventoryBalanceDto> IssueAsync(IssueDto dto, int userId);
    Task TransferAsync(TransferDto dto, int userId);
    Task<IEnumerable<InventoryBalanceDto>> GetBalancesAsync(int? warehouseId = null, int? productId = null);
}
