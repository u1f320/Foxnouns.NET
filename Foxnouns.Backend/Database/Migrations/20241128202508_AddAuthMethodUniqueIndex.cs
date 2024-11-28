using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foxnouns.Backend.Database.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(DatabaseContext))]
    [Migration("20241128202508_AddAuthMethodUniqueIndex")]
    public partial class AddAuthMethodUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_auth_methods_auth_type_remote_id",
                table: "auth_methods",
                columns: new[] { "auth_type", "remote_id" },
                unique: true,
                filter: "fediverse_application_id IS NULL"
            );

            migrationBuilder.CreateIndex(
                name: "ix_auth_methods_auth_type_remote_id_fediverse_application_id",
                table: "auth_methods",
                columns: new[] { "auth_type", "remote_id", "fediverse_application_id" },
                unique: true,
                filter: "fediverse_application_id IS NOT NULL"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_auth_methods_auth_type_remote_id",
                table: "auth_methods"
            );

            migrationBuilder.DropIndex(
                name: "ix_auth_methods_auth_type_remote_id_fediverse_application_id",
                table: "auth_methods"
            );
        }
    }
}
