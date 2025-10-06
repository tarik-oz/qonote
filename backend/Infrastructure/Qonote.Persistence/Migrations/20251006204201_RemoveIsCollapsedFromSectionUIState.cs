using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Qonote.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIsCollapsedFromSectionUIState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCollapsed",
                table: "SectionUIStates");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCollapsed",
                table: "SectionUIStates",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
