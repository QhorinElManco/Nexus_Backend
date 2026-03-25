using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Nexus.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWarehouseTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Warehouses");

            migrationBuilder.AddColumn<long>(
                name: "WarehouseTypeId",
                table: "Warehouses",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "WarehouseTypes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarehouseTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WarehouseTypes_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Warehouses_WarehouseTypeId",
                table: "Warehouses",
                column: "WarehouseTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseTypes_CompanyId",
                table: "WarehouseTypes",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseTypes_CompanyId_Name",
                table: "WarehouseTypes",
                columns: ["CompanyId", "Name"],
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Warehouses_WarehouseTypes_WarehouseTypeId",
                table: "Warehouses",
                column: "WarehouseTypeId",
                principalTable: "WarehouseTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Warehouses_WarehouseTypes_WarehouseTypeId",
                table: "Warehouses");

            migrationBuilder.DropTable(
                name: "WarehouseTypes");

            migrationBuilder.DropIndex(
                name: "IX_Warehouses_WarehouseTypeId",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "WarehouseTypeId",
                table: "Warehouses");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Warehouses",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }
    }
}
