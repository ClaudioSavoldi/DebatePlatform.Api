using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DebatePlatform.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchmakingCore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DebateMatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DebateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Phase = table.Column<int>(type: "int", nullable: false),
                    ProUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ControUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DebateMatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DebateMatches_Debates_DebateId",
                        column: x => x.DebateId,
                        principalTable: "Debates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DebateQueueEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DebateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Side = table.Column<int>(type: "int", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DebateQueueEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DebateQueueEntries_Debates_DebateId",
                        column: x => x.DebateId,
                        principalTable: "Debates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MatchSubmissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Phase = table.Column<int>(type: "int", nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchSubmissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MatchSubmissions_DebateMatches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "DebateMatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DebateMatches_DebateId",
                table: "DebateMatches",
                column: "DebateId");

            migrationBuilder.CreateIndex(
                name: "IX_DebateQueueEntries_DebateId_Side_JoinedAt",
                table: "DebateQueueEntries",
                columns: new[] { "DebateId", "Side", "JoinedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_DebateQueueEntries_DebateId_UserId",
                table: "DebateQueueEntries",
                columns: new[] { "DebateId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MatchSubmissions_MatchId_UserId_Phase",
                table: "MatchSubmissions",
                columns: new[] { "MatchId", "UserId", "Phase" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DebateQueueEntries");

            migrationBuilder.DropTable(
                name: "MatchSubmissions");

            migrationBuilder.DropTable(
                name: "DebateMatches");
        }
    }
}
