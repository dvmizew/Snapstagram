using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Snapstagram.Migrations
{
    /// <inheritdoc />
    public partial class AddUpdatedAtToComment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Comments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-id",
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp" },
                values: new object[] { "46b53b8b-8219-4a2d-a62c-23dcbdb773f4", new DateTime(2025, 6, 7, 14, 33, 9, 19, DateTimeKind.Utc).AddTicks(1889), "AQAAAAIAAYagAAAAEGdhzxnwSUr0cMTM+OjvTQjxaD6Tv9d7kwUwzlRWdPxdajrM9fIqbHXehLxRhey2pA==", "39605ae0-c85c-49a8-bbab-cef56ddbdcc4" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Comments");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-id",
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp" },
                values: new object[] { "4f1abbd3-41b4-4ce4-8933-fc0c330c3b36", new DateTime(2025, 6, 7, 14, 9, 23, 181, DateTimeKind.Utc).AddTicks(9195), "AQAAAAIAAYagAAAAEN6DIEnrqeJm2m3Q/kdFk2ABidAqk7yAfxyIHqUse+mFY6tHTAGz5wvIKrMavax9sw==", "5728a970-ccc0-4c4a-a448-019c3278c684" });
        }
    }
}
