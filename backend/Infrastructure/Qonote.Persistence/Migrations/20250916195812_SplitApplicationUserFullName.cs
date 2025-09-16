using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Qonote.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SplitApplicationUserFullName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "39c7e45d-b301-4b71-b6f3-f814242614a9");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "6763807d-cdb6-401d-9dec-efecfa30fd5a");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a03877ce-076c-4ae8-8a56-ab183340429d");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "AspNetUsers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Surname",
                table: "AspNetUsers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropColumn(
                name: "Name",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Surname",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "AspNetUsers",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "CreatedAt", "IsDeleted", "Name", "NormalizedName", "UpdatedAt" },
                values: new object[,]
                {
                    { "39c7e45d-b301-4b71-b6f3-f814242614a9", null, new DateTime(2025, 9, 6, 1, 36, 33, 600, DateTimeKind.Utc).AddTicks(4008), false, "Admin", "ADMIN", null },
                    { "6763807d-cdb6-401d-9dec-efecfa30fd5a", null, new DateTime(2025, 9, 6, 1, 36, 33, 600, DateTimeKind.Utc).AddTicks(4063), false, "FreeUser", "FREEUSER", null },
                    { "a03877ce-076c-4ae8-8a56-ab183340429d", null, new DateTime(2025, 9, 6, 1, 36, 33, 600, DateTimeKind.Utc).AddTicks(4067), false, "PremiumUser", "PREMIUMUSER", null }
                });
        }
    }
}
