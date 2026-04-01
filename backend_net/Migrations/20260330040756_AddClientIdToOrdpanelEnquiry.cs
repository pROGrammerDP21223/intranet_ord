using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend_net.Migrations
{
    /// <inheritdoc />
    public partial class AddClientIdToOrdpanelEnquiry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH('OrdpanelEnquiries', 'ListingClientId') IS NULL
BEGIN
    ALTER TABLE [OrdpanelEnquiries] ADD [ListingClientId] nvarchar(50) NULL;
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH('OrdpanelEnquiries', 'ListingClientId') IS NOT NULL
BEGIN
    ALTER TABLE [OrdpanelEnquiries] DROP COLUMN [ListingClientId];
END
");
        }
    }
}
