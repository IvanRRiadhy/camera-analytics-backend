using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CCTV.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSpatialRoomModuleCleanHierarchy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cameras_Floors_FloorId",
                table: "Cameras");

            migrationBuilder.DropForeignKey(
                name: "FK_Cameras_Sites_SiteId",
                table: "Cameras");

            migrationBuilder.DropIndex(
                name: "IX_PipelineAssignments_SessionId",
                table: "PipelineAssignments");

            migrationBuilder.DropIndex(
                name: "IX_Cameras_ApplicationId_SiteId",
                table: "Cameras");

            migrationBuilder.DropIndex(
                name: "IX_Cameras_SiteId",
                table: "Cameras");

            migrationBuilder.DropColumn(
                name: "PipelineParametersSnapshotJson",
                table: "PipelineAssignments");

            migrationBuilder.DropColumn(
                name: "SiteId",
                table: "Cameras");

            migrationBuilder.DropColumn(
                name: "DefaultParametersJson",
                table: "AnalyticsModules");

            migrationBuilder.RenameColumn(
                name: "FloorId",
                table: "Cameras",
                newName: "RoomId");

            migrationBuilder.RenameIndex(
                name: "IX_Cameras_FloorId",
                table: "Cameras",
                newName: "IX_Cameras_RoomId");

            migrationBuilder.RenameColumn(
                name: "RequiredMinCudaCapability",
                table: "AnalyticsModules",
                newName: "MinimumCudaCapability");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "AnalyticsModules",
                newName: "DisplayName");

            migrationBuilder.RenameColumn(
                name: "IsGlobalActive",
                table: "AnalyticsModules",
                newName: "SupportsRealtime");

            migrationBuilder.AlterColumn<Guid>(
                name: "SessionId",
                table: "PipelineAssignments",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<string>(
                name: "AnalyticsModules",
                table: "PipelineAssignments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<DateTime>(
                name: "AssignedAtUtc",
                table: "PipelineAssignments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CameraId",
                table: "PipelineAssignments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "PipelineId",
                table: "PipelineAssignments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PipelineName",
                table: "PipelineAssignments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "PipelineAssignments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RequiredCapabilities",
                table: "PipelineAssignments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<DateTime>(
                name: "StartedAtUtc",
                table: "PipelineAssignments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "PipelineAssignments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "StoppedAtUtc",
                table: "PipelineAssignments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EngineVersion",
                table: "EngineInstances",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GpuModel",
                table: "EngineInstances",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Hostname",
                table: "EngineInstances",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "InstalledPluginsJson",
                table: "EngineInstances",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "InstanceId",
                table: "EngineInstances",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "EngineInstances",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastHeartbeatUtc",
                table: "EngineInstances",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "OsPlatform",
                table: "EngineInstances",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "RegisteredAtUtc",
                table: "EngineInstances",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "State",
                table: "EngineInstances",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalRamMb",
                table: "EngineInstances",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalVramMb",
                table: "EngineInstances",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ActivePipelinesCount",
                table: "EngineHeartbeats",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<float>(
                name: "CpuUsagePercent",
                table: "EngineHeartbeats",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "GpuTemperatureC",
                table: "EngineHeartbeats",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "GpuUsagePercent",
                table: "EngineHeartbeats",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<string>(
                name: "InstanceIdString",
                table: "EngineHeartbeats",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "SequenceNumber",
                table: "EngineHeartbeats",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "TelemetryJson",
                table: "EngineHeartbeats",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "TimestampUtc",
                table: "EngineHeartbeats",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "VramUsedMb",
                table: "EngineHeartbeats",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "AnalyticsModules",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "AnalyticsModules",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DefaultConfigurationJson",
                table: "AnalyticsModules",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DocumentationUrl",
                table: "AnalyticsModules",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Icon",
                table: "AnalyticsModules",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PythonEntryPoint",
                table: "AnalyticsModules",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "AnalyticsModules",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "SupportsBatch",
                table: "AnalyticsModules",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "AnalyticsRuntimeLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    TimestampUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EngineInstanceId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    CameraId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    PipelineId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    PluginName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    MetricName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    MetricValue = table.Column<double>(type: "float", nullable: false),
                    JsonSnapshot = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InsertedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    ApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalyticsRuntimeLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EngineCapabilities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EngineInstanceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CapabilityCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Version = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SdkVersion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    ApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EngineCapabilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EngineCapabilities_EngineInstances_EngineInstanceId",
                        column: x => x.EngineInstanceId,
                        principalTable: "EngineInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EngineHealthHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EngineInstanceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CpuUsagePercent = table.Column<float>(type: "real", nullable: false),
                    GpuUsagePercent = table.Column<float>(type: "real", nullable: false),
                    VramUsedMb = table.Column<int>(type: "int", nullable: false),
                    TotalVramMb = table.Column<int>(type: "int", nullable: false),
                    GpuTemperatureC = table.Column<float>(type: "real", nullable: false),
                    InferenceFps = table.Column<float>(type: "real", nullable: false),
                    FrameDropCount = table.Column<int>(type: "int", nullable: false),
                    ActivePipelinesCount = table.Column<int>(type: "int", nullable: false),
                    HealthScore = table.Column<int>(type: "int", nullable: false),
                    HealthState = table.Column<int>(type: "int", nullable: false),
                    TimestampUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    ApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EngineHealthHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EngineHealthHistories_EngineInstances_EngineInstanceId",
                        column: x => x.EngineInstanceId,
                        principalTable: "EngineInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EngineRuntimes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    EngineInstanceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkerNodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Hostname = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false),
                    EngineVersion = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CudaVersion = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    GpuModel = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    GpuDriver = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CpuModel = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    RamTotalMb = table.Column<long>(type: "bigint", nullable: false),
                    RamUsedMb = table.Column<long>(type: "bigint", nullable: false),
                    VramTotalMb = table.Column<long>(type: "bigint", nullable: false),
                    VramUsedMb = table.Column<long>(type: "bigint", nullable: false),
                    CpuUtilization = table.Column<float>(type: "real", nullable: false),
                    GpuUtilization = table.Column<float>(type: "real", nullable: false),
                    DiskUsage = table.Column<float>(type: "real", nullable: false),
                    TemperatureC = table.Column<float>(type: "real", nullable: false),
                    InferenceFps = table.Column<float>(type: "real", nullable: false),
                    AverageLatencyMs = table.Column<float>(type: "real", nullable: false),
                    QueueLength = table.Column<int>(type: "int", nullable: false),
                    DroppedFrames = table.Column<long>(type: "bigint", nullable: false),
                    ProcessedFrames = table.Column<long>(type: "bigint", nullable: false),
                    PublishedEvents = table.Column<long>(type: "bigint", nullable: false),
                    ReconnectCount = table.Column<int>(type: "int", nullable: false),
                    LastHeartbeatUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    ApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EngineRuntimes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PipelineHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PipelineAssignmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PreviousStatus = table.Column<int>(type: "int", nullable: false),
                    NewStatus = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TimestampUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PipelineHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PipelineHistories_PipelineAssignments_PipelineAssignmentId",
                        column: x => x.PipelineAssignmentId,
                        principalTable: "PipelineAssignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PipelineModuleAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PipelineAssignmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    ParametersJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AssignedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    ApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PipelineModuleAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PipelineModuleAssignments_AnalyticsModules_ModuleId",
                        column: x => x.ModuleId,
                        principalTable: "AnalyticsModules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PipelineModuleAssignments_PipelineAssignments_PipelineAssignmentId",
                        column: x => x.PipelineAssignmentId,
                        principalTable: "PipelineAssignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PluginRuntimes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    PipelineAssignmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AnalyticsModuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PluginName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PluginVersion = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CurrentFps = table.Column<float>(type: "real", nullable: false),
                    AverageInferenceTimeMs = table.Column<float>(type: "real", nullable: false),
                    FramesProcessed = table.Column<long>(type: "bigint", nullable: false),
                    EventsGenerated = table.Column<long>(type: "bigint", nullable: false),
                    Errors = table.Column<int>(type: "int", nullable: false),
                    Warnings = table.Column<int>(type: "int", nullable: false),
                    DroppedFrames = table.Column<long>(type: "bigint", nullable: false),
                    MemoryUsageMb = table.Column<float>(type: "real", nullable: false),
                    GpuUsagePercent = table.Column<float>(type: "real", nullable: false),
                    CpuUsagePercent = table.Column<float>(type: "real", nullable: false),
                    LastEventTimestampUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    ApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PluginRuntimes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Rooms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    FloorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    ApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rooms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rooms_Floors_FloorId",
                        column: x => x.FloorId,
                        principalTable: "Floors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PipelineAssignments_CameraId",
                table: "PipelineAssignments",
                column: "CameraId");

            migrationBuilder.CreateIndex(
                name: "IX_PipelineAssignments_SessionId",
                table: "PipelineAssignments",
                column: "SessionId",
                unique: true,
                filter: "[SessionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Cameras_ApplicationId_RoomId",
                table: "Cameras",
                columns: new[] { "ApplicationId", "RoomId" });

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsRuntimeLogs_ApplicationId_CameraId_TimestampUtc",
                table: "AnalyticsRuntimeLogs",
                columns: new[] { "ApplicationId", "CameraId", "TimestampUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsRuntimeLogs_ApplicationId_PipelineId_TimestampUtc",
                table: "AnalyticsRuntimeLogs",
                columns: new[] { "ApplicationId", "PipelineId", "TimestampUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_EngineCapabilities_EngineInstanceId",
                table: "EngineCapabilities",
                column: "EngineInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_EngineHealthHistories_EngineInstanceId",
                table: "EngineHealthHistories",
                column: "EngineInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_EngineRuntimes_ApplicationId_EngineInstanceId",
                table: "EngineRuntimes",
                columns: new[] { "ApplicationId", "EngineInstanceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PipelineHistories_PipelineAssignmentId",
                table: "PipelineHistories",
                column: "PipelineAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_PipelineModuleAssignments_ModuleId",
                table: "PipelineModuleAssignments",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_PipelineModuleAssignments_PipelineAssignmentId",
                table: "PipelineModuleAssignments",
                column: "PipelineAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_PluginRuntimes_ApplicationId_PipelineAssignmentId_PluginName",
                table: "PluginRuntimes",
                columns: new[] { "ApplicationId", "PipelineAssignmentId", "PluginName" });

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_ApplicationId_FloorId_Code",
                table: "Rooms",
                columns: new[] { "ApplicationId", "FloorId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_ApplicationId_FloorId_SortOrder",
                table: "Rooms",
                columns: new[] { "ApplicationId", "FloorId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_FloorId",
                table: "Rooms",
                column: "FloorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cameras_Rooms_RoomId",
                table: "Cameras",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PipelineAssignments_Cameras_CameraId",
                table: "PipelineAssignments",
                column: "CameraId",
                principalTable: "Cameras",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cameras_Rooms_RoomId",
                table: "Cameras");

            migrationBuilder.DropForeignKey(
                name: "FK_PipelineAssignments_Cameras_CameraId",
                table: "PipelineAssignments");

            migrationBuilder.DropTable(
                name: "AnalyticsRuntimeLogs");

            migrationBuilder.DropTable(
                name: "EngineCapabilities");

            migrationBuilder.DropTable(
                name: "EngineHealthHistories");

            migrationBuilder.DropTable(
                name: "EngineRuntimes");

            migrationBuilder.DropTable(
                name: "PipelineHistories");

            migrationBuilder.DropTable(
                name: "PipelineModuleAssignments");

            migrationBuilder.DropTable(
                name: "PluginRuntimes");

            migrationBuilder.DropTable(
                name: "Rooms");

            migrationBuilder.DropIndex(
                name: "IX_PipelineAssignments_CameraId",
                table: "PipelineAssignments");

            migrationBuilder.DropIndex(
                name: "IX_PipelineAssignments_SessionId",
                table: "PipelineAssignments");

            migrationBuilder.DropIndex(
                name: "IX_Cameras_ApplicationId_RoomId",
                table: "Cameras");

            migrationBuilder.DropColumn(
                name: "AnalyticsModules",
                table: "PipelineAssignments");

            migrationBuilder.DropColumn(
                name: "AssignedAtUtc",
                table: "PipelineAssignments");

            migrationBuilder.DropColumn(
                name: "CameraId",
                table: "PipelineAssignments");

            migrationBuilder.DropColumn(
                name: "PipelineId",
                table: "PipelineAssignments");

            migrationBuilder.DropColumn(
                name: "PipelineName",
                table: "PipelineAssignments");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "PipelineAssignments");

            migrationBuilder.DropColumn(
                name: "RequiredCapabilities",
                table: "PipelineAssignments");

            migrationBuilder.DropColumn(
                name: "StartedAtUtc",
                table: "PipelineAssignments");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "PipelineAssignments");

            migrationBuilder.DropColumn(
                name: "StoppedAtUtc",
                table: "PipelineAssignments");

            migrationBuilder.DropColumn(
                name: "EngineVersion",
                table: "EngineInstances");

            migrationBuilder.DropColumn(
                name: "GpuModel",
                table: "EngineInstances");

            migrationBuilder.DropColumn(
                name: "Hostname",
                table: "EngineInstances");

            migrationBuilder.DropColumn(
                name: "InstalledPluginsJson",
                table: "EngineInstances");

            migrationBuilder.DropColumn(
                name: "InstanceId",
                table: "EngineInstances");

            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "EngineInstances");

            migrationBuilder.DropColumn(
                name: "LastHeartbeatUtc",
                table: "EngineInstances");

            migrationBuilder.DropColumn(
                name: "OsPlatform",
                table: "EngineInstances");

            migrationBuilder.DropColumn(
                name: "RegisteredAtUtc",
                table: "EngineInstances");

            migrationBuilder.DropColumn(
                name: "State",
                table: "EngineInstances");

            migrationBuilder.DropColumn(
                name: "TotalRamMb",
                table: "EngineInstances");

            migrationBuilder.DropColumn(
                name: "TotalVramMb",
                table: "EngineInstances");

            migrationBuilder.DropColumn(
                name: "ActivePipelinesCount",
                table: "EngineHeartbeats");

            migrationBuilder.DropColumn(
                name: "CpuUsagePercent",
                table: "EngineHeartbeats");

            migrationBuilder.DropColumn(
                name: "GpuTemperatureC",
                table: "EngineHeartbeats");

            migrationBuilder.DropColumn(
                name: "GpuUsagePercent",
                table: "EngineHeartbeats");

            migrationBuilder.DropColumn(
                name: "InstanceIdString",
                table: "EngineHeartbeats");

            migrationBuilder.DropColumn(
                name: "SequenceNumber",
                table: "EngineHeartbeats");

            migrationBuilder.DropColumn(
                name: "TelemetryJson",
                table: "EngineHeartbeats");

            migrationBuilder.DropColumn(
                name: "TimestampUtc",
                table: "EngineHeartbeats");

            migrationBuilder.DropColumn(
                name: "VramUsedMb",
                table: "EngineHeartbeats");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "AnalyticsModules");

            migrationBuilder.DropColumn(
                name: "DefaultConfigurationJson",
                table: "AnalyticsModules");

            migrationBuilder.DropColumn(
                name: "DocumentationUrl",
                table: "AnalyticsModules");

            migrationBuilder.DropColumn(
                name: "Icon",
                table: "AnalyticsModules");

            migrationBuilder.DropColumn(
                name: "PythonEntryPoint",
                table: "AnalyticsModules");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "AnalyticsModules");

            migrationBuilder.DropColumn(
                name: "SupportsBatch",
                table: "AnalyticsModules");

            migrationBuilder.RenameColumn(
                name: "RoomId",
                table: "Cameras",
                newName: "FloorId");

            migrationBuilder.RenameIndex(
                name: "IX_Cameras_RoomId",
                table: "Cameras",
                newName: "IX_Cameras_FloorId");

            migrationBuilder.RenameColumn(
                name: "SupportsRealtime",
                table: "AnalyticsModules",
                newName: "IsGlobalActive");

            migrationBuilder.RenameColumn(
                name: "MinimumCudaCapability",
                table: "AnalyticsModules",
                newName: "RequiredMinCudaCapability");

            migrationBuilder.RenameColumn(
                name: "DisplayName",
                table: "AnalyticsModules",
                newName: "Name");

            migrationBuilder.AlterColumn<Guid>(
                name: "SessionId",
                table: "PipelineAssignments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineParametersSnapshotJson",
                table: "PipelineAssignments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SiteId",
                table: "Cameras",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "AnalyticsModules",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AddColumn<string>(
                name: "DefaultParametersJson",
                table: "AnalyticsModules",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PipelineAssignments_SessionId",
                table: "PipelineAssignments",
                column: "SessionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cameras_ApplicationId_SiteId",
                table: "Cameras",
                columns: new[] { "ApplicationId", "SiteId" });

            migrationBuilder.CreateIndex(
                name: "IX_Cameras_SiteId",
                table: "Cameras",
                column: "SiteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cameras_Floors_FloorId",
                table: "Cameras",
                column: "FloorId",
                principalTable: "Floors",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Cameras_Sites_SiteId",
                table: "Cameras",
                column: "SiteId",
                principalTable: "Sites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
