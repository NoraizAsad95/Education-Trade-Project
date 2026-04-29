using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EducationTrade.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsOAuthUser",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "OAuthProvider",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OAuthProviderId",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsOAuthUser",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "OAuthProvider",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "OAuthProviderId",
                table: "Users");
        }
    }
}
