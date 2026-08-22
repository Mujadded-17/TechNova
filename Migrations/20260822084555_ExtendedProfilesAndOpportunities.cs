using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechNova.Migrations
{
    /// <inheritdoc />
    public partial class ExtendedProfilesAndOpportunities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AmountRaised",
                table: "Startups",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "BusinessModel",
                table: "Startups",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompetitiveAdvantage",
                table: "Startups",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactPerson",
                table: "Startups",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactPhone",
                table: "Startups",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CoverImagePath",
                table: "Startups",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Startups",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "EquityOffered",
                table: "Startups",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FoundedYear",
                table: "Startups",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FundingDeadline",
                table: "Startups",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Industry",
                table: "Startups",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPublished",
                table: "Startups",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Startups",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MinimumInvestment",
                table: "Startups",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfEmployees",
                table: "Startups",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProblemStatement",
                table: "Startups",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Solution",
                table: "Startups",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tagline",
                table: "Startups",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TargetMarket",
                table: "Startups",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Traction",
                table: "Startups",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Startups",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Bio",
                table: "Investors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Investors",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "InvestedIndustries",
                table: "Investors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InvestorType",
                table: "Investors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Investors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxInvestmentAmount",
                table: "Investors",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MinInvestmentAmount",
                table: "Investors",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfileImagePath",
                table: "Investors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ReceiveNotifications",
                table: "Investors",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Investors",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Website",
                table: "Investors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StartupInvestmentOpportunityOpportunityID",
                table: "InvestmentRequests",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FavoriteStartups",
                columns: table => new
                {
                    FavoriteID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvestorID = table.Column<int>(type: "int", nullable: false),
                    StartupID = table.Column<int>(type: "int", nullable: false),
                    SavedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FavoriteStartups", x => x.FavoriteID);
                    table.ForeignKey(
                        name: "FK_FavoriteStartups_Investors_InvestorID",
                        column: x => x.InvestorID,
                        principalTable: "Investors",
                        principalColumn: "InvestorID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FavoriteStartups_Startups_StartupID",
                        column: x => x.StartupID,
                        principalTable: "Startups",
                        principalColumn: "StartupID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StartupInvestmentOpportunities",
                columns: table => new
                {
                    OpportunityID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartupID = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PitchSummary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FundingGoal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CurrentFunding = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FundingStage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EquityPercentage = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MinimumInvestment = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Industry = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BusinessStage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FoundedYear = table.Column<int>(type: "int", nullable: true),
                    TeamSize = table.Column<int>(type: "int", nullable: true),
                    Website = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InvestmentDeadline = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StartupInvestmentOpportunities", x => x.OpportunityID);
                    table.ForeignKey(
                        name: "FK_StartupInvestmentOpportunities_Startups_StartupID",
                        column: x => x.StartupID,
                        principalTable: "Startups",
                        principalColumn: "StartupID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentRequests_StartupInvestmentOpportunityOpportunityID",
                table: "InvestmentRequests",
                column: "StartupInvestmentOpportunityOpportunityID");

            migrationBuilder.CreateIndex(
                name: "IX_FavoriteStartups_InvestorID_StartupID",
                table: "FavoriteStartups",
                columns: new[] { "InvestorID", "StartupID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FavoriteStartups_StartupID",
                table: "FavoriteStartups",
                column: "StartupID");

            migrationBuilder.CreateIndex(
                name: "IX_StartupInvestmentOpportunities_StartupID",
                table: "StartupInvestmentOpportunities",
                column: "StartupID");

            migrationBuilder.AddForeignKey(
                name: "FK_InvestmentRequests_StartupInvestmentOpportunities_StartupInvestmentOpportunityOpportunityID",
                table: "InvestmentRequests",
                column: "StartupInvestmentOpportunityOpportunityID",
                principalTable: "StartupInvestmentOpportunities",
                principalColumn: "OpportunityID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvestmentRequests_StartupInvestmentOpportunities_StartupInvestmentOpportunityOpportunityID",
                table: "InvestmentRequests");

            migrationBuilder.DropTable(
                name: "FavoriteStartups");

            migrationBuilder.DropTable(
                name: "StartupInvestmentOpportunities");

            migrationBuilder.DropIndex(
                name: "IX_InvestmentRequests_StartupInvestmentOpportunityOpportunityID",
                table: "InvestmentRequests");

            migrationBuilder.DropColumn(
                name: "AmountRaised",
                table: "Startups");

            migrationBuilder.DropColumn(
                name: "BusinessModel",
                table: "Startups");

            migrationBuilder.DropColumn(
                name: "CompetitiveAdvantage",
                table: "Startups");

            migrationBuilder.DropColumn(
                name: "ContactPerson",
                table: "Startups");

            migrationBuilder.DropColumn(
                name: "ContactPhone",
                table: "Startups");

            migrationBuilder.DropColumn(
                name: "CoverImagePath",
                table: "Startups");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Startups");

            migrationBuilder.DropColumn(
                name: "EquityOffered",
                table: "Startups");

            migrationBuilder.DropColumn(
                name: "FoundedYear",
                table: "Startups");

            migrationBuilder.DropColumn(
                name: "FundingDeadline",
                table: "Startups");

            migrationBuilder.DropColumn(
                name: "Industry",
                table: "Startups");

            migrationBuilder.DropColumn(
                name: "IsPublished",
                table: "Startups");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Startups");

            migrationBuilder.DropColumn(
                name: "MinimumInvestment",
                table: "Startups");

            migrationBuilder.DropColumn(
                name: "NumberOfEmployees",
                table: "Startups");

            migrationBuilder.DropColumn(
                name: "ProblemStatement",
                table: "Startups");

            migrationBuilder.DropColumn(
                name: "Solution",
                table: "Startups");

            migrationBuilder.DropColumn(
                name: "Tagline",
                table: "Startups");

            migrationBuilder.DropColumn(
                name: "TargetMarket",
                table: "Startups");

            migrationBuilder.DropColumn(
                name: "Traction",
                table: "Startups");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Startups");

            migrationBuilder.DropColumn(
                name: "Bio",
                table: "Investors");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Investors");

            migrationBuilder.DropColumn(
                name: "InvestedIndustries",
                table: "Investors");

            migrationBuilder.DropColumn(
                name: "InvestorType",
                table: "Investors");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Investors");

            migrationBuilder.DropColumn(
                name: "MaxInvestmentAmount",
                table: "Investors");

            migrationBuilder.DropColumn(
                name: "MinInvestmentAmount",
                table: "Investors");

            migrationBuilder.DropColumn(
                name: "ProfileImagePath",
                table: "Investors");

            migrationBuilder.DropColumn(
                name: "ReceiveNotifications",
                table: "Investors");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Investors");

            migrationBuilder.DropColumn(
                name: "Website",
                table: "Investors");

            migrationBuilder.DropColumn(
                name: "StartupInvestmentOpportunityOpportunityID",
                table: "InvestmentRequests");
        }
    }
}
