using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace City_Taxi.Migrations
{
    /// <inheritdoc />
    public partial class DriverBranchForeignKeySet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BranchCode",
                table: "Drivers");

            migrationBuilder.AddColumn<int>(
                name: "BranchID",
                table: "Drivers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_BranchID",
                table: "Drivers",
                column: "BranchID");

            migrationBuilder.AddForeignKey(
                name: "FK_Drivers_BankBranches_BranchID",
                table: "Drivers",
                column: "BranchID",
                principalTable: "BankBranches",
                principalColumn: "BranchID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Drivers_BankBranches_BranchID",
                table: "Drivers");

            migrationBuilder.DropIndex(
                name: "IX_Drivers_BranchID",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "BranchID",
                table: "Drivers");

            migrationBuilder.AddColumn<string>(
                name: "BranchCode",
                table: "Drivers",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");
        }
    }
}
