using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Airbee.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSensorCity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Sensors",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            // Sensors saved before this migration all came from the Osijek area
            migrationBuilder.Sql("UPDATE \"Sensors\" SET \"City\" = 'Osijek' WHERE \"City\" = '';");

            migrationBuilder.CreateIndex(
                name: "IX_Sensors_City",
                table: "Sensors",
                column: "City");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sensors_City",
                table: "Sensors");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Sensors");
        }
    }
}
