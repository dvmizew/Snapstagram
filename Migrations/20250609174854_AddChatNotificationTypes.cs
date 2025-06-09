using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Snapstagram.Migrations
{
    /// <inheritdoc />
    public partial class AddChatNotificationTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-id",
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp" },
                values: new object[] { "8cad76c9-a13d-43e9-a537-9010358bcb5b", new DateTime(2025, 6, 9, 17, 48, 53, 523, DateTimeKind.Utc).AddTicks(6225), "AQAAAAIAAYagAAAAEGv7QiIy1baAiif0i1yBtz8hXW9L7NHDzvgwmdC/+bl9m2heKmrBPAsTuOdVPVWy4w==", "5d6f17c1-5baa-4b9c-9452-cb4393544aae" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-id",
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp" },
                values: new object[] { "13a4b907-ed3a-4d06-b5a9-91fe7139245e", new DateTime(2025, 6, 9, 17, 33, 20, 901, DateTimeKind.Utc).AddTicks(7957), "AQAAAAIAAYagAAAAEKjiJEe6oUZLfpbE8DPcjSs2Tq78B/IbYmlw8ss1IajKzKja/VWw60Jkp6CxOaZHXg==", "841f5a30-ede8-4ef9-8d73-532c9c8c9e5c" });
        }
    }
}
