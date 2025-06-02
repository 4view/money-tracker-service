using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoneyTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class unixTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Time",
                table: "ExpenseEntity");

            migrationBuilder.AddColumn<long>(
                name: "TimeUnix",
                table: "ExpenseEntity",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimeUnix",
                table: "ExpenseEntity");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "Time",
                table: "ExpenseEntity",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));
        }
    }
}
