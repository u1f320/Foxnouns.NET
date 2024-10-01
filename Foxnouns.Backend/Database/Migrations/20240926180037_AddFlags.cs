using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Foxnouns.Backend.Database.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(DatabaseContext))]
    [Migration("20240926180037_AddFlags")]
    public partial class AddFlags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "pride_flags",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    hash = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pride_flags", x => x.id);
                    table.ForeignKey(
                        name: "fk_pride_flags_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "member_flags",
                columns: table => new
                {
                    id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    member_id = table.Column<long>(type: "bigint", nullable: false),
                    pride_flag_id = table.Column<long>(type: "bigint", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_member_flags", x => x.id);
                    table.ForeignKey(
                        name: "fk_member_flags_members_member_id",
                        column: x => x.member_id,
                        principalTable: "members",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "fk_member_flags_pride_flags_pride_flag_id",
                        column: x => x.pride_flag_id,
                        principalTable: "pride_flags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "user_flags",
                columns: table => new
                {
                    id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    pride_flag_id = table.Column<long>(type: "bigint", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_flags", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_flags_pride_flags_pride_flag_id",
                        column: x => x.pride_flag_id,
                        principalTable: "pride_flags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "fk_user_flags_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "ix_member_flags_member_id",
                table: "member_flags",
                column: "member_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_member_flags_pride_flag_id",
                table: "member_flags",
                column: "pride_flag_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_pride_flags_user_id",
                table: "pride_flags",
                column: "user_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_user_flags_pride_flag_id",
                table: "user_flags",
                column: "pride_flag_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_user_flags_user_id",
                table: "user_flags",
                column: "user_id"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "member_flags");

            migrationBuilder.DropTable(name: "user_flags");

            migrationBuilder.DropTable(name: "pride_flags");
        }
    }
}
