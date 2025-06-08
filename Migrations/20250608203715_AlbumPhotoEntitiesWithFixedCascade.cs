using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Snapstagram.Migrations
{
    /// <inheritdoc />
    public partial class AlbumPhotoEntitiesWithFixedCascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AlbumPhotoComments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PhotoId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlbumPhotoComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlbumPhotoComments_AlbumPhotos_PhotoId",
                        column: x => x.PhotoId,
                        principalTable: "AlbumPhotos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlbumPhotoComments_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AlbumPhotoLikes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PhotoId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlbumPhotoLikes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlbumPhotoLikes_AlbumPhotos_PhotoId",
                        column: x => x.PhotoId,
                        principalTable: "AlbumPhotos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlbumPhotoLikes_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-id",
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp" },
                values: new object[] { "22065e9c-4820-440c-a005-f992574156d7", new DateTime(2025, 6, 8, 20, 37, 13, 397, DateTimeKind.Utc).AddTicks(9460), "AQAAAAIAAYagAAAAEEMD2zN3IeTdPvywgUIX+qNTyataTgBkHw0StHKUHrPFgE6nA7ZNGtn9bVUwtgh0FQ==", "ce700dba-4d16-4a72-8370-c68469e79be7" });

            migrationBuilder.CreateIndex(
                name: "IX_AlbumPhotoComments_PhotoId",
                table: "AlbumPhotoComments",
                column: "PhotoId");

            migrationBuilder.CreateIndex(
                name: "IX_AlbumPhotoComments_UserId",
                table: "AlbumPhotoComments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AlbumPhotoLikes_PhotoId",
                table: "AlbumPhotoLikes",
                column: "PhotoId");

            migrationBuilder.CreateIndex(
                name: "IX_AlbumPhotoLikes_UserId",
                table: "AlbumPhotoLikes",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlbumPhotoComments");

            migrationBuilder.DropTable(
                name: "AlbumPhotoLikes");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-id",
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp" },
                values: new object[] { "6f6048de-feda-40bf-8c54-fbb898476c73", new DateTime(2025, 6, 8, 20, 18, 41, 574, DateTimeKind.Utc).AddTicks(1735), "AQAAAAIAAYagAAAAEJspQFDdIgblcwZMA69v/C+CB55FzMSwG/f79jd0UEM6pPha3oYKGcEztfAy0PQ+Mw==", "ca9342d6-f096-4892-afeb-260aa6728071" });
        }
    }
}
