using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Snapstagram.Migrations
{
    /// <inheritdoc />
    public partial class UpdateNotificationTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-id",
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp" },
                values: new object[] { "4f1abbd3-41b4-4ce4-8933-fc0c330c3b36", new DateTime(2025, 6, 7, 14, 9, 23, 181, DateTimeKind.Utc).AddTicks(9195), "AQAAAAIAAYagAAAAEN6DIEnrqeJm2m3Q/kdFk2ABidAqk7yAfxyIHqUse+mFY6tHTAGz5wvIKrMavax9sw==", "5728a970-ccc0-4c4a-a448-019c3278c684" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-id",
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp" },
                values: new object[] { "86020a35-a904-4b8b-b843-af97025194f3", new DateTime(2025, 6, 7, 14, 5, 4, 222, DateTimeKind.Utc).AddTicks(250), "AQAAAAIAAYagAAAAEKWhSqSgRrRX/TIIyIt+5rbipz5kHA135TlDzLZj6WQ1Bb+Nx9llVerX9tygfu1gKA==", "e0b47ab4-6f78-4327-897c-120f366606b6" });
        }
    }
}
