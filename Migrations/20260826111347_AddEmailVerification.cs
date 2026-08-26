using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechNova.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailVerification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EmailVerificationSentAt",
                table: "Startups",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmailVerificationToken",
                table: "Startups",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EmailVerified",
                table: "Startups",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "EmailVerificationSentAt",
                table: "Investors",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmailVerificationToken",
                table: "Investors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EmailVerified",
                table: "Investors",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // Accounts that already existed predate verification. Marking
            // them unverified would lock every current user out of a system
            // they were legitimately using, so they are grandfathered in.
            // Only accounts created from now on must confirm.
            migrationBuilder.Sql("UPDATE [Startups]  SET [EmailVerified] = 1;");
            migrationBuilder.Sql("UPDATE [Investors] SET [EmailVerified] = 1;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailVerificationSentAt",
                table: "Startups");

            migrationBuilder.DropColumn(
                name: "EmailVerificationToken",
                table: "Startups");

            migrationBuilder.DropColumn(
                name: "EmailVerified",
                table: "Startups");

            migrationBuilder.DropColumn(
                name: "EmailVerificationSentAt",
                table: "Investors");

            migrationBuilder.DropColumn(
                name: "EmailVerificationToken",
                table: "Investors");

            migrationBuilder.DropColumn(
                name: "EmailVerified",
                table: "Investors");
        }
    }
}
