using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DigitalArs.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddServicesAndReserves : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MoneyReserves",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TargetAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    CurrentBalance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    Icon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Color = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MoneyReserves", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MoneyReserves_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiceProviders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    CodeLabel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IconName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceProviders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServicePayments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    ServiceProviderId = table.Column<int>(type: "int", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ReceiptNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TransactionId = table.Column<int>(type: "int", nullable: false),
                    ReserveId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServicePayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServicePayments_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServicePayments_MoneyReserves_ReserveId",
                        column: x => x.ReserveId,
                        principalTable: "MoneyReserves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ServicePayments_ServiceProviders_ServiceProviderId",
                        column: x => x.ServiceProviderId,
                        principalTable: "ServiceProviders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServicePayments_Transactions_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "ServiceProviders",
                columns: new[] { "Id", "Category", "CodeLabel", "IconName", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, 1, "Número de Cuenta / Cliente (10 dígitos)", "bolt", true, "Edenor" },
                    { 2, 1, "Número de Cliente (8 dígitos)", "bolt", true, "Edesur" },
                    { 3, 1, "Código de Pago Electrónico (14 dígitos)", "bolt", true, "EPEC Córdoba" },
                    { 4, 2, "Número de Cuenta de Servicios (10 dígitos)", "water_drop", true, "AySA" },
                    { 5, 2, "Código de Unidad de Facturación (8 dígitos)", "water_drop", true, "Aguas Cordobesas" },
                    { 6, 3, "Número de Referencia de Pago (11 dígitos)", "local_fire_department", true, "Metrogas" },
                    { 7, 3, "Número de Cuenta de Factura (10 dígitos)", "local_fire_department", true, "Naturgy" },
                    { 8, 3, "Código Link / Banelco (12 dígitos)", "local_fire_department", true, "Camuzzi Gas" },
                    { 9, 4, "Número de Línea (10 dígitos con código de área)", "phone_iphone", true, "Claro" },
                    { 10, 4, "Número de Línea o Código de Pago (10 dígitos)", "phone_iphone", true, "Personal Flow" },
                    { 11, 4, "Número de Celular o Cuenta (10 dígitos)", "phone_iphone", true, "Movistar" },
                    { 12, 4, "Número de Cliente (8 dígitos)", "router", true, "Telecentro" },
                    { 13, 5, "Número de VEP (Volante Electrónico de Pago)", "account_balance", true, "ARCA / AFIP (VEP)" },
                    { 14, 5, "Código de Pago Electrónico / Partida", "receipt", true, "AGIP Rentas CABA" },
                    { 15, 5, "Código de Pago Electrónico (14 dígitos)", "receipt", true, "ARBA Buenos Aires" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_MoneyReserves_AccountId",
                table: "MoneyReserves",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ServicePayments_AccountId",
                table: "ServicePayments",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ServicePayments_ReserveId",
                table: "ServicePayments",
                column: "ReserveId");

            migrationBuilder.CreateIndex(
                name: "IX_ServicePayments_ServiceProviderId",
                table: "ServicePayments",
                column: "ServiceProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_ServicePayments_TransactionId",
                table: "ServicePayments",
                column: "TransactionId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServicePayments");

            migrationBuilder.DropTable(
                name: "MoneyReserves");

            migrationBuilder.DropTable(
                name: "ServiceProviders");
        }
    }
}
