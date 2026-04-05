using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrisonEmployeeManagement.Migrations
{
    /// <inheritdoc />
    public partial class fixnotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 4, 5, 17, 58, 24, 443, DateTimeKind.Local).AddTicks(3943), new DateTime(2026, 4, 5, 17, 58, 24, 443, DateTimeKind.Local).AddTicks(3995) });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 4, 5, 17, 58, 24, 443, DateTimeKind.Local).AddTicks(4016), new DateTime(2026, 4, 5, 17, 58, 24, 443, DateTimeKind.Local).AddTicks(4024) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 4, 5, 15, 52, 56, 587, DateTimeKind.Local).AddTicks(8016), new DateTime(2026, 4, 5, 15, 52, 56, 587, DateTimeKind.Local).AddTicks(8108) });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 4, 5, 15, 52, 56, 587, DateTimeKind.Local).AddTicks(8134), new DateTime(2026, 4, 5, 15, 52, 56, 587, DateTimeKind.Local).AddTicks(8140) });
        }
    }
}
