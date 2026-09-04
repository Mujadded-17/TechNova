using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechNova.Migrations
{
    /// <inheritdoc />
    public partial class AddNfcCardRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NfcCardRequests",
                columns: table => new
                {
                    NfcCardRequestID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    UserType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StripeSessionID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StripePaymentIntentID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequestStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CardToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CardUID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaidAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IssuedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NfcCardRequests", x => x.NfcCardRequestID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NfcCardRequests");
        }
    }
}
