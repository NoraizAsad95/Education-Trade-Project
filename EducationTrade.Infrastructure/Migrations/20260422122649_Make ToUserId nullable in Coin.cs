using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EducationTrade.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeToUserIdnullableinCoin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CoinTransactions_Users_ToUserId",
                table: "CoinTransactions");

            migrationBuilder.AlterColumn<int>(
                name: "ToUserId",
                table: "CoinTransactions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_CoinTransactions_Users_ToUserId",
                table: "CoinTransactions",
                column: "ToUserId",
                principalTable: "Users",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CoinTransactions_Users_ToUserId",
                table: "CoinTransactions");

            migrationBuilder.AlterColumn<int>(
                name: "ToUserId",
                table: "CoinTransactions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CoinTransactions_Users_ToUserId",
                table: "CoinTransactions",
                column: "ToUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
