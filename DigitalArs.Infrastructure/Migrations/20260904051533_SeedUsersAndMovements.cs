using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DigitalArs.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedUsersAndMovements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "CreatedAt", "Email", "EmailConfirmed", "FirstName", "LastName", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "RoleId", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[,]
                {
                    { 4, 0, "seed-user4-concurrency", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "alejandro.silva@digitalars.com", true, "Alejandro", "Silva", false, null, "ALEJANDRO.SILVA@DIGITALARS.COM", "ALEJANDRO.SILVA@DIGITALARS.COM", "AQAAAAIAAYagAAAAEIF4BH6BgJcp+Hmu8tYbCiyDyfC8/R3A8lus7ILAex/9qAxhI8YRaq7+ERYrGrRrYg==", null, false, 2, "SEED-USER4-SECURITY-STAMP", false, "alejandro.silva@digitalars.com" },
                    { 5, 0, "seed-user5-concurrency", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "micaela.mulato@digitalars.com", true, "Micaela", "Mulato", false, null, "MICAELA.MULATO@DIGITALARS.COM", "MICAELA.MULATO@DIGITALARS.COM", "AQAAAAIAAYagAAAAEPlWIfeBvEa2UIgXOAlJgZkhZ9W+6n3zxsEIiVncqS9jY+6qmsMbOL+u+DeaM10S1w==", null, false, 2, "SEED-USER5-SECURITY-STAMP", false, "micaela.mulato@digitalars.com" },
                    { 6, 0, "seed-user6-concurrency", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "emmanuel.torres@digitalars.com", true, "Emmanuel", "Torres", false, null, "EMMANUEL.TORRES@DIGITALARS.COM", "EMMANUEL.TORRES@DIGITALARS.COM", "AQAAAAIAAYagAAAAEISvy59LTmpoZ20JI0rhgziQdnw7hg1vq272APTffUCeMWtHb8rAl0V5Src75EltPA==", null, false, 2, "SEED-USER6-SECURITY-STAMP", false, "emmanuel.torres@digitalars.com" }
                });

            migrationBuilder.InsertData(
                table: "Transactions",
                columns: new[] { "Id", "AccountId", "Amount", "Concept", "Date", "ToAccountId", "Type" },
                values: new object[,]
                {
                    { 2, 2, 239000.00m, "Depósito inicial", new DateTime(2026, 8, 1, 10, 0, 0, 0, DateTimeKind.Utc), null, 1 },
                    { 3, 3, 193500.50m, "Depósito inicial", new DateTime(2026, 8, 1, 10, 0, 0, 0, DateTimeKind.Utc), null, 1 }
                });

            migrationBuilder.InsertData(
                table: "Accounts",
                columns: new[] { "Id", "CreatedAt", "Money", "UserId" },
                values: new object[,]
                {
                    { 4, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 45230.50m, 4 },
                    { 5, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 320000.00m, 5 },
                    { 6, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 410000.00m, 6 }
                });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[,]
                {
                    { 2, 4 },
                    { 2, 5 },
                    { 2, 6 }
                });

            migrationBuilder.InsertData(
                table: "Transactions",
                columns: new[] { "Id", "AccountId", "Amount", "Concept", "Date", "ToAccountId", "Type" },
                values: new object[,]
                {
                    { 1, 4, 43730.50m, "Depósito inicial de fondos", new DateTime(2026, 8, 1, 10, 0, 0, 0, DateTimeKind.Utc), null, 1 },
                    { 4, 5, 340000.00m, "Depósito inicial", new DateTime(2026, 8, 1, 10, 0, 0, 0, DateTimeKind.Utc), null, 1 },
                    { 5, 6, 404000.00m, "Depósito inicial", new DateTime(2026, 8, 1, 10, 0, 0, 0, DateTimeKind.Utc), null, 1 },
                    { 6, 4, 15000.00m, "Transferencia enviada a Roberto Carlos", new DateTime(2026, 8, 10, 14, 30, 0, 0, DateTimeKind.Utc), 2, 3 },
                    { 7, 2, 15000.00m, "Transferencia recibida de Alejandro Silva", new DateTime(2026, 8, 10, 14, 30, 0, 0, DateTimeKind.Utc), 4, 2 },
                    { 8, 5, 25000.00m, "Transferencia enviada a Alejandro Silva", new DateTime(2026, 8, 18, 11, 15, 0, 0, DateTimeKind.Utc), 4, 3 },
                    { 9, 4, 25000.00m, "Transferencia recibida de Micaela Mulato", new DateTime(2026, 8, 18, 11, 15, 0, 0, DateTimeKind.Utc), 5, 2 },
                    { 10, 4, 12000.00m, "Transferencia enviada a Emmanuel Torres", new DateTime(2026, 8, 25, 16, 45, 0, 0, DateTimeKind.Utc), 6, 3 },
                    { 11, 6, 12000.00m, "Transferencia recibida de Alejandro Silva", new DateTime(2026, 8, 25, 16, 45, 0, 0, DateTimeKind.Utc), 4, 2 },
                    { 12, 4, 5000.00m, "Transferencia enviada a Micaela Mulato", new DateTime(2026, 9, 1, 18, 20, 0, 0, DateTimeKind.Utc), 5, 3 },
                    { 13, 5, 5000.00m, "Transferencia recibida de Alejandro Silva", new DateTime(2026, 9, 1, 18, 20, 0, 0, DateTimeKind.Utc), 4, 2 },
                    { 14, 3, 8500.00m, "Transferencia enviada a Alejandro Silva", new DateTime(2026, 9, 2, 10, 0, 0, 0, DateTimeKind.Utc), 4, 3 },
                    { 15, 4, 8500.00m, "Transferencia recibida de Mohammed Khan", new DateTime(2026, 9, 2, 10, 0, 0, 0, DateTimeKind.Utc), 3, 2 },
                    { 16, 6, 6000.00m, "Transferencia enviada a Roberto Carlos", new DateTime(2026, 9, 3, 9, 30, 0, 0, DateTimeKind.Utc), 2, 3 },
                    { 17, 2, 6000.00m, "Transferencia recibida de Emmanuel Torres", new DateTime(2026, 9, 3, 9, 30, 0, 0, DateTimeKind.Utc), 6, 2 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { 2, 4 });

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { 2, 5 });

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { 2, 6 });

            migrationBuilder.DeleteData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 6);
        }
    }
}
