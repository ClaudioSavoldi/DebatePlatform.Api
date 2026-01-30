using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DebatePlatform.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddSubmissionDraftFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSubmitted",
                table: "MatchSubmissions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubmittedAt",
                table: "MatchSubmissions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "MatchSubmissions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSubmitted",
                table: "MatchSubmissions");

            migrationBuilder.DropColumn(
                name: "SubmittedAt",
                table: "MatchSubmissions");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "MatchSubmissions");
        }
    }
}
