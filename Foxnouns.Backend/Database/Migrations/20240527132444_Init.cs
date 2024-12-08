using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace Foxnouns.Backend.Database.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(DatabaseContext))]
    [Migration("20240527132444_Init")]
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "fediverse_applications",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    domain = table.Column<string>(type: "text", nullable: false),
                    client_id = table.Column<string>(type: "text", nullable: false),
                    client_secret = table.Column<string>(type: "text", nullable: false),
                    instance_type = table.Column<int>(type: "integer", nullable: false),
                },
                constraints: table => table.PrimaryKey("pk_fediverse_applications", x => x.id)
            );

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    username = table.Column<string>(type: "text", nullable: false),
                    display_name = table.Column<string>(type: "text", nullable: true),
                    bio = table.Column<string>(type: "text", nullable: true),
                    member_title = table.Column<string>(type: "text", nullable: true),
                    avatar = table.Column<string>(type: "text", nullable: true),
                    links = table.Column<string[]>(type: "text[]", nullable: false),
                    role = table.Column<int>(type: "integer", nullable: false),
                    fields = table.Column<string>(type: "jsonb", nullable: false),
                    names = table.Column<string>(type: "jsonb", nullable: false),
                    pronouns = table.Column<string>(type: "jsonb", nullable: false),
                },
                constraints: table => table.PrimaryKey("pk_users", x => x.id)
            );

            migrationBuilder.CreateTable(
                name: "auth_methods",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    auth_type = table.Column<int>(type: "integer", nullable: false),
                    remote_id = table.Column<string>(type: "text", nullable: false),
                    remote_username = table.Column<string>(type: "text", nullable: true),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    fediverse_application_id = table.Column<long>(type: "bigint", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_auth_methods", x => x.id);
                    table.ForeignKey(
                        name: "fk_auth_methods_fediverse_applications_fediverse_application_id",
                        column: x => x.fediverse_application_id,
                        principalTable: "fediverse_applications",
                        principalColumn: "id"
                    );
                    table.ForeignKey(
                        name: "fk_auth_methods_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "members",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    display_name = table.Column<string>(type: "text", nullable: true),
                    bio = table.Column<string>(type: "text", nullable: true),
                    avatar = table.Column<string>(type: "text", nullable: true),
                    links = table.Column<string[]>(type: "text[]", nullable: false),
                    unlisted = table.Column<bool>(type: "boolean", nullable: false),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    fields = table.Column<string>(type: "jsonb", nullable: false),
                    names = table.Column<string>(type: "jsonb", nullable: false),
                    pronouns = table.Column<string>(type: "jsonb", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_members", x => x.id);
                    table.ForeignKey(
                        name: "fk_members_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "tokens",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    expires_at = table.Column<Instant>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    scopes = table.Column<string[]>(type: "text[]", nullable: false),
                    manually_expired = table.Column<bool>(type: "boolean", nullable: false),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tokens", x => x.id);
                    table.ForeignKey(
                        name: "fk_tokens_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "ix_auth_methods_fediverse_application_id",
                table: "auth_methods",
                column: "fediverse_application_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_auth_methods_user_id",
                table: "auth_methods",
                column: "user_id"
            );

            // EF Core doesn't support creating indexes on arbitrary expressions, so we have to create it manually.
            // Due to historical reasons (I made a mistake while writing the initial migration for the Go version)
            // only members have case-insensitive names.
            migrationBuilder.Sql(
                "CREATE UNIQUE INDEX ix_members_user_id_name ON members (user_id, lower(name))"
            );

            migrationBuilder.CreateIndex(
                name: "ix_tokens_user_id",
                table: "tokens",
                column: "user_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_users_username",
                table: "users",
                column: "username",
                unique: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "auth_methods");

            migrationBuilder.DropTable(name: "members");

            migrationBuilder.DropTable(name: "tokens");

            migrationBuilder.DropTable(name: "fediverse_applications");

            migrationBuilder.DropTable(name: "users");
        }
    }
}
