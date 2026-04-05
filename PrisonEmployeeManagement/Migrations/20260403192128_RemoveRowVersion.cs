using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrisonEmployeeManagement.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRowVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 4, 4, 0, 21, 25, 521, DateTimeKind.Local).AddTicks(6079), new DateTime(2026, 4, 4, 0, 21, 25, 521, DateTimeKind.Local).AddTicks(6250) });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 4, 4, 0, 21, 25, 521, DateTimeKind.Local).AddTicks(6274), new DateTime(2026, 4, 4, 0, 21, 25, 521, DateTimeKind.Local).AddTicks(6281) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
    }
}
