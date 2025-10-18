using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Qonote.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserSubscriptionUserIdStatusStartDateIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_UserSubscriptions_UserId_Status_StartDate",
                table: "UserSubscriptions",
                columns: new[] { "UserId", "Status", "StartDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserSubscriptions_UserId_Status_StartDate",
                table: "UserSubscriptions");
        }
    }
}
