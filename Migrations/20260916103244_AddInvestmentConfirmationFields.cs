using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechNova.Migrations
{
    /// <inheritdoc />
    public partial class AddInvestmentConfirmationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AcceptedDate",
                table: "InvestmentRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "InvestorPdfDownloaded",
                table: "InvestmentRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "StartupPdfDownloaded",
                table: "InvestmentRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AcceptedDate",
                table: "InvestmentRequests");

            migrationBuilder.DropColumn(
                name: "InvestorPdfDownloaded",
                table: "InvestmentRequests");

            migrationBuilder.DropColumn(
                name: "StartupPdfDownloaded",
                table: "InvestmentRequests");
        }
    }
}
