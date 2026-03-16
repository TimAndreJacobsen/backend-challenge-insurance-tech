using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Claims.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditColumnConstraintsAndIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "HttpRequestType",
                table: "CoverAudits",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "CoverId",
                table: "CoverAudits",
                type: "nvarchar(36)",
                maxLength: 36,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "HttpRequestType",
                table: "ClaimAudits",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ClaimId",
                table: "ClaimAudits",
                type: "nvarchar(36)",
                maxLength: 36,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_CoverAudits_CoverId",
                table: "CoverAudits",
                column: "CoverId");

            migrationBuilder.CreateIndex(
                name: "IX_ClaimAudits_ClaimId",
                table: "ClaimAudits",
                column: "ClaimId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CoverAudits_CoverId",
                table: "CoverAudits");

            migrationBuilder.DropIndex(
                name: "IX_ClaimAudits_ClaimId",
                table: "ClaimAudits");

            migrationBuilder.AlterColumn<string>(
                name: "HttpRequestType",
                table: "CoverAudits",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "CoverId",
                table: "CoverAudits",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(36)",
                oldMaxLength: 36);

            migrationBuilder.AlterColumn<string>(
                name: "HttpRequestType",
                table: "ClaimAudits",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "ClaimId",
                table: "ClaimAudits",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(36)",
                oldMaxLength: 36);
        }
    }
}
