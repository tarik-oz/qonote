using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Qonote.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDeletedAtColumnsAndNoteIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Notes_NoteGroupId",
                table: "Notes");

            migrationBuilder.DropIndex(
                name: "IX_Notes_UserId",
                table: "Notes");

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

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Sections",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Notes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "NoteGroups",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Blocks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "AspNetRoles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "CreatedAt", "DeletedAt", "IsDeleted", "Name", "NormalizedName", "UpdatedAt" },
                values: new object[,]
                {
                    { "02323320-fe52-4e78-883c-dc33d8e1d8f2", null, new DateTime(2025, 9, 25, 4, 54, 45, 361, DateTimeKind.Utc).AddTicks(2020), null, false, "FreeUser", "FREEUSER", null },
                    { "03ce4f0d-1412-4108-9da7-b8c87d28774d", null, new DateTime(2025, 9, 25, 4, 54, 45, 361, DateTimeKind.Utc).AddTicks(1925), null, false, "Admin", "ADMIN", null },
                    { "71b03e31-c939-4f28-a387-331dc25ad1c9", null, new DateTime(2025, 9, 25, 4, 54, 45, 361, DateTimeKind.Utc).AddTicks(2032), null, false, "PremiumUser", "PREMIUMUSER", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notes_NoteGroupId_IsDeleted_UpdatedAt",
                table: "Notes",
                columns: new[] { "NoteGroupId", "IsDeleted", "UpdatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Notes_UserId_IsDeleted_UpdatedAt",
                table: "Notes",
                columns: new[] { "UserId", "IsDeleted", "UpdatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Notes_NoteGroupId_IsDeleted_UpdatedAt",
                table: "Notes");

            migrationBuilder.DropIndex(
                name: "IX_Notes_UserId_IsDeleted_UpdatedAt",
                table: "Notes");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "02323320-fe52-4e78-883c-dc33d8e1d8f2");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "03ce4f0d-1412-4108-9da7-b8c87d28774d");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "71b03e31-c939-4f28-a387-331dc25ad1c9");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Sections");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "NoteGroups");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Blocks");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "AspNetRoles");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "CreatedAt", "IsDeleted", "Name", "NormalizedName", "UpdatedAt" },
                values: new object[,]
                {
                    { "8507eed1-38de-4de7-b606-bbbf41d1dafd", null, new DateTime(2025, 9, 22, 12, 32, 17, 345, DateTimeKind.Utc).AddTicks(3548), false, "FreeUser", "FREEUSER", null },
                    { "bfcb02e5-c78a-407f-baf2-301f78f4b319", null, new DateTime(2025, 9, 22, 12, 32, 17, 345, DateTimeKind.Utc).AddTicks(3560), false, "PremiumUser", "PREMIUMUSER", null },
                    { "d398c405-66a4-45f2-9e1b-33fc28a0f1bf", null, new DateTime(2025, 9, 22, 12, 32, 17, 345, DateTimeKind.Utc).AddTicks(3486), false, "Admin", "ADMIN", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notes_NoteGroupId",
                table: "Notes",
                column: "NoteGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_UserId",
                table: "Notes",
                column: "UserId");
        }
    }
}
