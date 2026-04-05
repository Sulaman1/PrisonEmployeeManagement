using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrisonEmployeeManagement.Migrations
{
    /// <inheritdoc />
    public partial class fixnotification2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 4, 5, 20, 42, 13, 677, DateTimeKind.Local).AddTicks(1141), new DateTime(2026, 4, 5, 20, 42, 13, 677, DateTimeKind.Local).AddTicks(1220) });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 4, 5, 20, 42, 13, 677, DateTimeKind.Local).AddTicks(1240), new DateTime(2026, 4, 5, 20, 42, 13, 677, DateTimeKind.Local).AddTicks(1245) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
    }
}
