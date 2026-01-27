using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DebatePlatform.Api.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDomainUserAndUserRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Debates_Users_CreatedByUserId",
                table: "Debates");

            migrationBuilder.DropForeignKey(
                name: "FK_DebateStatusHistories_Users_ChangedByUserId",
                table: "DebateStatusHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_Votes_Users_UserId",
                table: "Votes");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Votes_UserId",
                table: "Votes");

            migrationBuilder.DropIndex(
                name: "IX_DebateStatusHistories_ChangedByUserId",
                table: "DebateStatusHistories");

            migrationBuilder.DropIndex(
                name: "IX_Debates_CreatedByUserId",
                table: "Debates");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Votes_UserId",
                table: "Votes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DebateStatusHistories_ChangedByUserId",
                table: "DebateStatusHistories",
                column: "ChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Debates_CreatedByUserId",
                table: "Debates",
                column: "CreatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Debates_Users_CreatedByUserId",
                table: "Debates",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DebateStatusHistories_Users_ChangedByUserId",
                table: "DebateStatusHistories",
                column: "ChangedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Votes_Users_UserId",
                table: "Votes",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
