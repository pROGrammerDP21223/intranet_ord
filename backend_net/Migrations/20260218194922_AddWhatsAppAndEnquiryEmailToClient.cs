using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend_net.Migrations
{
    /// <inheritdoc />
    public partial class AddWhatsAppAndEnquiryEmailToClient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EnquiryEmail",
                table: "Clients",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "UseSameEmailForEnquiries",
                table: "Clients",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "UseWhatsAppService",
                table: "Clients",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "WhatsAppNumber",
                table: "Clients",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "WhatsAppSameAsMobile",
                table: "Clients",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnquiryEmail",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "UseSameEmailForEnquiries",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "UseWhatsAppService",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "WhatsAppNumber",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "WhatsAppSameAsMobile",
                table: "Clients");
        }
    }
}
