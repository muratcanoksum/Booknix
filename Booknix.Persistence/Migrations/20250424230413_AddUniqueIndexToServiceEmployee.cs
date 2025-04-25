using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Booknix.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexToServiceEmployee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceEmployees_Workers_EmployeeId",
                table: "ServiceEmployees");

            migrationBuilder.DropIndex(
                name: "IX_ServiceEmployees_ServiceId",
                table: "ServiceEmployees");

            migrationBuilder.RenameColumn(
                name: "EmployeeId",
                table: "ServiceEmployees",
                newName: "WorkerId");

            migrationBuilder.RenameIndex(
                name: "IX_ServiceEmployees_EmployeeId",
                table: "ServiceEmployees",
                newName: "IX_ServiceEmployees_WorkerId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceEmployees_ServiceId_WorkerId",
                table: "ServiceEmployees",
                columns: new[] { "ServiceId", "WorkerId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceEmployees_Workers_WorkerId",
                table: "ServiceEmployees",
                column: "WorkerId",
                principalTable: "Workers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceEmployees_Workers_WorkerId",
                table: "ServiceEmployees");

            migrationBuilder.DropIndex(
                name: "IX_ServiceEmployees_ServiceId_WorkerId",
                table: "ServiceEmployees");

            migrationBuilder.RenameColumn(
                name: "WorkerId",
                table: "ServiceEmployees",
                newName: "EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_ServiceEmployees_WorkerId",
                table: "ServiceEmployees",
                newName: "IX_ServiceEmployees_EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceEmployees_ServiceId",
                table: "ServiceEmployees",
                column: "ServiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceEmployees_Workers_EmployeeId",
                table: "ServiceEmployees",
                column: "EmployeeId",
                principalTable: "Workers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
