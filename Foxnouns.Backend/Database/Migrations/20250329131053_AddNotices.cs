using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace Foxnouns.Backend.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddNotices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "notices",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    message = table.Column<string>(type: "text", nullable: false),
                    start_time = table.Column<Instant>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    end_time = table.Column<Instant>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    author_id = table.Column<long>(type: "bigint", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notices", x => x.id);
                    table.ForeignKey(
                        name: "fk_notices_users_author_id",
                        column: x => x.author_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "ix_notices_author_id",
                table: "notices",
                column: "author_id"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "notices");
        }
    }
}
