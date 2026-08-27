using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DigitalArs.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Description", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { 1, "b9b271b3-438c-4bf7-a728-49464f787828", "Administrador con permisos elevados para gestionar usuarios", "Admin", "ADMIN" },
                    { 2, "5e5705da-92b0-43c5-a07c-19a0c038dcf2", "Usuario estandar de la billetera virtual", "User", "USER" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "CreatedAt", "Email", "EmailConfirmed", "FirstName", "LastName", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "RoleId", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[,]
                {
                    { 1, 0, "seed-admin-concurrency", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin@digitalars.com", true, "Admin", "DigitalArs", false, null, "ADMIN@DIGITALARS.COM", "ADMIN@DIGITALARS.COM", "AQAAAAIAAYagAAAAENO87HGO7ibu/kR6bblZLBu39LF1P9oeSEu7bwGb0YRvny7KouBk+XFrlxztTvecMQ==", null, false, 1, "SEED-ADMIN-SECURITY-STAMP", false, "admin@digitalars.com" },
                    { 2, 0, "seed-user1-concurrency", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "robercarlos3@gmail.com", true, "Roberto", "Carlos", false, null, "ROBERCARLOS3@GMAIL.COM", "ROBERCARLOS3@GMAIL.COM", "AQAAAAIAAYagAAAAELKOHh9OA8J6+VbOTSSbRWvcAbbwaPQ9dZcgFx02YAC1LeN/DPShnsiygednoXJUNQ==", null, false, 2, "SEED-USER1-SECURITY-STAMP", false, "robercarlos3@gmail.com" },
                    { 3, 0, "seed-user2-concurrency", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "mokha@gmail.com", true, "Mohammed", "Khan", false, null, "MOKHA@GMAIL.COM", "MOKHA@GMAIL.COM", "AQAAAAIAAYagAAAAEErdfRgSxQZB1799p6YXG2T/bLL4bqGYPBRcYHsKas3tZrZUfw1cn6bK9oGtgvtkhA==", null, false, 2, "SEED-USER2-SECURITY-STAMP", false, "mokha@gmail.com" }
                });

            migrationBuilder.InsertData(
                table: "Accounts",
                columns: new[] { "Id", "Money", "UserId" },
                values: new object[,]
                {
                    { 1, 500000.00m, 1 },
                    { 2, 260000.00m, 2 },
                    { 3, 185000.50m, 3 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
