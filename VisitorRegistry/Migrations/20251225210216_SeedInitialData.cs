using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VisitorRegistry.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Timestamp",
                table: "Presences",
                newName: "Date");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "FirstName", "LastName", "NickName", "Password" },
                values: new object[] { new Guid("77b1a967-bc12-45d0-b088-45fbf062c324"), "admin@example.com", "Costanzo", "Buonarroti", "Admin", "password" });

            migrationBuilder.InsertData(
                table: "Visitors",
                columns: new[] { "Id", "Cognome", "DataVisita", "Nome", "QrCode" },
                values: new object[] { 1, "Rossi", new DateTime(2025, 12, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "Mario", "abcdefg" });

            migrationBuilder.InsertData(
                table: "Presences",
                columns: new[] { "Id", "Date", "IsInside", "VisitorId" },
                values: new object[] { 1, new DateTime(2025, 12, 24, 10, 0, 0, 0, DateTimeKind.Unspecified), 1, 1 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Presences",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("77b1a967-bc12-45d0-b088-45fbf062c324"));

            migrationBuilder.DeleteData(
                table: "Visitors",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "Presences",
                newName: "Timestamp");
        }
    }
}
