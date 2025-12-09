using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HabitTribe.Migrations
{
    /// <inheritdoc />
    public partial class tagusability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Usable",
                table: "Tags",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Usable",
                table: "Tags");
        }
    }
}
