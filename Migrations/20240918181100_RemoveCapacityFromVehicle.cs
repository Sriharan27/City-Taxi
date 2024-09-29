using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace City_Taxi.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCapacityFromVehicle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Capacity",
                table: "Vehicles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Capacity",
                table: "Vehicles",
                type: "int",
                nullable: true);
        }
    }
}
