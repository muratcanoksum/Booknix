using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Booknix.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DeleteTokenFieldsToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeleteToken",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeleteTokenRequesAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeleteToken",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DeleteTokenRequesAt",
                table: "Users");
        }
    }
}
