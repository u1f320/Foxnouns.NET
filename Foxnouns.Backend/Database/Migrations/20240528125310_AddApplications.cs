using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foxnouns.Backend.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddApplications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "application_id",
                table: "tokens",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<byte[]>(
                name: "hash",
                table: "tokens",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.CreateTable(
                name: "applications",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    client_id = table.Column<string>(type: "text", nullable: false),
                    client_secret = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    scopes = table.Column<string[]>(type: "text[]", nullable: false),
                    redirect_uris = table.Column<string[]>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_applications", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_tokens_application_id",
                table: "tokens",
                column: "application_id");

            migrationBuilder.AddForeignKey(
                name: "fk_tokens_applications_application_id",
                table: "tokens",
                column: "application_id",
                principalTable: "applications",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_tokens_applications_application_id",
                table: "tokens");

            migrationBuilder.DropTable(
                name: "applications");

            migrationBuilder.DropIndex(
                name: "ix_tokens_application_id",
                table: "tokens");

            migrationBuilder.DropColumn(
                name: "application_id",
                table: "tokens");

            migrationBuilder.DropColumn(
                name: "hash",
                table: "tokens");
        }
    }
}
