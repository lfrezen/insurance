using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProposalService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Proposals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InsuredPersonFullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    InsuredPersonCpf = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    InsuredPersonEmail = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CoverageType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    InsuredAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proposals", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Proposals_CreatedAt",
                table: "Proposals",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Proposals_Status",
                table: "Proposals",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Proposals");
        }
    }
}
