using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CCTV.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUsernameToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EnhancedMediaUrl",
                table: "DetectionEvidences",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OriginalMediaUrl",
                table: "DetectionEvidences",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_ApplicationId_Username",
                table: "Users",
                columns: new[] { "ApplicationId", "Username" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_ApplicationId_Username",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Username",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EnhancedMediaUrl",
                table: "DetectionEvidences");

            migrationBuilder.DropColumn(
                name: "OriginalMediaUrl",
                table: "DetectionEvidences");
        }
    }
}
