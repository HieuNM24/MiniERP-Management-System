namespace Domain.Entities;

public class Warehouse
{
    public int WarehouseId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public ICollection<InventoryBalance> InventoryBalances { get; set; } = new List<InventoryBalance>();
}
