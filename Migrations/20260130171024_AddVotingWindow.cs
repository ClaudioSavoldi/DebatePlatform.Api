using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DebatePlatform.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddVotingWindow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ClosedAt",
                table: "DebateMatches",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "VotingEndsAt",
                table: "DebateMatches",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClosedAt",
                table: "DebateMatches");

            migrationBuilder.DropColumn(
                name: "VotingEndsAt",
                table: "DebateMatches");
        }
    }
}
