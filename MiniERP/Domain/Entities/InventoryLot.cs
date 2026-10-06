namespace Domain.Entities;

public class InventoryLot
{
    public int InventoryLotId { get; set; }
    public int WarehouseId { get; set; }
    public int ProductId { get; set; }
    public string LotNumber { get; set; } = string.Empty;
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiryDate { get; set; }
    public decimal UnitCost { get; set; }
    public int RemainingQuantity { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    public Warehouse Warehouse { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
