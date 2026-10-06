namespace Domain.Entities;

public class InventoryBalance
{
    public int InventoryBalanceId { get; set; }
    public int WarehouseId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    public Warehouse Warehouse { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
