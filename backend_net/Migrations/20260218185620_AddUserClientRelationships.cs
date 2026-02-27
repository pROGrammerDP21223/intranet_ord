using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend_net.Migrations
{
    /// <inheritdoc />
    public partial class AddUserClientRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SalesManagerSalesPersons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalesManagerId = table.Column<int>(type: "int", nullable: false),
                    SalesPersonId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesManagerSalesPersons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesManagerSalesPersons_Users_SalesManagerId",
                        column: x => x.SalesManagerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesManagerSalesPersons_Users_SalesPersonId",
                        column: x => x.SalesPersonId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SalesPersonClients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalesPersonId = table.Column<int>(type: "int", nullable: false),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    ClientId1 = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesPersonClients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesPersonClients_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SalesPersonClients_Clients_ClientId1",
                        column: x => x.ClientId1,
                        principalTable: "Clients",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SalesPersonClients_Users_SalesPersonId",
                        column: x => x.SalesPersonId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserClients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    ClientId1 = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserClients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserClients_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserClients_Clients_ClientId1",
                        column: x => x.ClientId1,
                        principalTable: "Clients",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserClients_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalesManagerSalesPersons_SalesManagerId",
                table: "SalesManagerSalesPersons",
                column: "SalesManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesManagerSalesPersons_SalesManagerId_SalesPersonId",
                table: "SalesManagerSalesPersons",
                columns: new[] { "SalesManagerId", "SalesPersonId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesManagerSalesPersons_SalesPersonId",
                table: "SalesManagerSalesPersons",
                column: "SalesPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesPersonClients_ClientId",
                table: "SalesPersonClients",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesPersonClients_ClientId1",
                table: "SalesPersonClients",
                column: "ClientId1");

            migrationBuilder.CreateIndex(
                name: "IX_SalesPersonClients_SalesPersonId",
                table: "SalesPersonClients",
                column: "SalesPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesPersonClients_SalesPersonId_ClientId",
                table: "SalesPersonClients",
                columns: new[] { "SalesPersonId", "ClientId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserClients_ClientId",
                table: "UserClients",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_UserClients_ClientId1",
                table: "UserClients",
                column: "ClientId1");

            migrationBuilder.CreateIndex(
                name: "IX_UserClients_UserId",
                table: "UserClients",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserClients_UserId_ClientId",
                table: "UserClients",
                columns: new[] { "UserId", "ClientId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SalesManagerSalesPersons");

            migrationBuilder.DropTable(
                name: "SalesPersonClients");

            migrationBuilder.DropTable(
                name: "UserClients");
        }
    }
}
