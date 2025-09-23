using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Qonote.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MakeEnumTypesStringForSectionsAndBlocks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "190ba005-6849-4646-8a15-7421a686946b");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "306c92ad-623b-4abc-996a-5ef2c30d3440");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "93688a9a-3294-485d-9ef3-0e468e7f8bf0");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Sections",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Blocks",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "CreatedAt", "IsDeleted", "Name", "NormalizedName", "UpdatedAt" },
                values: new object[,]
                {
                    { "8507eed1-38de-4de7-b606-bbbf41d1dafd", null, new DateTime(2025, 9, 22, 12, 32, 17, 345, DateTimeKind.Utc).AddTicks(3548), false, "FreeUser", "FREEUSER", null },
                    { "bfcb02e5-c78a-407f-baf2-301f78f4b319", null, new DateTime(2025, 9, 22, 12, 32, 17, 345, DateTimeKind.Utc).AddTicks(3560), false, "PremiumUser", "PREMIUMUSER", null },
                    { "d398c405-66a4-45f2-9e1b-33fc28a0f1bf", null, new DateTime(2025, 9, 22, 12, 32, 17, 345, DateTimeKind.Utc).AddTicks(3486), false, "Admin", "ADMIN", null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "8507eed1-38de-4de7-b606-bbbf41d1dafd");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "bfcb02e5-c78a-407f-baf2-301f78f4b319");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d398c405-66a4-45f2-9e1b-33fc28a0f1bf");

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "Sections",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "Blocks",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "CreatedAt", "IsDeleted", "Name", "NormalizedName", "UpdatedAt" },
                values: new object[,]
                {
                    { "190ba005-6849-4646-8a15-7421a686946b", null, new DateTime(2025, 9, 21, 0, 24, 48, 674, DateTimeKind.Utc).AddTicks(7540), false, "PremiumUser", "PREMIUMUSER", null },
                    { "306c92ad-623b-4abc-996a-5ef2c30d3440", null, new DateTime(2025, 9, 21, 0, 24, 48, 674, DateTimeKind.Utc).AddTicks(7462), false, "Admin", "ADMIN", null },
                    { "93688a9a-3294-485d-9ef3-0e468e7f8bf0", null, new DateTime(2025, 9, 21, 0, 24, 48, 674, DateTimeKind.Utc).AddTicks(7536), false, "FreeUser", "FREEUSER", null }
                });
        }
    }
}
