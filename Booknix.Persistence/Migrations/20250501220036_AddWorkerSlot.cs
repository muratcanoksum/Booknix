using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Booknix.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkerSlot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppointmentSlots_Users_AssignedEmployeeId",
                table: "AppointmentSlots");

            migrationBuilder.RenameColumn(
                name: "AssignedEmployeeId",
                table: "AppointmentSlots",
                newName: "AssignerWorkerId");

            migrationBuilder.RenameIndex(
                name: "IX_AppointmentSlots_AssignedEmployeeId",
                table: "AppointmentSlots",
                newName: "IX_AppointmentSlots_AssignerWorkerId");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Appointments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentSlots_Workers_AssignerWorkerId",
                table: "AppointmentSlots",
                column: "AssignerWorkerId",
                principalTable: "Workers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppointmentSlots_Workers_AssignerWorkerId",
                table: "AppointmentSlots");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Appointments");

            migrationBuilder.RenameColumn(
                name: "AssignerWorkerId",
                table: "AppointmentSlots",
                newName: "AssignedEmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_AppointmentSlots_AssignerWorkerId",
                table: "AppointmentSlots",
                newName: "IX_AppointmentSlots_AssignedEmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentSlots_Users_AssignedEmployeeId",
                table: "AppointmentSlots",
                column: "AssignedEmployeeId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
