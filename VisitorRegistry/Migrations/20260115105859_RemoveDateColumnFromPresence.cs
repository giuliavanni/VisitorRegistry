using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VisitorRegistry.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDateColumnFromPresence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("847146fe-603c-4955-a4d7-03688f522a83"));

            migrationBuilder.DropColumn(
                name: "Date",
                table: "Presences");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "FirstName", "LastName", "NickName", "Password" },
                values: new object[] { new Guid("10f9caa4-b399-44a5-be91-6c91f95bf555"), "admin@example.com", "Costanzo", "Buonarroti", "Admin", "XohImNooBHFR0OVvjcYpJ3NgPQ1qq73WKhHvch0VQtg=" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("10f9caa4-b399-44a5-be91-6c91f95bf555"));

            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "Presences",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Presences",
                keyColumn: "Id",
                keyValue: 1,
                column: "Date",
                value: new DateTime(2025, 12, 24, 10, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "FirstName", "LastName", "NickName", "Password" },
                values: new object[] { new Guid("847146fe-603c-4955-a4d7-03688f522a83"), "admin@example.com", "Costanzo", "Buonarroti", "Admin", "XohImNooBHFR0OVvjcYpJ3NgPQ1qq73WKhHvch0VQtg=" });
        }
    }
}
