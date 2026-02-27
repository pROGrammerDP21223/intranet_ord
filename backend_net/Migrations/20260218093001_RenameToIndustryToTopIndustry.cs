using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend_net.Migrations
{
    /// <inheritdoc />
    public partial class RenameToIndustryToTopIndustry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ToIndustry",
                table: "Industries",
                newName: "TopIndustry");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TopIndustry",
                table: "Industries",
                newName: "ToIndustry");
        }
    }
}
