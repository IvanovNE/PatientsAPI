using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PatientsAPI.Infrastructure.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "patients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    name_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name_use = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    name_family = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    name_given = table.Column<string>(type: "jsonb", nullable: false),
                    Gender = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    birth_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_patients", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_patients_birth_date",
                table: "patients",
                column: "birth_date");

            migrationBuilder.CreateIndex(
                name: "ix_patients_name_family",
                table: "patients",
                column: "name_family");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "patients");
        }
    }
}
