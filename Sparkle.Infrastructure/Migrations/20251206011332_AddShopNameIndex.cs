using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sparkle.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddShopNameIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ShopName",
                schema: "sellers",
                table: "Sellers",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Sellers_ShopName",
                schema: "sellers",
                table: "Sellers",
                column: "ShopName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sellers_ShopName",
                schema: "sellers",
                table: "Sellers");

            migrationBuilder.AlterColumn<string>(
                name: "ShopName",
                schema: "sellers",
                table: "Sellers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
