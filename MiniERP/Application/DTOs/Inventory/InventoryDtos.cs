namespace Application.DTOs.Inventory;

public class ReceiptDto
{
    public int WarehouseId { get; set; }
    public int ProductId { get; set; }
    public string LotNumber { get; set; } = string.Empty;
    public DateTime? ExpiryDate { get; set; }
    public decimal UnitCost { get; set; }
    public int Quantity { get; set; }
    public string? Reference { get; set; }
}

public class IssueDto
{
    public int WarehouseId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public string? Reference { get; set; }
}

public class TransferDto : IssueDto
{
    public int DestinationWarehouseId { get; set; }
}

public class InventoryBalanceDto
{
    public int WarehouseId { get; set; }
    public string WarehouseCode { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
