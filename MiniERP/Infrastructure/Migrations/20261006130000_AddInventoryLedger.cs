using Microsoft.EntityFrameworkCore.Migrations;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace Infrastructure.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261006130000_AddInventoryLedger")]
public partial class AddInventoryLedger : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(name: "FK_Orders_Users_CreatedByUserUserId", table: "Orders");
        migrationBuilder.DropIndex(name: "IX_Orders_CreatedByUserUserId", table: "Orders");
        migrationBuilder.DropColumn(name: "CreatedByUserUserId", table: "Orders");
        migrationBuilder.CreateIndex(name: "IX_Orders_CreatedBy", table: "Orders", column: "CreatedBy");
        migrationBuilder.AddForeignKey(name: "FK_Orders_Users_CreatedBy", table: "Orders", column: "CreatedBy", principalTable: "Users", principalColumn: "UserId", onDelete: ReferentialAction.Restrict);

        migrationBuilder.CreateTable(name: "Warehouses", columns: table => new
        {
            WarehouseId = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
            Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
            Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
            IsActive = table.Column<bool>(type: "bit", nullable: false)
        }, constraints: table => table.PrimaryKey("PK_Warehouses", x => x.WarehouseId));

        migrationBuilder.InsertData(table: "Warehouses", columns: new[] { "WarehouseId", "Code", "Name", "IsActive" }, values: new object[] { 1, "MAIN", "Kho chính", true });

        migrationBuilder.CreateTable(name: "InventoryBalances", columns: table => new
        {
            InventoryBalanceId = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
            WarehouseId = table.Column<int>(type: "int", nullable: false), ProductId = table.Column<int>(type: "int", nullable: false),
            Quantity = table.Column<int>(type: "int", nullable: false), RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_InventoryBalances", x => x.InventoryBalanceId);
            table.ForeignKey("FK_InventoryBalances_Warehouses_WarehouseId", x => x.WarehouseId, "Warehouses", "WarehouseId", onDelete: ReferentialAction.Restrict);
            table.ForeignKey("FK_InventoryBalances_Products_ProductId", x => x.ProductId, "Products", "ProductId", onDelete: ReferentialAction.Restrict);
        });

        migrationBuilder.CreateTable(name: "InventoryLots", columns: table => new
        {
            InventoryLotId = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
            WarehouseId = table.Column<int>(type: "int", nullable: false), ProductId = table.Column<int>(type: "int", nullable: false),
            LotNumber = table.Column<string>(type: "nvarchar(450)", nullable: false), ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
            ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true), UnitCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            RemainingQuantity = table.Column<int>(type: "int", nullable: false), RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_InventoryLots", x => x.InventoryLotId);
            table.ForeignKey("FK_InventoryLots_Warehouses_WarehouseId", x => x.WarehouseId, "Warehouses", "WarehouseId", onDelete: ReferentialAction.Restrict);
            table.ForeignKey("FK_InventoryLots_Products_ProductId", x => x.ProductId, "Products", "ProductId", onDelete: ReferentialAction.Restrict);
        });

        migrationBuilder.CreateTable(name: "InventoryTransactions", columns: table => new
        {
            InventoryTransactionId = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
            TransactionCode = table.Column<string>(type: "nvarchar(max)", nullable: false), TransactionType = table.Column<string>(type: "nvarchar(max)", nullable: false),
            WarehouseId = table.Column<int>(type: "int", nullable: false), ProductId = table.Column<int>(type: "int", nullable: false), InventoryLotId = table.Column<int>(type: "int", nullable: true),
            Quantity = table.Column<int>(type: "int", nullable: false), UnitCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false), Reference = table.Column<string>(type: "nvarchar(max)", nullable: true),
            CreatedBy = table.Column<int>(type: "int", nullable: false), CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_InventoryTransactions", x => x.InventoryTransactionId);
            table.ForeignKey("FK_InventoryTransactions_Warehouses_WarehouseId", x => x.WarehouseId, "Warehouses", "WarehouseId", onDelete: ReferentialAction.Restrict);
            table.ForeignKey("FK_InventoryTransactions_Products_ProductId", x => x.ProductId, "Products", "ProductId", onDelete: ReferentialAction.Restrict);
            table.ForeignKey("FK_InventoryTransactions_InventoryLots_InventoryLotId", x => x.InventoryLotId, "InventoryLots", "InventoryLotId", onDelete: ReferentialAction.Restrict);
        });

        migrationBuilder.CreateIndex(name: "IX_Warehouses_Code", table: "Warehouses", column: "Code", unique: true);
        migrationBuilder.CreateIndex(name: "IX_InventoryBalances_WarehouseId_ProductId", table: "InventoryBalances", columns: new[] { "WarehouseId", "ProductId" }, unique: true);
        migrationBuilder.CreateIndex(name: "IX_InventoryBalances_ProductId", table: "InventoryBalances", column: "ProductId");
        migrationBuilder.CreateIndex(name: "IX_InventoryLots_WarehouseId_ProductId_LotNumber", table: "InventoryLots", columns: new[] { "WarehouseId", "ProductId", "LotNumber" }, unique: true);
        migrationBuilder.CreateIndex(name: "IX_InventoryLots_ProductId", table: "InventoryLots", column: "ProductId");
        migrationBuilder.CreateIndex(name: "IX_InventoryLots_WarehouseId", table: "InventoryLots", column: "WarehouseId");
        migrationBuilder.CreateIndex(name: "IX_InventoryTransactions_WarehouseId_ProductId_CreatedAt", table: "InventoryTransactions", columns: new[] { "WarehouseId", "ProductId", "CreatedAt" });
        migrationBuilder.CreateIndex(name: "IX_InventoryTransactions_ProductId", table: "InventoryTransactions", column: "ProductId");
        migrationBuilder.CreateIndex(name: "IX_InventoryTransactions_InventoryLotId", table: "InventoryTransactions", column: "InventoryLotId");

        migrationBuilder.Sql("INSERT INTO InventoryBalances (WarehouseId, ProductId, Quantity) SELECT 1, ProductId, StockQuantity FROM Products WHERE StockQuantity > 0;");
        migrationBuilder.Sql("INSERT INTO InventoryLots (WarehouseId, ProductId, LotNumber, ReceivedAt, UnitCost, RemainingQuantity) SELECT 1, ProductId, CONCAT('OPEN-', ProductId), SYSUTCDATETIME(), UnitPrice, StockQuantity FROM Products WHERE StockQuantity > 0;");
        migrationBuilder.Sql("INSERT INTO InventoryTransactions (TransactionCode, TransactionType, WarehouseId, ProductId, InventoryLotId, Quantity, UnitCost, Reference, CreatedBy, CreatedAt) SELECT CONCAT('OPEN-', p.ProductId), 'RECEIPT', 1, p.ProductId, l.InventoryLotId, p.StockQuantity, p.UnitPrice, 'Opening balance', 0, SYSUTCDATETIME() FROM Products p JOIN InventoryLots l ON l.WarehouseId = 1 AND l.ProductId = p.ProductId AND l.LotNumber = CONCAT('OPEN-', p.ProductId) WHERE p.StockQuantity > 0;");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "InventoryTransactions");
        migrationBuilder.DropTable(name: "InventoryBalances");
        migrationBuilder.DropTable(name: "InventoryLots");
        migrationBuilder.DeleteData(table: "Warehouses", keyColumn: "WarehouseId", keyValue: 1);
        migrationBuilder.DropTable(name: "Warehouses");
        migrationBuilder.DropForeignKey(name: "FK_Orders_Users_CreatedBy", table: "Orders");
        migrationBuilder.DropIndex(name: "IX_Orders_CreatedBy", table: "Orders");
        migrationBuilder.AddColumn<int>(name: "CreatedByUserUserId", table: "Orders", type: "int", nullable: false, defaultValue: 0);
        migrationBuilder.CreateIndex(name: "IX_Orders_CreatedByUserUserId", table: "Orders", column: "CreatedByUserUserId");
        migrationBuilder.AddForeignKey(name: "FK_Orders_Users_CreatedByUserUserId", table: "Orders", column: "CreatedByUserUserId", principalTable: "Users", principalColumn: "UserId", onDelete: ReferentialAction.Cascade);
    }
}
