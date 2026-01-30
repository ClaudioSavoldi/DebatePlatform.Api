using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DebatePlatform.Api.Migrations
{
    /// <inheritdoc />
    public partial class VotesUseMatchId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Votes_Debates_DebateId",
                table: "Votes");

            migrationBuilder.DropIndex(
                name: "IX_Votes_DebateId_UserId",
                table: "Votes");

            migrationBuilder.AlterColumn<Guid>(
                name: "DebateId",
                table: "Votes",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "MatchId",
                table: "Votes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Votes_DebateId",
                table: "Votes",
                column: "DebateId");

            migrationBuilder.CreateIndex(
                name: "IX_Votes_MatchId_UserId",
                table: "Votes",
                columns: new[] { "MatchId", "UserId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Votes_DebateMatches_MatchId",
                table: "Votes",
                column: "MatchId",
                principalTable: "DebateMatches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Votes_Debates_DebateId",
                table: "Votes",
                column: "DebateId",
                principalTable: "Debates",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Votes_DebateMatches_MatchId",
                table: "Votes");

            migrationBuilder.DropForeignKey(
                name: "FK_Votes_Debates_DebateId",
                table: "Votes");

            migrationBuilder.DropIndex(
                name: "IX_Votes_DebateId",
                table: "Votes");

            migrationBuilder.DropIndex(
                name: "IX_Votes_MatchId_UserId",
                table: "Votes");

            migrationBuilder.DropColumn(
                name: "MatchId",
                table: "Votes");

            migrationBuilder.AlterColumn<Guid>(
                name: "DebateId",
                table: "Votes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Votes_DebateId_UserId",
                table: "Votes",
                columns: new[] { "DebateId", "UserId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Votes_Debates_DebateId",
                table: "Votes",
                column: "DebateId",
                principalTable: "Debates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
