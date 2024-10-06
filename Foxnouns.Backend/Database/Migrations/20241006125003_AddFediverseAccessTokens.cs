using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace Foxnouns.Backend.Database.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(DatabaseContext))]
    [Migration("20241006125003_AddFediverseAccessTokens")]
    public partial class AddFediverseAccessTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "access_token",
                table: "fediverse_applications",
                type: "text",
                nullable: true
            );

            migrationBuilder.AddColumn<Instant>(
                name: "token_valid_until",
                table: "fediverse_applications",
                type: "timestamp with time zone",
                nullable: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "access_token", table: "fediverse_applications");

            migrationBuilder.DropColumn(name: "token_valid_until", table: "fediverse_applications");
        }
    }
}
