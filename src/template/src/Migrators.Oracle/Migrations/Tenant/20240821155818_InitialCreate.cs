using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migrators.Oracle.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "c##gnx_identity");

            migrationBuilder.CreateTable(
                name: "Tenants",
                schema: "c##gnx_identity",
                columns: table => new
                {
                    Id = table.Column<string>(type: "NVARCHAR2(64)", maxLength: 64, nullable: false),
                    ConnectionString = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    AdminEmail = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    IsActive = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    ValidUpTo = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    Issuer = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Identifier = table.Column<string>(type: "NVARCHAR2(450)", nullable: true),
                    Name = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Identifier",
                schema: "c##gnx_identity",
                table: "Tenants",
                column: "Identifier",
                unique: true,
                filter: "\"Identifier\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tenants",
                schema: "c##gnx_identity");
        }
    }
}
