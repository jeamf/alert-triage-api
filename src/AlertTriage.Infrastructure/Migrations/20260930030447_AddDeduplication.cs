using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlertTriage.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDeduplication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastSeenAtUtc",
                table: "Alerts",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "OccurrenceCount",
                table: "Alerts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_LastSeenAtUtc",
                table: "Alerts",
                column: "LastSeenAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Alerts_LastSeenAtUtc",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "LastSeenAtUtc",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "OccurrenceCount",
                table: "Alerts");
        }
    }
}
