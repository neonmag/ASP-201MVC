using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASP_201MVC.Migrations
{
    /// <inheritdoc />
    public partial class AddIsPublicFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDateTimePublic",
                table: "Users",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsEmailPublic",
                table: "Users",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "isRealNamePublic",
                table: "Users",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDateTimePublic",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsEmailPublic",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "isRealNamePublic",
                table: "Users");
        }
    }
}
