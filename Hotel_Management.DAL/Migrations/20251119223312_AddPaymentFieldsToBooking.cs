using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hotel_Management.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentFieldsToBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPaid",
                table: "Bookings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaymentDate",
                table: "Bookings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RefundAmount",
                table: "Bookings",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StripePaymentIntentId",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1",
                column: "ConcurrencyStamp",
                value: "53c9e902-045c-46fb-aee3-f0eb592931f3");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2",
                column: "ConcurrencyStamp",
                value: "c594ef51-71c7-4c56-8ea6-5ad13be66a51");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id",
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp" },
                values: new object[] { "785b5531-ad5f-429a-8373-a2319c7982cc", new DateTime(2025, 11, 19, 22, 33, 10, 508, DateTimeKind.Utc).AddTicks(9669), "AQAAAAIAAYagAAAAEE6nee1LB2aVrLTLHMcCbWAOA5B4SCPkBvIuSOBUoA0hqP0kjwcG6vzhowIBv3HbFg==", "cdda51e9-36ac-493a-a6c0-00f64897bded" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPaid",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "PaymentDate",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "RefundAmount",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "StripePaymentIntentId",
                table: "Bookings");

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
    }
}
