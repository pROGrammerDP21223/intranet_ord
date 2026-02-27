using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend_net.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEnquiryTableWithApiKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enquiries_Users_AssignedToUserId",
                table: "Enquiries");

            migrationBuilder.DropIndex(
                name: "IX_Enquiries_AssignedToUserId",
                table: "Enquiries");

            migrationBuilder.DropColumn(
                name: "AssignedToUserId",
                table: "Enquiries");

            migrationBuilder.DropColumn(
                name: "Company",
                table: "Enquiries");

            migrationBuilder.DropColumn(
                name: "Message",
                table: "Enquiries");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Enquiries");

            migrationBuilder.DropColumn(
                name: "Subject",
                table: "Enquiries");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Enquiries",
                newName: "FullName");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Enquiries",
                newName: "EmailId");

            migrationBuilder.RenameIndex(
                name: "IX_Enquiries_Email",
                table: "Enquiries",
                newName: "IX_Enquiries_EmailId");

            migrationBuilder.AddColumn<int>(
                name: "ClientId",
                table: "Enquiries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "MobileNumber",
                table: "Enquiries",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RawPayload",
                table: "Enquiries",
                type: "NVARCHAR(MAX)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ApiKeys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AllowedOrigins = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiKeys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApiKeys_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Enquiries_ClientId",
                table: "Enquiries",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Enquiries_MobileNumber",
                table: "Enquiries",
                column: "MobileNumber");

            migrationBuilder.CreateIndex(
                name: "IX_ApiKeys_ClientId",
                table: "ApiKeys",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ApiKeys_Key",
                table: "ApiKeys",
                column: "Key",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Enquiries_Clients_ClientId",
                table: "Enquiries",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enquiries_Clients_ClientId",
                table: "Enquiries");

            migrationBuilder.DropTable(
                name: "ApiKeys");

            migrationBuilder.DropIndex(
                name: "IX_Enquiries_ClientId",
                table: "Enquiries");

            migrationBuilder.DropIndex(
                name: "IX_Enquiries_MobileNumber",
                table: "Enquiries");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "Enquiries");

            migrationBuilder.DropColumn(
                name: "MobileNumber",
                table: "Enquiries");

            migrationBuilder.DropColumn(
                name: "RawPayload",
                table: "Enquiries");

            migrationBuilder.RenameColumn(
                name: "FullName",
                table: "Enquiries",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "EmailId",
                table: "Enquiries",
                newName: "Email");

            migrationBuilder.RenameIndex(
                name: "IX_Enquiries_EmailId",
                table: "Enquiries",
                newName: "IX_Enquiries_Email");

            migrationBuilder.AddColumn<int>(
                name: "AssignedToUserId",
                table: "Enquiries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Company",
                table: "Enquiries",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Message",
                table: "Enquiries",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Enquiries",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Subject",
                table: "Enquiries",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Enquiries_AssignedToUserId",
                table: "Enquiries",
                column: "AssignedToUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Enquiries_Users_AssignedToUserId",
                table: "Enquiries",
                column: "AssignedToUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
