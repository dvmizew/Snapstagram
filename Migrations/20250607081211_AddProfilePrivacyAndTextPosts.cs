using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Snapstagram.Migrations
{
    /// <inheritdoc />
    public partial class AddProfilePrivacyAndTextPosts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "4", "admin-id" });

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "4");

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<bool>(
                name: "IsProfilePublic",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "3",
                columns: new[] { "Name", "NormalizedName" },
                values: new object[] { "Administrator", "ADMINISTRATOR" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "3", "admin-id" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-id",
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "IsProfilePublic", "PasswordHash", "SecurityStamp" },
                values: new object[] { "218ae101-39b8-4ff4-a8af-1572bfeece15", new DateTime(2025, 6, 7, 8, 12, 11, 1, DateTimeKind.Utc).AddTicks(8096), true, "AQAAAAIAAYagAAAAEP5XTEkqs3FXnnm0Qhsr5SRjg++cgYFtS59IinS/DQHYrx3H2FbuexzL9HWlUNUNKA==", "c7ef86f3-c226-437d-b91e-916579781b53" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "3", "admin-id" });

            migrationBuilder.DropColumn(
                name: "IsProfilePublic",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "3",
                columns: new[] { "Name", "NormalizedName" },
                values: new object[] { "Moderator", "MODERATOR" });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "4", null, "Administrator", "ADMINISTRATOR" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-id",
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp" },
                values: new object[] { "f57cc8ab-2bd5-4983-b89b-69afcda706b1", new DateTime(2025, 6, 6, 21, 40, 39, 883, DateTimeKind.Utc).AddTicks(8194), "AQAAAAIAAYagAAAAEBt1q7Yo2kqJ1YkqEfPDhWCV3Mjo0mPjfetiQWOeNSVxVH3jg9jjZQKr+dnjRWwCqg==", "eb0012b1-0f25-40a9-9f5d-ed76b315fc5b" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "4", "admin-id" });
        }
    }
}
