using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend_net.Migrations
{
    /// <inheritdoc />
    public partial class AddSalesManagerClientAndOwnerClient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Documents_ParentDocumentId",
                table: "Documents");

            migrationBuilder.CreateTable(
                name: "OwnerClients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OwnerId = table.Column<int>(type: "int", nullable: false),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OwnerClients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OwnerClients_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OwnerClients_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SalesManagerClients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalesManagerId = table.Column<int>(type: "int", nullable: false),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesManagerClients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesManagerClients_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SalesManagerClients_Users_SalesManagerId",
                        column: x => x.SalesManagerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OwnerClients_ClientId",
                table: "OwnerClients",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_OwnerClients_OwnerId",
                table: "OwnerClients",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_OwnerClients_OwnerId_ClientId",
                table: "OwnerClients",
                columns: new[] { "OwnerId", "ClientId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesManagerClients_ClientId",
                table: "SalesManagerClients",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesManagerClients_SalesManagerId",
                table: "SalesManagerClients",
                column: "SalesManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesManagerClients_SalesManagerId_ClientId",
                table: "SalesManagerClients",
                columns: new[] { "SalesManagerId", "ClientId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Documents_ParentDocumentId",
                table: "Documents",
                column: "ParentDocumentId",
                principalTable: "Documents",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Documents_ParentDocumentId",
                table: "Documents");

            migrationBuilder.DropTable(
                name: "OwnerClients");

            migrationBuilder.DropTable(
                name: "SalesManagerClients");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Documents_ParentDocumentId",
                table: "Documents",
                column: "ParentDocumentId",
                principalTable: "Documents",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
