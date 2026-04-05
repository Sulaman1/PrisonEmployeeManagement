using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrisonEmployeeManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddPersonalfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 4, 6, 0, 9, 2, 901, DateTimeKind.Local).AddTicks(1658), new DateTime(2026, 4, 6, 0, 9, 2, 901, DateTimeKind.Local).AddTicks(1726) });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 4, 6, 0, 9, 2, 901, DateTimeKind.Local).AddTicks(1751), new DateTime(2026, 4, 6, 0, 9, 2, 901, DateTimeKind.Local).AddTicks(1756) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 4, 5, 23, 23, 37, 634, DateTimeKind.Local).AddTicks(1362), new DateTime(2026, 4, 5, 23, 23, 37, 634, DateTimeKind.Local).AddTicks(1430) });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 4, 5, 23, 23, 37, 634, DateTimeKind.Local).AddTicks(1452), new DateTime(2026, 4, 5, 23, 23, 37, 634, DateTimeKind.Local).AddTicks(1456) });
        }
    }
}
