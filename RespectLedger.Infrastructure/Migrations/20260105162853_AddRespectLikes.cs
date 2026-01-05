using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RespectLedger.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRespectLikes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RespectLikes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RespectId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RespectLikes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RespectLikes_Respects_RespectId",
                        column: x => x.RespectId,
                        principalTable: "Respects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RespectLikes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RespectLikes_RespectId_UserId",
                table: "RespectLikes",
                columns: new[] { "RespectId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RespectLikes_UserId",
                table: "RespectLikes",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RespectLikes");
        }
    }
}
