using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSiteStatusAndOnboarding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ActivatedAt",
                schema: "core",
                table: "Sites",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OnboardingNotes",
                schema: "core",
                table: "Sites",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                schema: "core",
                table: "Sites",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Sites_Status",
                schema: "core",
                table: "Sites",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sites_Status",
                schema: "core",
                table: "Sites");

            migrationBuilder.DropColumn(
                name: "ActivatedAt",
                schema: "core",
                table: "Sites");

            migrationBuilder.DropColumn(
                name: "OnboardingNotes",
                schema: "core",
                table: "Sites");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "core",
                table: "Sites");
        }
    }
}
