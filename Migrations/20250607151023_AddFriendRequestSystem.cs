using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Snapstagram.Migrations
{
    /// <inheritdoc />
    public partial class AddFriendRequestSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FriendRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SenderId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ReceiverId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RespondedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FriendRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FriendRequests_AspNetUsers_ReceiverId",
                        column: x => x.ReceiverId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FriendRequests_AspNetUsers_SenderId",
                        column: x => x.SenderId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-id",
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp" },
                values: new object[] { "8f287d32-3bb1-43f5-b843-aad62d371e6d", new DateTime(2025, 6, 7, 15, 10, 23, 329, DateTimeKind.Utc).AddTicks(3408), "AQAAAAIAAYagAAAAEGO2PV+HTF4e5dZIGdY52VwZxWL+7ftoj3L50/3LlKUiZp8TlIbwk0R4NV1Zkzf4Ww==", "61471e88-6181-4730-9c8e-192b9f74e587" });

            migrationBuilder.CreateIndex(
                name: "IX_FriendRequests_ReceiverId",
                table: "FriendRequests",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_FriendRequests_SenderId_ReceiverId",
                table: "FriendRequests",
                columns: new[] { "SenderId", "ReceiverId" },
                unique: true,
                filter: "[SenderId] IS NOT NULL AND [ReceiverId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FriendRequests");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-id",
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp" },
                values: new object[] { "46b53b8b-8219-4a2d-a62c-23dcbdb773f4", new DateTime(2025, 6, 7, 14, 33, 9, 19, DateTimeKind.Utc).AddTicks(1889), "AQAAAAIAAYagAAAAEGdhzxnwSUr0cMTM+OjvTQjxaD6Tv9d7kwUwzlRWdPxdajrM9fIqbHXehLxRhey2pA==", "39605ae0-c85c-49a8-bbab-cef56ddbdcc4" });
        }
    }
}
