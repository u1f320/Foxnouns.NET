using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foxnouns.Backend.Database.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(DatabaseContext))]
    [Migration("20241225155818_AddLegacyIds")]
    public partial class AddLegacyIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "legacy_id",
                table: "users",
                type: "text",
                nullable: false,
                defaultValueSql: "gen_random_uuid()"
            );

            migrationBuilder.AddColumn<string>(
                name: "legacy_id",
                table: "pride_flags",
                type: "text",
                nullable: false,
                defaultValueSql: "gen_random_uuid()"
            );

            migrationBuilder.AddColumn<string>(
                name: "legacy_id",
                table: "members",
                type: "text",
                nullable: false,
                defaultValueSql: "gen_random_uuid()"
            );

            migrationBuilder.CreateIndex(
                name: "ix_users_legacy_id",
                table: "users",
                column: "legacy_id",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_pride_flags_legacy_id",
                table: "pride_flags",
                column: "legacy_id",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_members_legacy_id",
                table: "members",
                column: "legacy_id",
                unique: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "ix_users_legacy_id", table: "users");

            migrationBuilder.DropIndex(name: "ix_pride_flags_legacy_id", table: "pride_flags");

            migrationBuilder.DropIndex(name: "ix_members_legacy_id", table: "members");

            migrationBuilder.DropColumn(name: "legacy_id", table: "users");

            migrationBuilder.DropColumn(name: "legacy_id", table: "pride_flags");

            migrationBuilder.DropColumn(name: "legacy_id", table: "members");
        }
    }
}
