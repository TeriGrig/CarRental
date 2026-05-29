using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarRental.Migrations
{
    /// <inheritdoc />
    public partial class AddSuspensionEnd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Issuspended",
                table: "AspNetUsers",
                newName: "IsSuspended");

            migrationBuilder.AddColumn<DateOnly>(
                name: "SuspensionEnd",
                table: "AspNetUsers",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SuspensionEnd",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "IsSuspended",
                table: "AspNetUsers",
                newName: "Issuspended");
        }
    }
}
