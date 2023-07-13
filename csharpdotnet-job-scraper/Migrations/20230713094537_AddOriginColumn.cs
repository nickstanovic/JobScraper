using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace indeed_scraper.Migrations
{
    /// <inheritdoc />
    public partial class AddOriginColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Applied",
                table: "Jobs");

            migrationBuilder.RenameColumn(
                name: "ScrapeTimestamp",
                table: "Jobs",
                newName: "ScrapedAt");

            migrationBuilder.RenameColumn(
                name: "ApplyButton",
                table: "Jobs",
                newName: "SearchTerm");

            migrationBuilder.AddColumn<string>(
                name: "ApplyUrl",
                table: "Jobs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Origin",
                table: "Jobs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApplyUrl",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "Origin",
                table: "Jobs");

            migrationBuilder.RenameColumn(
                name: "SearchTerm",
                table: "Jobs",
                newName: "ApplyButton");

            migrationBuilder.RenameColumn(
                name: "ScrapedAt",
                table: "Jobs",
                newName: "ScrapeTimestamp");

            migrationBuilder.AddColumn<bool>(
                name: "Applied",
                table: "Jobs",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
