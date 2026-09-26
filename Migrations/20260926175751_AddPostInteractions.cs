using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechNova.Migrations
{
    /// <inheritdoc />
    public partial class AddPostInteractions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PostComments",
                columns: table => new
                {
                    PostCommentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PostID = table.Column<int>(type: "int", nullable: false),
                    InvestorID = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostComments", x => x.PostCommentID);
                    table.ForeignKey(
                        name: "FK_PostComments_Investors_InvestorID",
                        column: x => x.InvestorID,
                        principalTable: "Investors",
                        principalColumn: "InvestorID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PostComments_Posts_PostID",
                        column: x => x.PostID,
                        principalTable: "Posts",
                        principalColumn: "PostID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PostLikes",
                columns: table => new
                {
                    PostLikeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PostID = table.Column<int>(type: "int", nullable: false),
                    InvestorID = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostLikes", x => x.PostLikeID);
                    table.ForeignKey(
                        name: "FK_PostLikes_Investors_InvestorID",
                        column: x => x.InvestorID,
                        principalTable: "Investors",
                        principalColumn: "InvestorID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PostLikes_Posts_PostID",
                        column: x => x.PostID,
                        principalTable: "Posts",
                        principalColumn: "PostID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PostComments_InvestorID",
                table: "PostComments",
                column: "InvestorID");

            migrationBuilder.CreateIndex(
                name: "IX_PostComments_PostID",
                table: "PostComments",
                column: "PostID");

            migrationBuilder.CreateIndex(
                name: "IX_PostLikes_InvestorID",
                table: "PostLikes",
                column: "InvestorID");

            migrationBuilder.CreateIndex(
                name: "IX_PostLikes_PostID",
                table: "PostLikes",
                column: "PostID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PostComments");

            migrationBuilder.DropTable(
                name: "PostLikes");
        }
    }
}
