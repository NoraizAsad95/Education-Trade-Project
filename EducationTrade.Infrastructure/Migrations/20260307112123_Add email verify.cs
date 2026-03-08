using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EducationTrade.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Addemailverify : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "CoinBalance",
                table: "Users",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 200);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "CoinBalance",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 200,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
