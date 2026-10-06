using Application.DTOs.Inventory;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class InventoryService : IInventoryService
{
    private readonly IApplicationDbContext _context;

    public InventoryService(IApplicationDbContext context) => _context = context;

    public async Task<InventoryBalanceDto> ReceiveAsync(ReceiptDto dto, int userId)
    {
        Validate(dto.Quantity, dto.UnitCost);
        await EnsureActiveAsync(dto.WarehouseId, dto.ProductId);

        if (await _context.InventoryLots.AnyAsync(x => x.WarehouseId == dto.WarehouseId && x.ProductId == dto.ProductId && x.LotNumber == dto.LotNumber))
            throw new InvalidOperationException("Lô đã tồn tại trong kho; mỗi lần nhập phải có mã lô riêng.");

        var lot = new InventoryLot
        {
            WarehouseId = dto.WarehouseId, ProductId = dto.ProductId, LotNumber = dto.LotNumber,
            ExpiryDate = dto.ExpiryDate, UnitCost = dto.UnitCost, RemainingQuantity = dto.Quantity
        };
        var balance = await GetOrCreateBalanceAsync(dto.WarehouseId, dto.ProductId);
        balance.Quantity += dto.Quantity;
        _context.InventoryLots.Add(lot);
        _context.InventoryTransactions.Add(Movement("RECEIPT", dto.WarehouseId, dto.ProductId, lot, dto.Quantity, dto.UnitCost, dto.Reference, userId));
        await _context.SaveChangesAsync();
        return await ToDtoAsync(balance);
    }

    public async Task<InventoryBalanceDto> IssueAsync(IssueDto dto, int userId)
    {
        Validate(dto.Quantity);
        await EnsureActiveAsync(dto.WarehouseId, dto.ProductId);
        var balance = await GetBalanceAsync(dto.WarehouseId, dto.ProductId);
        var allocations = await AllocateFifoAsync(dto.WarehouseId, dto.ProductId, dto.Quantity);
        balance.Quantity -= dto.Quantity;
        foreach (var (lot, quantity) in allocations)
        {
            lot.RemainingQuantity -= quantity;
            _context.InventoryTransactions.Add(Movement("ISSUE", dto.WarehouseId, dto.ProductId, lot, quantity, lot.UnitCost, dto.Reference, userId));
        }
        await _context.SaveChangesAsync();
        return await ToDtoAsync(balance);
    }

    public async Task TransferAsync(TransferDto dto, int userId)
    {
        if (dto.WarehouseId == dto.DestinationWarehouseId) throw new InvalidOperationException("Kho nguồn và kho đích phải khác nhau.");
        Validate(dto.Quantity);
        await EnsureActiveAsync(dto.WarehouseId, dto.ProductId);
        await EnsureActiveAsync(dto.DestinationWarehouseId, dto.ProductId);

        var source = await GetBalanceAsync(dto.WarehouseId, dto.ProductId);
        var destination = await GetOrCreateBalanceAsync(dto.DestinationWarehouseId, dto.ProductId);
        var allocations = await AllocateFifoAsync(dto.WarehouseId, dto.ProductId, dto.Quantity);
        var code = $"TRF-{Guid.NewGuid():N}";
        source.Quantity -= dto.Quantity;
        destination.Quantity += dto.Quantity;

        foreach (var (sourceLot, quantity) in allocations)
        {
            sourceLot.RemainingQuantity -= quantity;
            var destinationLot = await _context.InventoryLots.SingleOrDefaultAsync(x =>
                x.WarehouseId == dto.DestinationWarehouseId && x.ProductId == dto.ProductId && x.LotNumber == sourceLot.LotNumber);
            if (destinationLot is null)
            {
                destinationLot = new InventoryLot
                {
                    WarehouseId = dto.DestinationWarehouseId, ProductId = dto.ProductId, LotNumber = sourceLot.LotNumber,
                    ReceivedAt = sourceLot.ReceivedAt, ExpiryDate = sourceLot.ExpiryDate, UnitCost = sourceLot.UnitCost
                };
                _context.InventoryLots.Add(destinationLot);
            }
            destinationLot.RemainingQuantity += quantity;
            _context.InventoryTransactions.Add(Movement("TRANSFER_OUT", dto.WarehouseId, dto.ProductId, sourceLot, quantity, sourceLot.UnitCost, code, userId));
            _context.InventoryTransactions.Add(Movement("TRANSFER_IN", dto.DestinationWarehouseId, dto.ProductId, destinationLot, quantity, destinationLot.UnitCost, code, userId));
        }
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<InventoryBalanceDto>> GetBalancesAsync(int? warehouseId = null, int? productId = null)
    {
        var query = _context.InventoryBalances.Include(x => x.Warehouse).Include(x => x.Product).AsQueryable();
        if (warehouseId.HasValue) query = query.Where(x => x.WarehouseId == warehouseId);
        if (productId.HasValue) query = query.Where(x => x.ProductId == productId);
        return await query.OrderBy(x => x.Warehouse.Code).ThenBy(x => x.Product.ProductName).Select(x => new InventoryBalanceDto
        {
            WarehouseId = x.WarehouseId, WarehouseCode = x.Warehouse.Code, ProductId = x.ProductId,
            ProductName = x.Product.ProductName, Quantity = x.Quantity, RowVersion = x.RowVersion
        }).ToListAsync();
    }

    private async Task<List<(InventoryLot Lot, int Quantity)>> AllocateFifoAsync(int warehouseId, int productId, int quantity)
    {
        var lots = await _context.InventoryLots.Where(x => x.WarehouseId == warehouseId && x.ProductId == productId && x.RemainingQuantity > 0 &&
                (!x.ExpiryDate.HasValue || x.ExpiryDate >= DateTime.UtcNow.Date))
            .OrderBy(x => x.ReceivedAt).ThenBy(x => x.InventoryLotId).ToListAsync();
        var left = quantity;
        var allocations = new List<(InventoryLot, int)>();
        foreach (var lot in lots)
        {
            var take = Math.Min(left, lot.RemainingQuantity);
            allocations.Add((lot, take));
            left -= take;
            if (left == 0) return allocations;
        }
        throw new InvalidOperationException("Tồn khả dụng không đủ hoặc lô còn lại đã hết hạn.");
    }

    private async Task<InventoryBalance> GetBalanceAsync(int warehouseId, int productId) =>
        await _context.InventoryBalances.SingleOrDefaultAsync(x => x.WarehouseId == warehouseId && x.ProductId == productId)
        ?? throw new InvalidOperationException("Không có tồn kho cho sản phẩm tại kho này.");

    private async Task<InventoryBalance> GetOrCreateBalanceAsync(int warehouseId, int productId)
    {
        var balance = await _context.InventoryBalances.SingleOrDefaultAsync(x => x.WarehouseId == warehouseId && x.ProductId == productId);
        if (balance is not null) return balance;
        balance = new InventoryBalance { WarehouseId = warehouseId, ProductId = productId };
        _context.InventoryBalances.Add(balance);
        return balance;
    }

    private async Task EnsureActiveAsync(int warehouseId, int productId)
    {
        if (!await _context.Warehouses.AnyAsync(x => x.WarehouseId == warehouseId && x.IsActive)) throw new InvalidOperationException("Kho không tồn tại hoặc đã ngừng hoạt động.");
        if (!await _context.Products.AnyAsync(x => x.ProductId == productId && x.IsActive)) throw new InvalidOperationException("Sản phẩm không tồn tại hoặc đã ngừng hoạt động.");
    }

    private async Task<InventoryBalanceDto> ToDtoAsync(InventoryBalance balance) => await _context.InventoryBalances
        .Where(x => x.InventoryBalanceId == balance.InventoryBalanceId).Include(x => x.Warehouse).Include(x => x.Product)
        .Select(x => new InventoryBalanceDto { WarehouseId = x.WarehouseId, WarehouseCode = x.Warehouse.Code, ProductId = x.ProductId, ProductName = x.Product.ProductName, Quantity = x.Quantity, RowVersion = x.RowVersion }).SingleAsync();

    private static InventoryTransaction Movement(string type, int warehouseId, int productId, InventoryLot lot, int quantity, decimal unitCost, string? reference, int userId) => new()
    {
        TransactionCode = $"INV-{Guid.NewGuid():N}", TransactionType = type, WarehouseId = warehouseId, ProductId = productId,
        InventoryLot = lot, Quantity = quantity, UnitCost = unitCost, Reference = reference, CreatedBy = userId
    };

    private static void Validate(int quantity, decimal? unitCost = null)
    {
        if (quantity <= 0) throw new InvalidOperationException("Số lượng phải lớn hơn 0.");
        if (unitCost.HasValue && unitCost < 0) throw new InvalidOperationException("Đơn giá không hợp lệ.");
    }
}
