using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Qonote.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddNoteTitleNormalizedAndUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "108ea72d-516a-4ed9-b40f-b5b804a60262");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "146020ed-a5a3-4f36-ab8e-3977c929e3ac");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a19cd228-f7e4-4cb5-bd85-8c6974ed72fd");

            migrationBuilder.AddColumn<string>(
                name: "CustomTitleNormalized",
                table: "Notes",
                type: "text",
                nullable: true,
                computedColumnSql: "lower(btrim(\"CustomTitle\"))",
                stored: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notes_UserId_CustomTitleNormalized",
                table: "Notes",
                columns: new[] { "UserId", "CustomTitleNormalized" },
                unique: true,
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Notes_UserId_CustomTitleNormalized",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "CustomTitleNormalized",
                table: "Notes");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "CreatedAt", "DeletedAt", "IsDeleted", "Name", "NormalizedName", "UpdatedAt" },
                values: new object[,]
                {
                    { "108ea72d-516a-4ed9-b40f-b5b804a60262", null, new DateTime(2025, 9, 28, 18, 23, 30, 245, DateTimeKind.Utc).AddTicks(2401), null, false, "Admin", "ADMIN", null },
                    { "146020ed-a5a3-4f36-ab8e-3977c929e3ac", null, new DateTime(2025, 9, 28, 18, 23, 30, 245, DateTimeKind.Utc).AddTicks(2459), null, false, "FreeUser", "FREEUSER", null },
                    { "a19cd228-f7e4-4cb5-bd85-8c6974ed72fd", null, new DateTime(2025, 9, 28, 18, 23, 30, 245, DateTimeKind.Utc).AddTicks(2463), null, false, "PremiumUser", "PREMIUMUSER", null }
                });
        }
    }
}
