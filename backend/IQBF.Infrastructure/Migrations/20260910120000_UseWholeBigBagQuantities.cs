using IQBF.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IQBF.Infrastructure.Migrations
{
    [DbContext(typeof(IQBFDbContext))]
    [Migration("20260910120000_UseWholeBigBagQuantities")]
    public partial class UseWholeBigBagQuantities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM BLs WHERE TotalQuantity <> FLOOR(TotalQuantity))
    THROW 50001, 'Existen BL con cantidades fraccionarias. Corrija los datos antes de aplicar la migración.', 1;
IF EXISTS (SELECT 1 FROM ReceptionItems WHERE Quantity <> FLOOR(Quantity))
    THROW 50002, 'Existen recepciones con cantidades fraccionarias. Corrija los datos antes de aplicar la migración.', 1;
IF EXISTS (SELECT 1 FROM DispatchItems WHERE Quantity <> FLOOR(Quantity))
    THROW 50003, 'Existen despachos con cantidades fraccionarias. Corrija los datos antes de aplicar la migración.', 1;
");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BLs_TotalQuantity_Positive",
                table: "BLs");
            migrationBuilder.DropCheckConstraint(
                name: "CK_ReceptionItems_Quantity_Positive",
                table: "ReceptionItems");
            migrationBuilder.DropCheckConstraint(
                name: "CK_DispatchItems_Quantity_Positive",
                table: "DispatchItems");

            migrationBuilder.AlterColumn<int>(name: "TotalQuantity", table: "BLs", type: "int", nullable: false, oldClrType: typeof(decimal), oldType: "decimal(18,3)", oldPrecision: 18, oldScale: 3);
            migrationBuilder.AlterColumn<int>(name: "Quantity", table: "ReceptionItems", type: "int", nullable: false, oldClrType: typeof(decimal), oldType: "decimal(18,3)", oldPrecision: 18, oldScale: 3);
            migrationBuilder.AlterColumn<int>(name: "Quantity", table: "DispatchItems", type: "int", nullable: false, oldClrType: typeof(decimal), oldType: "decimal(18,3)", oldPrecision: 18, oldScale: 3);

            migrationBuilder.AddCheckConstraint(
                name: "CK_BLs_TotalQuantity_Positive",
                table: "BLs",
                sql: "[TotalQuantity] > 0");
            migrationBuilder.AddCheckConstraint(
                name: "CK_ReceptionItems_Quantity_Positive",
                table: "ReceptionItems",
                sql: "[Quantity] > 0");
            migrationBuilder.AddCheckConstraint(
                name: "CK_DispatchItems_Quantity_Positive",
                table: "DispatchItems",
                sql: "[Quantity] > 0");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(name: "CK_BLs_TotalQuantity_Positive", table: "BLs");
            migrationBuilder.DropCheckConstraint(name: "CK_ReceptionItems_Quantity_Positive", table: "ReceptionItems");
            migrationBuilder.DropCheckConstraint(name: "CK_DispatchItems_Quantity_Positive", table: "DispatchItems");

            migrationBuilder.AlterColumn<decimal>(name: "TotalQuantity", table: "BLs", type: "decimal(18,3)", precision: 18, scale: 3, nullable: false, oldClrType: typeof(int), oldType: "int");
            migrationBuilder.AlterColumn<decimal>(name: "Quantity", table: "ReceptionItems", type: "decimal(18,3)", precision: 18, scale: 3, nullable: false, oldClrType: typeof(int), oldType: "int");
            migrationBuilder.AlterColumn<decimal>(name: "Quantity", table: "DispatchItems", type: "decimal(18,3)", precision: 18, scale: 3, nullable: false, oldClrType: typeof(int), oldType: "int");

            migrationBuilder.AddCheckConstraint(name: "CK_BLs_TotalQuantity_Positive", table: "BLs", sql: "[TotalQuantity] > 0");
            migrationBuilder.AddCheckConstraint(name: "CK_ReceptionItems_Quantity_Positive", table: "ReceptionItems", sql: "[Quantity] > 0");
            migrationBuilder.AddCheckConstraint(name: "CK_DispatchItems_Quantity_Positive", table: "DispatchItems", sql: "[Quantity] > 0");
        }
    }
}
