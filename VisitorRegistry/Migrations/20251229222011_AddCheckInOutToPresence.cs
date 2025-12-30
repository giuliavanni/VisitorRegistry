using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VisitorRegistry.Migrations
{
    /// <inheritdoc />
    public partial class AddCheckInOutToPresence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("05cfad08-9547-4f82-84ab-ab4603d62139"));

            migrationBuilder.DropColumn(
                name: "IsInside",
                table: "Presences");

            migrationBuilder.AddColumn<DateTime>(
                name: "CheckInTime",
                table: "Presences",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CheckOutTime",
                table: "Presences",
                type: "TEXT",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Presences",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CheckInTime", "CheckOutTime" },
                values: new object[] { new DateTime(2025, 12, 24, 10, 0, 0, 0, DateTimeKind.Unspecified), null });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "FirstName", "LastName", "NickName", "Password" },
                values: new object[] { new Guid("705bebd8-fb42-4392-9b5a-badbb3750d04"), "admin@example.com", "Costanzo", "Buonarroti", "Admin", "XohImNooBHFR0OVvjcYpJ3NgPQ1qq73WKhHvch0VQtg=" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("705bebd8-fb42-4392-9b5a-badbb3750d04"));

            migrationBuilder.DropColumn(
                name: "CheckInTime",
                table: "Presences");

            migrationBuilder.DropColumn(
                name: "CheckOutTime",
                table: "Presences");

            migrationBuilder.AddColumn<int>(
                name: "IsInside",
                table: "Presences",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Presences",
                keyColumn: "Id",
                keyValue: 1,
                column: "IsInside",
                value: 1);

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "FirstName", "LastName", "NickName", "Password" },
                values: new object[] { new Guid("05cfad08-9547-4f82-84ab-ab4603d62139"), "admin@example.com", "Costanzo", "Buonarroti", "Admin", "XohImNooBHFR0OVvjcYpJ3NgPQ1qq73WKhHvch0VQtg=" });
        }
    }
}
