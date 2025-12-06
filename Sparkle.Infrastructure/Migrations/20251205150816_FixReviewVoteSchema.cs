using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sparkle.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixReviewVoteSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReviewVotes_ProductReviews_ProductReviewId1",
                schema: "reviews",
                table: "ReviewVotes");

            migrationBuilder.DropIndex(
                name: "IX_ReviewVotes_ProductReviewId1",
                schema: "reviews",
                table: "ReviewVotes");

            migrationBuilder.DropColumn(
                name: "ProductReviewId1",
                schema: "reviews",
                table: "ReviewVotes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProductReviewId1",
                schema: "reviews",
                table: "ReviewVotes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReviewVotes_ProductReviewId1",
                schema: "reviews",
                table: "ReviewVotes",
                column: "ProductReviewId1");

            migrationBuilder.AddForeignKey(
                name: "FK_ReviewVotes_ProductReviews_ProductReviewId1",
                schema: "reviews",
                table: "ReviewVotes",
                column: "ProductReviewId1",
                principalSchema: "reviews",
                principalTable: "ProductReviews",
                principalColumn: "Id");
        }
    }
}
