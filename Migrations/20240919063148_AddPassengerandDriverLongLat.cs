using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace City_Taxi.Migrations
{
    /// <inheritdoc />
    public partial class AddPassengerandDriverLongLat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "PricePerKM",
                table: "VehicleTypes",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)");

            migrationBuilder.AddColumn<string>(
                name: "CurrentLatitude",
                table: "Passengers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CurrentLongitude",
                table: "Passengers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "DriverStatuses",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "DriverStatuses",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentLatitude",
                table: "Passengers");

            migrationBuilder.DropColumn(
                name: "CurrentLongitude",
                table: "Passengers");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "DriverStatuses");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "DriverStatuses");

            migrationBuilder.AlterColumn<decimal>(
                name: "PricePerKM",
                table: "VehicleTypes",
                type: "decimal(10,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");
        }
    }
}
