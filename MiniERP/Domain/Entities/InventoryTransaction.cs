namespace Domain.Entities;

public class InventoryTransaction
{
    public int InventoryTransactionId { get; set; }
    public string TransactionCode { get; set; } = string.Empty;
    public string TransactionType { get; set; } = string.Empty; // RECEIPT, ISSUE, TRANSFER_OUT, TRANSFER_IN
    public int WarehouseId { get; set; }
    public int ProductId { get; set; }
    public int? InventoryLotId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public string? Reference { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Warehouse Warehouse { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public InventoryLot? InventoryLot { get; set; }
}
