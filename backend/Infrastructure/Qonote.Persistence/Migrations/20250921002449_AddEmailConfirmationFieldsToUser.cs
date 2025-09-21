using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Qonote.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailConfirmationFieldsToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "39cf4084-1323-43e6-a2af-3d9dded6418d");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "873f92ce-849b-4e67-9252-dcaf88d6be7c");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "ca5be020-cb21-4b75-bf4b-635e2b609227");

            migrationBuilder.AddColumn<string>(
                name: "EmailConfirmationCode",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EmailConfirmationCodeExpiry",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropColumn(
                name: "EmailConfirmationCode",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "EmailConfirmationCodeExpiry",
                table: "AspNetUsers");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "CreatedAt", "IsDeleted", "Name", "NormalizedName", "UpdatedAt" },
                values: new object[,]
                {
                    { "39cf4084-1323-43e6-a2af-3d9dded6418d", null, new DateTime(2025, 9, 16, 19, 58, 11, 781, DateTimeKind.Utc).AddTicks(6051), false, "Admin", "ADMIN", null },
                    { "873f92ce-849b-4e67-9252-dcaf88d6be7c", null, new DateTime(2025, 9, 16, 19, 58, 11, 781, DateTimeKind.Utc).AddTicks(6123), false, "PremiumUser", "PREMIUMUSER", null },
                    { "ca5be020-cb21-4b75-bf4b-635e2b609227", null, new DateTime(2025, 9, 16, 19, 58, 11, 781, DateTimeKind.Utc).AddTicks(6119), false, "FreeUser", "FREEUSER", null }
                });
        }
    }
}
