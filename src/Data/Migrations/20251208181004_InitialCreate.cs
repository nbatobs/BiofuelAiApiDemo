using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "core");

            migrationBuilder.EnsureSchema(
                name: "config");

            migrationBuilder.EnsureSchema(
                name: "data");

            migrationBuilder.EnsureSchema(
                name: "ml");

            migrationBuilder.CreateTable(
                name: "Companies",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsIndividual = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastLogin = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "core",
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Dashboards",
                schema: "config",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SiteId = table.Column<int>(type: "integer", nullable: false),
                    CreatedById = table.Column<int>(type: "integer", nullable: true),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PlotlyConfigJson = table.Column<string>(type: "jsonb", nullable: true),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastViewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dashboards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dashboards_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalSchema: "core",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "DataCleaningRules",
                schema: "config",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SiteId = table.Column<int>(type: "integer", nullable: false),
                    RuleType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ConfigJson = table.Column<string>(type: "jsonb", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    CreatedById = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    VersionNumber = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataCleaningRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataCleaningRules_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalSchema: "core",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "DataRows",
                schema: "data",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SiteId = table.Column<int>(type: "integer", nullable: false),
                    SchemaVersionId = table.Column<int>(type: "integer", nullable: true),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SensorDataJson = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataRows", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DataValidationRules",
                schema: "config",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SiteId = table.Column<int>(type: "integer", nullable: false),
                    ColumnName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RuleType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ConfigJson = table.Column<string>(type: "jsonb", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    CreatedById = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataValidationRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataValidationRules_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalSchema: "core",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "InferenceRequests",
                schema: "ml",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SiteId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    InputDataJson = table.Column<string>(type: "jsonb", nullable: false),
                    PredictionResultId = table.Column<int>(type: "integer", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DurationMs = table.Column<long>(type: "bigint", nullable: true),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InferenceRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InferenceRequests_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "core",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ModelVersions",
                schema: "ml",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SiteId = table.Column<int>(type: "integer", nullable: false),
                    BlobStoragePath = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    TrainedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TrainingDataStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TrainingDataEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MetricsJson = table.Column<string>(type: "jsonb", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    VersionNumber = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModelVersions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PredictionResults",
                schema: "ml",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SiteId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    ModelVersionId = table.Column<int>(type: "integer", nullable: true),
                    ScenarioName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ScenarioDescription = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ScenarioParametersJson = table.Column<string>(type: "jsonb", nullable: true),
                    InputDataJson = table.Column<string>(type: "jsonb", nullable: false),
                    PredictionOutputJson = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DurationMs = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PredictionResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PredictionResults_ModelVersions_ModelVersionId",
                        column: x => x.ModelVersionId,
                        principalSchema: "ml",
                        principalTable: "ModelVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PredictionResults_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "core",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SiteDataSchemas",
                schema: "config",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SiteId = table.Column<int>(type: "integer", nullable: false),
                    VersionNumber = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    SchemaDefinition = table.Column<string>(type: "jsonb", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ChangeDescription = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedById = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteDataSchemas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SiteDataSchemas_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalSchema: "core",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Sites",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    SiteName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Location = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    TimeZone = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CurrentSchemaVersionId = table.Column<int>(type: "integer", nullable: true),
                    ConfigJson = table.Column<string>(type: "jsonb", nullable: false),
                    AutoInferenceEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    InferenceSchedule = table.Column<TimeSpan>(type: "interval", nullable: true),
                    AutoRetrainingEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    RetrainingFrequencyDays = table.Column<int>(type: "integer", nullable: true),
                    TrainOnEveryUpload = table.Column<bool>(type: "boolean", nullable: false),
                    PowerBiEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    PowerBiWorkspaceId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sites_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "core",
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Sites_SiteDataSchemas_CurrentSchemaVersionId",
                        column: x => x.CurrentSchemaVersionId,
                        principalSchema: "config",
                        principalTable: "SiteDataSchemas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "TrainingJobs",
                schema: "ml",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SiteId = table.Column<int>(type: "integer", nullable: false),
                    ScheduledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TrainingDataStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TrainingDataEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModelVersionId = table.Column<int>(type: "integer", nullable: true),
                    ConfigJson = table.Column<string>(type: "jsonb", nullable: false),
                    TriggeredById = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainingJobs_ModelVersions_ModelVersionId",
                        column: x => x.ModelVersionId,
                        principalSchema: "ml",
                        principalTable: "ModelVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TrainingJobs_Sites_SiteId",
                        column: x => x.SiteId,
                        principalSchema: "core",
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrainingJobs_Users_TriggeredById",
                        column: x => x.TriggeredById,
                        principalSchema: "core",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Uploads",
                schema: "data",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SiteId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FileName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    RowsParsed = table.Column<int>(type: "integer", nullable: false),
                    RowsInserted = table.Column<int>(type: "integer", nullable: false),
                    ValidationStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Uploads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Uploads_Sites_SiteId",
                        column: x => x.SiteId,
                        principalSchema: "core",
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Uploads_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "core",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "UserSiteAccess",
                schema: "core",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    SiteId = table.Column<int>(type: "integer", nullable: false),
                    RoleOnSite = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSiteAccess", x => new { x.UserId, x.SiteId });
                    table.ForeignKey(
                        name: "FK_UserSiteAccess_Sites_SiteId",
                        column: x => x.SiteId,
                        principalSchema: "core",
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserSiteAccess_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "core",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Companies_Name",
                schema: "core",
                table: "Companies",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Dashboards_CreatedById",
                schema: "config",
                table: "Dashboards",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Dashboards_SiteId_IsPublic",
                schema: "config",
                table: "Dashboards",
                columns: new[] { "SiteId", "IsPublic" });

            migrationBuilder.CreateIndex(
                name: "IX_DataCleaningRules_CreatedById",
                schema: "config",
                table: "DataCleaningRules",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_DataCleaningRules_SiteId_IsActive_Priority",
                schema: "config",
                table: "DataCleaningRules",
                columns: new[] { "SiteId", "IsActive", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_DataRows_SchemaVersionId",
                schema: "data",
                table: "DataRows",
                column: "SchemaVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_DataRows_SiteId_Date",
                schema: "data",
                table: "DataRows",
                columns: new[] { "SiteId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_DataValidationRules_CreatedById",
                schema: "config",
                table: "DataValidationRules",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_DataValidationRules_SiteId_IsActive_Priority",
                schema: "config",
                table: "DataValidationRules",
                columns: new[] { "SiteId", "IsActive", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_InferenceRequests_PredictionResultId",
                schema: "ml",
                table: "InferenceRequests",
                column: "PredictionResultId");

            migrationBuilder.CreateIndex(
                name: "IX_InferenceRequests_SiteId_RequestedAt",
                schema: "ml",
                table: "InferenceRequests",
                columns: new[] { "SiteId", "RequestedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_InferenceRequests_Status_RequestedAt",
                schema: "ml",
                table: "InferenceRequests",
                columns: new[] { "Status", "RequestedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_InferenceRequests_UserId",
                schema: "ml",
                table: "InferenceRequests",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ModelVersions_SiteId_IsActive",
                schema: "ml",
                table: "ModelVersions",
                columns: new[] { "SiteId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_ModelVersions_SiteId_VersionNumber",
                schema: "ml",
                table: "ModelVersions",
                columns: new[] { "SiteId", "VersionNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_PredictionResults_ModelVersionId",
                schema: "ml",
                table: "PredictionResults",
                column: "ModelVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_PredictionResults_SiteId_CreatedAt",
                schema: "ml",
                table: "PredictionResults",
                columns: new[] { "SiteId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PredictionResults_UserId",
                schema: "ml",
                table: "PredictionResults",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SiteDataSchemas_CreatedById",
                schema: "config",
                table: "SiteDataSchemas",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_SiteDataSchemas_SiteId_VersionNumber",
                schema: "config",
                table: "SiteDataSchemas",
                columns: new[] { "SiteId", "VersionNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_Sites_CompanyId",
                schema: "core",
                table: "Sites",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Sites_CurrentSchemaVersionId",
                schema: "core",
                table: "Sites",
                column: "CurrentSchemaVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingJobs_ModelVersionId",
                schema: "ml",
                table: "TrainingJobs",
                column: "ModelVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingJobs_SiteId_ScheduledAt",
                schema: "ml",
                table: "TrainingJobs",
                columns: new[] { "SiteId", "ScheduledAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TrainingJobs_Status_ScheduledAt",
                schema: "ml",
                table: "TrainingJobs",
                columns: new[] { "Status", "ScheduledAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TrainingJobs_TriggeredById",
                schema: "ml",
                table: "TrainingJobs",
                column: "TriggeredById");

            migrationBuilder.CreateIndex(
                name: "IX_Uploads_SiteId_UploadedAt",
                schema: "data",
                table: "Uploads",
                columns: new[] { "SiteId", "UploadedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Uploads_UserId",
                schema: "data",
                table: "Uploads",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_CompanyId",
                schema: "core",
                table: "Users",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                schema: "core",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSiteAccess_SiteId",
                schema: "core",
                table: "UserSiteAccess",
                column: "SiteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Dashboards_Sites_SiteId",
                schema: "config",
                table: "Dashboards",
                column: "SiteId",
                principalSchema: "core",
                principalTable: "Sites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DataCleaningRules_Sites_SiteId",
                schema: "config",
                table: "DataCleaningRules",
                column: "SiteId",
                principalSchema: "core",
                principalTable: "Sites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DataRows_SiteDataSchemas_SchemaVersionId",
                schema: "data",
                table: "DataRows",
                column: "SchemaVersionId",
                principalSchema: "config",
                principalTable: "SiteDataSchemas",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_DataRows_Sites_SiteId",
                schema: "data",
                table: "DataRows",
                column: "SiteId",
                principalSchema: "core",
                principalTable: "Sites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DataValidationRules_Sites_SiteId",
                schema: "config",
                table: "DataValidationRules",
                column: "SiteId",
                principalSchema: "core",
                principalTable: "Sites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InferenceRequests_PredictionResults_PredictionResultId",
                schema: "ml",
                table: "InferenceRequests",
                column: "PredictionResultId",
                principalSchema: "ml",
                principalTable: "PredictionResults",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_InferenceRequests_Sites_SiteId",
                schema: "ml",
                table: "InferenceRequests",
                column: "SiteId",
                principalSchema: "core",
                principalTable: "Sites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ModelVersions_Sites_SiteId",
                schema: "ml",
                table: "ModelVersions",
                column: "SiteId",
                principalSchema: "core",
                principalTable: "Sites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PredictionResults_Sites_SiteId",
                schema: "ml",
                table: "PredictionResults",
                column: "SiteId",
                principalSchema: "core",
                principalTable: "Sites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SiteDataSchemas_Sites_SiteId",
                schema: "config",
                table: "SiteDataSchemas",
                column: "SiteId",
                principalSchema: "core",
                principalTable: "Sites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SiteDataSchemas_Sites_SiteId",
                schema: "config",
                table: "SiteDataSchemas");

            migrationBuilder.DropTable(
                name: "Dashboards",
                schema: "config");

            migrationBuilder.DropTable(
                name: "DataCleaningRules",
                schema: "config");

            migrationBuilder.DropTable(
                name: "DataRows",
                schema: "data");

            migrationBuilder.DropTable(
                name: "DataValidationRules",
                schema: "config");

            migrationBuilder.DropTable(
                name: "InferenceRequests",
                schema: "ml");

            migrationBuilder.DropTable(
                name: "TrainingJobs",
                schema: "ml");

            migrationBuilder.DropTable(
                name: "Uploads",
                schema: "data");

            migrationBuilder.DropTable(
                name: "UserSiteAccess",
                schema: "core");

            migrationBuilder.DropTable(
                name: "PredictionResults",
                schema: "ml");

            migrationBuilder.DropTable(
                name: "ModelVersions",
                schema: "ml");

            migrationBuilder.DropTable(
                name: "Sites",
                schema: "core");

            migrationBuilder.DropTable(
                name: "SiteDataSchemas",
                schema: "config");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "core");

            migrationBuilder.DropTable(
                name: "Companies",
                schema: "core");
        }
    }
}
