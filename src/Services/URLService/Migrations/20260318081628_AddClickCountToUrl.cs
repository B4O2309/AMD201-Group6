using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URLService.Migrations
{
    /// <inheritdoc />
    public partial class AddClickCountToUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClickCount",
                table: "Urls",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClickCount",
                table: "Urls");
        }
    }
}
