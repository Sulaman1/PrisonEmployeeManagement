using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrisonEmployeeManagement.Migrations
{
    /// <inheritdoc />
    public partial class InitFileTrack : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 4, 3, 4, 14, 15, 492, DateTimeKind.Local).AddTicks(3284), new DateTime(2026, 4, 3, 4, 14, 15, 492, DateTimeKind.Local).AddTicks(3404) });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 4, 3, 4, 14, 15, 492, DateTimeKind.Local).AddTicks(3428), new DateTime(2026, 4, 3, 4, 14, 15, 492, DateTimeKind.Local).AddTicks(3436) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 4, 3, 4, 9, 35, 672, DateTimeKind.Local).AddTicks(2508), new DateTime(2026, 4, 3, 4, 9, 35, 672, DateTimeKind.Local).AddTicks(2645) });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 4, 3, 4, 9, 35, 672, DateTimeKind.Local).AddTicks(2672), new DateTime(2026, 4, 3, 4, 9, 35, 672, DateTimeKind.Local).AddTicks(2682) });
        }
    }
}
