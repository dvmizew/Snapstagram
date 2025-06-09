using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Snapstagram.Migrations
{
    /// <inheritdoc />
    public partial class AddSenderNameToNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SenderName",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-id",
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp" },
                values: new object[] { "5a4e71c2-d28b-4494-8c1b-fd45e7ce1360", new DateTime(2025, 6, 9, 19, 5, 45, 443, DateTimeKind.Utc).AddTicks(2558), "AQAAAAIAAYagAAAAEODMPTQssrvPngvaAx6yA0y7Z6C2/jzLXwArOo80hgAdAzk6rL+hhaQKF8EOGZv7FA==", "e25ee7f6-76d7-4ce6-9b4c-73a737ce41de" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SenderName",
                table: "Notifications");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-id",
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp" },
                values: new object[] { "8cad76c9-a13d-43e9-a537-9010358bcb5b", new DateTime(2025, 6, 9, 17, 48, 53, 523, DateTimeKind.Utc).AddTicks(6225), "AQAAAAIAAYagAAAAEGv7QiIy1baAiif0i1yBtz8hXW9L7NHDzvgwmdC/+bl9m2heKmrBPAsTuOdVPVWy4w==", "5d6f17c1-5baa-4b9c-9452-cb4393544aae" });
        }
    }
}
