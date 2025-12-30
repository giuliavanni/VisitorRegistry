using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VisitorRegistry.Migrations
{
    /// <inheritdoc />
    public partial class UpdateVisitorTab : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("3f802096-8cd0-40ca-abd5-92d937b5ece9"));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "FirstName", "LastName", "NickName", "Password" },
                values: new object[] { new Guid("05cfad08-9547-4f82-84ab-ab4603d62139"), "admin@example.com", "Costanzo", "Buonarroti", "Admin", "XohImNooBHFR0OVvjcYpJ3NgPQ1qq73WKhHvch0VQtg=" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("05cfad08-9547-4f82-84ab-ab4603d62139"));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "FirstName", "LastName", "NickName", "Password" },
                values: new object[] { new Guid("3f802096-8cd0-40ca-abd5-92d937b5ece9"), "admin@example.com", "Costanzo", "Buonarroti", "Admin", "XohImNooBHFR0OVvjcYpJ3NgPQ1qq73WKhHvch0VQtg=" });
        }
    }
}
