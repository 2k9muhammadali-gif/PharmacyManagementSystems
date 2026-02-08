using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PharmacyManagementSystem.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDistributionCompanies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DistributionId",
                table: "StockBatches",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ManufacturerId",
                table: "StockBatches",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DistributionCompanies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DistributionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ManufacturerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DistributionCompanies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DistributionCompanies_Distributions_DistributionId",
                        column: x => x.DistributionId,
                        principalTable: "Distributions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DistributionCompanies_Manufacturers_ManufacturerId",
                        column: x => x.ManufacturerId,
                        principalTable: "Manufacturers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StockBatches_DistributionId",
                table: "StockBatches",
                column: "DistributionId");

            migrationBuilder.CreateIndex(
                name: "IX_StockBatches_ManufacturerId",
                table: "StockBatches",
                column: "ManufacturerId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderLines_ManufacturerId",
                table: "PurchaseOrderLines",
                column: "ManufacturerId");

            migrationBuilder.CreateIndex(
                name: "IX_DistributionCompanies_DistributionId_ManufacturerId",
                table: "DistributionCompanies",
                columns: new[] { "DistributionId", "ManufacturerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DistributionCompanies_ManufacturerId",
                table: "DistributionCompanies",
                column: "ManufacturerId");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrderLines_Manufacturers_ManufacturerId",
                table: "PurchaseOrderLines",
                column: "ManufacturerId",
                principalTable: "Manufacturers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StockBatches_Distributions_DistributionId",
                table: "StockBatches",
                column: "DistributionId",
                principalTable: "Distributions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StockBatches_Manufacturers_ManufacturerId",
                table: "StockBatches",
                column: "ManufacturerId",
                principalTable: "Manufacturers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrderLines_Manufacturers_ManufacturerId",
                table: "PurchaseOrderLines");

            migrationBuilder.DropForeignKey(
                name: "FK_StockBatches_Distributions_DistributionId",
                table: "StockBatches");

            migrationBuilder.DropForeignKey(
                name: "FK_StockBatches_Manufacturers_ManufacturerId",
                table: "StockBatches");

            migrationBuilder.DropTable(
                name: "DistributionCompanies");

            migrationBuilder.DropIndex(
                name: "IX_StockBatches_DistributionId",
                table: "StockBatches");

            migrationBuilder.DropIndex(
                name: "IX_StockBatches_ManufacturerId",
                table: "StockBatches");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseOrderLines_ManufacturerId",
                table: "PurchaseOrderLines");

            migrationBuilder.DropColumn(
                name: "DistributionId",
                table: "StockBatches");

            migrationBuilder.DropColumn(
                name: "ManufacturerId",
                table: "StockBatches");
        }
    }
}
