using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VisitorRegistry.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDateFromPresence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("705bebd8-fb42-4392-9b5a-badbb3750d04"));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "FirstName", "LastName", "NickName", "Password" },
                values: new object[] { new Guid("847146fe-603c-4955-a4d7-03688f522a83"), "admin@example.com", "Costanzo", "Buonarroti", "Admin", "XohImNooBHFR0OVvjcYpJ3NgPQ1qq73WKhHvch0VQtg=" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("847146fe-603c-4955-a4d7-03688f522a83"));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "FirstName", "LastName", "NickName", "Password" },
                values: new object[] { new Guid("705bebd8-fb42-4392-9b5a-badbb3750d04"), "admin@example.com", "Costanzo", "Buonarroti", "Admin", "XohImNooBHFR0OVvjcYpJ3NgPQ1qq73WKhHvch0VQtg=" });
        }
    }
}
