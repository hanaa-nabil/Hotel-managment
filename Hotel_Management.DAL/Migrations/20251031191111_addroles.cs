using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Hotel_Management.DAL.Migrations
{
    /// <inheritdoc />
    public partial class addroles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "1", "3d65c921-651b-45e3-964a-13bf14123dd7", "Admin", "ADMIN" },
                    { "2", "edc972c8-a79d-4f4d-ae19-07e36aab79d8", "User", "USER" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "CreatedAt", "Email", "EmailConfirmed", "FirstName", "LastName", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "admin-user-id", 0, "3e7ee7fb-e28a-4790-8be3-211b7117d1ae", new DateTime(2025, 10, 31, 19, 11, 5, 406, DateTimeKind.Utc).AddTicks(8948), "admin@hotel.com", true, "Admin", "User", false, null, "ADMIN@HOTEL.COM", "ADMIN@HOTEL.COM", "AQAAAAIAAYagAAAAEFm1OrUXkUBrtQxzKej3LDIlZZzgY8Y0a/QZJBR6PP2AfRjtKg0but9U6K5FQn+HwQ==", null, false, "ab514a0b-7eda-4ef4-84e9-2349765891a2", false, "admin@hotel.com" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "1", "admin-user-id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2");

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "1", "admin-user-id" });

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id");
        }
    }
}
