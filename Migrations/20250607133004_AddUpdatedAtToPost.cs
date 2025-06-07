using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Snapstagram.Migrations
{
    /// <inheritdoc />
    public partial class AddUpdatedAtToPost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Posts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-id",
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp" },
                values: new object[] { "67cd76d1-a291-4efe-9c8a-4ad5e2e1a49b", new DateTime(2025, 6, 7, 13, 30, 3, 571, DateTimeKind.Utc).AddTicks(4326), "AQAAAAIAAYagAAAAEJ9ozNz6q8fb3WLZX+75zVaqPScTcLaQYx6WgliDIpuaZlYwh2WUaPklzZjth9H4xw==", "2eedb3b5-f292-4495-94a9-fa327397f15e" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Posts");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-id",
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp" },
                values: new object[] { "e2f661df-5ca5-44c7-a615-db053aa5e8e0", new DateTime(2025, 6, 7, 11, 57, 11, 599, DateTimeKind.Utc).AddTicks(5474), "AQAAAAIAAYagAAAAEBjPOtviCc7vLWptF4m5UgQbCvG75vby8wgRyeMHRXLCZBV0EIPvt14eFet7R2IMGg==", "58d28823-ac69-49c4-af55-802f44a89d76" });
        }
    }
}
