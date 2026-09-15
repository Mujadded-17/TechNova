using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechNova.Migrations
{
    /// <inheritdoc />
    public partial class AddProfileVerificationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProfileVerificationStatus",
                table: "Startups",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "NotRequested");

            migrationBuilder.AddColumn<int>(
                name: "ProfileVerifiedByAdminID",
                table: "Startups",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProfileVerifiedAt",
                table: "Startups",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Startups_ProfileVerifiedByAdminID",
                table: "Startups",
                column: "ProfileVerifiedByAdminID");

            migrationBuilder.AddForeignKey(
                name: "FK_Startups_Admins_ProfileVerifiedByAdminID",
                table: "Startups",
                column: "ProfileVerifiedByAdminID",
                principalTable: "Admins",
                principalColumn: "AdminID",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Startups_Admins_ProfileVerifiedByAdminID",
                table: "Startups");

            migrationBuilder.DropIndex(
                name: "IX_Startups_ProfileVerifiedByAdminID",
                table: "Startups");

            migrationBuilder.DropColumn(
                name: "ProfileVerificationStatus",
                table: "Startups");

            migrationBuilder.DropColumn(
                name: "ProfileVerifiedByAdminID",
                table: "Startups");

            migrationBuilder.DropColumn(
                name: "ProfileVerifiedAt",
                table: "Startups");
        }
    }
}