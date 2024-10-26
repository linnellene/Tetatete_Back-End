using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TetaBackend.Migrations
{
    /// <inheritdoc />
    public partial class IsDisliked : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDisliked",
                table: "Matches",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDisliked",
                table: "Matches");
        }
    }
}
