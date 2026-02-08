using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PharmacyManagementSystem.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNewEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchaseOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaymentMode = table.Column<int>(type: "int", nullable: false),
                    Reference = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_PurchaseOrders_PurchaseOrderId",
                        column: x => x.PurchaseOrderId,
                        principalTable: "PurchaseOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SaleReturns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SaleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReturnDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FBRCreditNoteNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalRefund = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaleReturns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SaleReturns_Sales_SaleId",
                        column: x => x.SaleId,
                        principalTable: "Sales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StockAdjustments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StockBatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    AdjustmentType = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdjustedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AdjustedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockAdjustments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockAdjustments_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockAdjustments_StockBatches_StockBatchId",
                        column: x => x.StockBatchId,
                        principalTable: "StockBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TransferRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromBranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ToBranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RequestedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApprovedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransferRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransferRequests_Branches_FromBranchId",
                        column: x => x.FromBranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransferRequests_Branches_ToBranchId",
                        column: x => x.ToBranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TransferRequestLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransferRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StockBatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransferRequestLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransferRequestLines_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TransferRequestLines_TransferRequests_TransferRequestId",
                        column: x => x.TransferRequestId,
                        principalTable: "TransferRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ControlledSubstanceLogs_BranchId",
                table: "ControlledSubstanceLogs",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_ControlledSubstanceLogs_ProductId",
                table: "ControlledSubstanceLogs",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PurchaseOrderId",
                table: "Payments",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleReturns_SaleId",
                table: "SaleReturns",
                column: "SaleId");

            migrationBuilder.CreateIndex(
                name: "IX_StockAdjustments_BranchId",
                table: "StockAdjustments",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_StockAdjustments_StockBatchId",
                table: "StockAdjustments",
                column: "StockBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferRequestLines_ProductId",
                table: "TransferRequestLines",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferRequestLines_TransferRequestId",
                table: "TransferRequestLines",
                column: "TransferRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferRequests_FromBranchId",
                table: "TransferRequests",
                column: "FromBranchId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferRequests_ToBranchId",
                table: "TransferRequests",
                column: "ToBranchId");

            migrationBuilder.AddForeignKey(
                name: "FK_ControlledSubstanceLogs_Branches_BranchId",
                table: "ControlledSubstanceLogs",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ControlledSubstanceLogs_Products_ProductId",
                table: "ControlledSubstanceLogs",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ControlledSubstanceLogs_Branches_BranchId",
                table: "ControlledSubstanceLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_ControlledSubstanceLogs_Products_ProductId",
                table: "ControlledSubstanceLogs");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "SaleReturns");

            migrationBuilder.DropTable(
                name: "StockAdjustments");

            migrationBuilder.DropTable(
                name: "TransferRequestLines");

            migrationBuilder.DropTable(
                name: "TransferRequests");

            migrationBuilder.DropIndex(
                name: "IX_ControlledSubstanceLogs_BranchId",
                table: "ControlledSubstanceLogs");

            migrationBuilder.DropIndex(
                name: "IX_ControlledSubstanceLogs_ProductId",
                table: "ControlledSubstanceLogs");
        }
    }
}
