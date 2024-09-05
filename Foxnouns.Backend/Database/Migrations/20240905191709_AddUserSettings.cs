using Foxnouns.Backend.Database.Models;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foxnouns.Backend.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddUserSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<UserSettings>(
                name: "settings",
                table: "users",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'{}'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "settings",
                table: "users");
        }
    }
}
