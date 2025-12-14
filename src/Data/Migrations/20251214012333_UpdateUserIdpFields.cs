using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserIdpFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdpIssuer",
                schema: "core",
                table: "Users",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdpSub",
                schema: "core",
                table: "Users",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                schema: "core",
                table: "Users",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                schema: "core",
                table: "Users",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_Users_IdpIssuer_IdpSub",
                schema: "core",
                table: "Users",
                columns: new[] { "IdpIssuer", "IdpSub" },
                unique: true,
                filter: "\"IdpSub\" IS NOT NULL AND \"IdpIssuer\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_IdpIssuer_IdpSub",
                schema: "core",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IdpIssuer",
                schema: "core",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IdpSub",
                schema: "core",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Name",
                schema: "core",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                schema: "core",
                table: "Users");
        }
    }
}
