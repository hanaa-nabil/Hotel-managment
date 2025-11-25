using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hotel_Management.DAL.Migrations
{
    /// <inheritdoc />
    public partial class admindashboard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1",
                column: "ConcurrencyStamp",
                value: "3d65c921-651b-45e3-964a-13bf14123dd7");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2",
                column: "ConcurrencyStamp",
                value: "edc972c8-a79d-4f4d-ae19-07e36aab79d8");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id",
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp" },
                values: new object[] { "3e7ee7fb-e28a-4790-8be3-211b7117d1ae", new DateTime(2025, 10, 31, 19, 11, 5, 406, DateTimeKind.Utc).AddTicks(8948), "AQAAAAIAAYagAAAAEFm1OrUXkUBrtQxzKej3LDIlZZzgY8Y0a/QZJBR6PP2AfRjtKg0but9U6K5FQn+HwQ==", "ab514a0b-7eda-4ef4-84e9-2349765891a2" });
        }
    }
}
