using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoneyTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class newPorperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmailConfirmationToken",
                table: "UserEntity",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EmailConfirmationTokenExpiry",
                table: "UserEntity",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEmailConfirmed",
                table: "UserEntity",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PasswordResetToken",
                table: "UserEntity",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PasswordResetTokenExpiry",
                table: "UserEntity",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailConfirmationToken",
                table: "UserEntity");

            migrationBuilder.DropColumn(
                name: "EmailConfirmationTokenExpiry",
                table: "UserEntity");

            migrationBuilder.DropColumn(
                name: "IsEmailConfirmed",
                table: "UserEntity");

            migrationBuilder.DropColumn(
                name: "PasswordResetToken",
                table: "UserEntity");

            migrationBuilder.DropColumn(
                name: "PasswordResetTokenExpiry",
                table: "UserEntity");
        }
    }
}
