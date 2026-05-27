using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DreamsPools.API.Migrations
{
    /// <inheritdoc />
    public partial class Editsss : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Banners",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: true),
                    IsVisible = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Banners", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Banners_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                });

            migrationBuilder.UpdateData(
                table: "Admins",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 5, 25, 23, 40, 41, 271, DateTimeKind.Utc).AddTicks(5692), "$2a$11$PIelF0CUD.BdVAZc7TSv7.rb4UA3QgM8YqGsiqGQA/kufAAnFBUrO" });

            migrationBuilder.UpdateData(
                table: "AppSettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 25, 23, 40, 41, 271, DateTimeKind.Utc).AddTicks(6386));

            migrationBuilder.UpdateData(
                table: "AppSettings",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 25, 23, 40, 41, 271, DateTimeKind.Utc).AddTicks(6388));

            migrationBuilder.UpdateData(
                table: "AppSettings",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 25, 23, 40, 41, 271, DateTimeKind.Utc).AddTicks(6390));

            migrationBuilder.UpdateData(
                table: "AppSettings",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 25, 23, 40, 41, 271, DateTimeKind.Utc).AddTicks(6392));

            migrationBuilder.CreateIndex(
                name: "IX_Banners_ProductId",
                table: "Banners",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Banners");

            migrationBuilder.UpdateData(
                table: "Admins",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 5, 23, 16, 43, 53, 714, DateTimeKind.Utc).AddTicks(9745), "$2a$11$36MaBADjIzlqy0zMn3A9o.NaFuik4ZRDrJJVdbQjlDPrto5JB.BVe" });

            migrationBuilder.UpdateData(
                table: "AppSettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 23, 16, 43, 53, 715, DateTimeKind.Utc).AddTicks(553));

            migrationBuilder.UpdateData(
                table: "AppSettings",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 23, 16, 43, 53, 715, DateTimeKind.Utc).AddTicks(556));

            migrationBuilder.UpdateData(
                table: "AppSettings",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 23, 16, 43, 53, 715, DateTimeKind.Utc).AddTicks(559));

            migrationBuilder.UpdateData(
                table: "AppSettings",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 23, 16, 43, 53, 715, DateTimeKind.Utc).AddTicks(561));
        }
    }
}
