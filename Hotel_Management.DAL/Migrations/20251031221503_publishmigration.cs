using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hotel_Management.DAL.Migrations
{
    /// <inheritdoc />
    public partial class publishmigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1",
                column: "ConcurrencyStamp",
                value: "4085242f-08a8-4e62-bc37-78501cbf4144");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2",
                column: "ConcurrencyStamp",
                value: "aa933e9b-c0b8-4eee-a5e0-aaddde377fe4");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id",
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp" },
                values: new object[] { "7798178a-c0c4-488e-aed2-7fcea2f9f4b1", new DateTime(2025, 10, 31, 22, 15, 2, 505, DateTimeKind.Utc).AddTicks(9319), "AQAAAAIAAYagAAAAEKCTtDpIOVJ/JxwFlNmOmxwmLV+bWFchhGFqWlNJ4JnzKUTQpQpNqkmOxgSRPEGEFA==", "0e887ea4-4c7b-429c-ad13-517479964643" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1",
                column: "ConcurrencyStamp",
                value: "99b4010f-3223-4842-9a99-4313db803cc7");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2",
                column: "ConcurrencyStamp",
                value: "f1b55dd1-3000-4648-b99e-11c1d06db897");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id",
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp" },
                values: new object[] { "4e6e8bf7-80c5-4322-a24c-935c1efb6b1a", new DateTime(2025, 10, 31, 21, 1, 12, 182, DateTimeKind.Utc).AddTicks(2300), "AQAAAAIAAYagAAAAEEhaBTdrJ/uaZErPytdJYLkEDoexBcXIObnTSVW0KIDGZl7Bm0X/ShV0UZlWkxNMPg==", "48715927-fb43-49bb-9d32-610c708f8654" });
        }
    }
}
