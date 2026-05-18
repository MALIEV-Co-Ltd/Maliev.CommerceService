using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maliev.CommerceService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ExtendProductBomDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssemblyName",
                table: "ProductBomItems",
                type: "character varying(160)",
                maxLength: 160,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DrawingUrl",
                table: "ProductBomItems",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "ProductBomItems",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LeadTimeDays",
                table: "ProductBomItems",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PartNumber",
                table: "ProductBomItems",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SourcingTimeDays",
                table: "ProductBomItems",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubassemblyName",
                table: "ProductBomItems",
                type: "character varying(160)",
                maxLength: 160,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupplierName",
                table: "ProductBomItems",
                type: "character varying(240)",
                maxLength: 240,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupplierUrl",
                table: "ProductBomItems",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssemblyName",
                table: "ProductBomItems");

            migrationBuilder.DropColumn(
                name: "DrawingUrl",
                table: "ProductBomItems");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "ProductBomItems");

            migrationBuilder.DropColumn(
                name: "LeadTimeDays",
                table: "ProductBomItems");

            migrationBuilder.DropColumn(
                name: "PartNumber",
                table: "ProductBomItems");

            migrationBuilder.DropColumn(
                name: "SourcingTimeDays",
                table: "ProductBomItems");

            migrationBuilder.DropColumn(
                name: "SubassemblyName",
                table: "ProductBomItems");

            migrationBuilder.DropColumn(
                name: "SupplierName",
                table: "ProductBomItems");

            migrationBuilder.DropColumn(
                name: "SupplierUrl",
                table: "ProductBomItems");
        }
    }
}
