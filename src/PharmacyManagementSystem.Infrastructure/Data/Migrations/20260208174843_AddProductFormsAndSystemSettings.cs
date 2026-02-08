using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PharmacyManagementSystem.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProductFormsAndSystemSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrderLines_Manufacturers_ManufacturerId",
                table: "PurchaseOrderLines");

            migrationBuilder.CreateTable(
                name: "ProductForms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductForms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSettings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductForms_Name",
                table: "ProductForms",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SystemSettings_Category_Key",
                table: "SystemSettings",
                columns: new[] { "Category", "Key" },
                unique: true);

            // Seed default product forms
            var defaultForms = new[] { "Tablet", "Capsule", "Injection", "Syrup", "Suspension", "Liquid", "Cream", "Ointment", "Gel", "Drops", "Spray", "Powder", "Suppository", "Lotion", "Solution", "Other" };
            for (int i = 0; i < defaultForms.Length; i++)
            {
                var id = Guid.NewGuid();
                migrationBuilder.InsertData("ProductForms", new[] { "Id", "Name", "DisplayOrder", "IsActive" }, new object[] { id, defaultForms[i], i, true });
            }

            migrationBuilder.AddColumn<Guid>(
                name: "ProductFormId",
                table: "Products",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_ProductFormId",
                table: "Products",
                column: "ProductFormId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_ProductForms_ProductFormId",
                table: "Products",
                column: "ProductFormId",
                principalTable: "ProductForms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.DropColumn(
                name: "Formulation",
                table: "Products");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrderLines_Manufacturers_ManufacturerId",
                table: "PurchaseOrderLines",
                column: "ManufacturerId",
                principalTable: "Manufacturers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_ProductForms_ProductFormId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrderLines_Manufacturers_ManufacturerId",
                table: "PurchaseOrderLines");

            migrationBuilder.DropTable(
                name: "ProductForms");

            migrationBuilder.DropTable(
                name: "SystemSettings");

            migrationBuilder.DropIndex(
                name: "IX_Products_ProductFormId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ProductFormId",
                table: "Products");

            migrationBuilder.AddColumn<string>(
                name: "Formulation",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrderLines_Manufacturers_ManufacturerId",
                table: "PurchaseOrderLines",
                column: "ManufacturerId",
                principalTable: "Manufacturers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
