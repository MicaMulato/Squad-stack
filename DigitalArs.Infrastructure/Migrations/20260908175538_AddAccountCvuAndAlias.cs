using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalArs.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountCvuAndAlias : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Alias",
                table: "Accounts",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Cvu",
                table: "Accounts",
                type: "nvarchar(22)",
                maxLength: 22,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Alias", "Cvu" },
                values: new object[] { "admin.digital.ars", "0000003100010000000001" });

            migrationBuilder.UpdateData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Alias", "Cvu" },
                values: new object[] { "roberto.carlos.ars", "0000003100010000000002" });

            migrationBuilder.UpdateData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Alias", "Cvu" },
                values: new object[] { "mohammed.khan.ars", "0000003100010000000003" });

            migrationBuilder.UpdateData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Alias", "Cvu" },
                values: new object[] { "alejandro.silva.ars", "0000003100010000000004" });

            migrationBuilder.UpdateData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Alias", "Cvu" },
                values: new object[] { "micaela.mulato.ars", "0000003100010000000005" });

            migrationBuilder.UpdateData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Alias", "Cvu" },
                values: new object[] { "emmanuel.torres.ars", "0000003100010000000006" });

            // Rellenar automáticamente cuentas dinámicas existentes que tengan Alias o CVU vacíos
            migrationBuilder.Sql("UPDATE [Accounts] SET [Alias] = CONCAT('cuenta.', [Id], '.ars'), [Cvu] = CONCAT('0000003100010000', RIGHT('000000' + CAST([Id] AS VARCHAR(6)), 6)) WHERE [Alias] = '' OR [Alias] IS NULL OR [Cvu] = '' OR [Cvu] IS NULL;");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Alias",
                table: "Accounts",
                column: "Alias",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Cvu",
                table: "Accounts",
                column: "Cvu",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Accounts_Alias",
                table: "Accounts");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_Cvu",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "Alias",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "Cvu",
                table: "Accounts");
        }
    }
}
