using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPendingSiteUploads : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PendingSiteUploads",
                schema: "data",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SiteId = table.Column<int>(type: "integer", nullable: false),
                    UploadedByUserId = table.Column<int>(type: "integer", nullable: false),
                    FileName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    BlobPath = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    MappingResultJson = table.Column<string>(type: "jsonb", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ConfirmedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConfirmedByUserId = table.Column<int>(type: "integer", nullable: true),
                    ResultingSchemaVersionId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PendingSiteUploads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PendingSiteUploads_SiteDataSchemas_ResultingSchemaVersionId",
                        column: x => x.ResultingSchemaVersionId,
                        principalSchema: "config",
                        principalTable: "SiteDataSchemas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PendingSiteUploads_Sites_SiteId",
                        column: x => x.SiteId,
                        principalSchema: "core",
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PendingSiteUploads_Users_ConfirmedByUserId",
                        column: x => x.ConfirmedByUserId,
                        principalSchema: "core",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PendingSiteUploads_Users_UploadedByUserId",
                        column: x => x.UploadedByUserId,
                        principalSchema: "core",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PendingSiteUploads_ConfirmedByUserId",
                schema: "data",
                table: "PendingSiteUploads",
                column: "ConfirmedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PendingSiteUploads_ResultingSchemaVersionId",
                schema: "data",
                table: "PendingSiteUploads",
                column: "ResultingSchemaVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_PendingSiteUploads_SiteId_Status",
                schema: "data",
                table: "PendingSiteUploads",
                columns: new[] { "SiteId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_PendingSiteUploads_Status",
                schema: "data",
                table: "PendingSiteUploads",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PendingSiteUploads_UploadedByUserId",
                schema: "data",
                table: "PendingSiteUploads",
                column: "UploadedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PendingSiteUploads",
                schema: "data");
        }
    }
}
