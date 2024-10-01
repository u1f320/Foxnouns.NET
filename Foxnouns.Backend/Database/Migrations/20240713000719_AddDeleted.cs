using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace Foxnouns.Backend.Database.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(DatabaseContext))]
    [Migration("20240713000719_AddDeleted")]
    public partial class AddDeleted : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "deleted",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false
            );

            migrationBuilder.AddColumn<Instant>(
                name: "deleted_at",
                table: "users",
                type: "timestamp with time zone",
                nullable: true
            );

            migrationBuilder.AddColumn<long>(
                name: "deleted_by",
                table: "users",
                type: "bigint",
                nullable: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "deleted", table: "users");

            migrationBuilder.DropColumn(name: "deleted_at", table: "users");

            migrationBuilder.DropColumn(name: "deleted_by", table: "users");
        }
    }
}
