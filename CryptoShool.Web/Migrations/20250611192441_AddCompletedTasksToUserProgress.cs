using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CryptoShool.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddCompletedTasksToUserProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CompletedTasks",
                table: "UserProgress",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletedTasks",
                table: "UserProgress");
        }
    }
}
