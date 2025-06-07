using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Snapstagram.Migrations
{
    /// <inheritdoc />
    public partial class AddUserProfileFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Occupation",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Website",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-id",
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "DateOfBirth", "Location", "Occupation", "PasswordHash", "SecurityStamp", "Website" },
                values: new object[] { "86f54cdf-5572-46c0-ad5c-650c9bc5949b", new DateTime(2025, 6, 7, 9, 19, 22, 474, DateTimeKind.Utc).AddTicks(1399), null, null, null, "AQAAAAIAAYagAAAAEGuhkLte1Iu1PhoTxRXjcI3QMY9R56fAYM1v4DF4IVzs4DQBR097oPNruqupogrzLA==", "60292822-84e1-49d4-ad58-87dc5c4d3268", null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Occupation",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Website",
                table: "AspNetUsers");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-id",
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp" },
                values: new object[] { "218ae101-39b8-4ff4-a8af-1572bfeece15", new DateTime(2025, 6, 7, 8, 12, 11, 1, DateTimeKind.Utc).AddTicks(8096), "AQAAAAIAAYagAAAAEP5XTEkqs3FXnnm0Qhsr5SRjg++cgYFtS59IinS/DQHYrx3H2FbuexzL9HWlUNUNKA==", "c7ef86f3-c226-437d-b91e-916579781b53" });
        }
    }
}
