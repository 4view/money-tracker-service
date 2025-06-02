using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoneyTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class DescriptionProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "ExpenseEntity",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "ExpenseEntity");
        }
    }
}
