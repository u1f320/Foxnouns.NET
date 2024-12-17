using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace Foxnouns.Backend.Database.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(DatabaseContext))]
    [Migration("20241217010207_AddReports")]
    public partial class AddReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase().Annotation("Npgsql:PostgresExtension:hstore", ",,");

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    target_id = table.Column<long>(type: "bigint", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    message = table.Column<string>(type: "text", nullable: true),
                    localization_key = table.Column<string>(type: "text", nullable: true),
                    localization_params = table.Column<Dictionary<string, string>>(
                        type: "hstore",
                        nullable: false
                    ),
                    acknowledged_at = table.Column<Instant>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notifications", x => x.id);
                    table.ForeignKey(
                        name: "fk_notifications_users_target_id",
                        column: x => x.target_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "reports",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    reporter_id = table.Column<long>(type: "bigint", nullable: false),
                    target_user_id = table.Column<long>(type: "bigint", nullable: false),
                    target_member_id = table.Column<long>(type: "bigint", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    reason = table.Column<int>(type: "integer", nullable: false),
                    target_type = table.Column<int>(type: "integer", nullable: false),
                    target_snapshot = table.Column<string>(type: "text", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reports", x => x.id);
                    table.ForeignKey(
                        name: "fk_reports_members_target_member_id",
                        column: x => x.target_member_id,
                        principalTable: "members",
                        principalColumn: "id"
                    );
                    table.ForeignKey(
                        name: "fk_reports_users_reporter_id",
                        column: x => x.reporter_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "fk_reports_users_target_user_id",
                        column: x => x.target_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "audit_log",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    moderator_id = table.Column<long>(type: "bigint", nullable: false),
                    moderator_username = table.Column<string>(type: "text", nullable: false),
                    target_user_id = table.Column<long>(type: "bigint", nullable: true),
                    target_username = table.Column<string>(type: "text", nullable: true),
                    target_member_id = table.Column<long>(type: "bigint", nullable: true),
                    target_member_name = table.Column<string>(type: "text", nullable: true),
                    report_id = table.Column<long>(type: "bigint", nullable: true),
                    type = table.Column<int>(type: "integer", nullable: false),
                    reason = table.Column<string>(type: "text", nullable: true),
                    cleared_fields = table.Column<string[]>(type: "text[]", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_log", x => x.id);
                    table.ForeignKey(
                        name: "fk_audit_log_reports_report_id",
                        column: x => x.report_id,
                        principalTable: "reports",
                        principalColumn: "id"
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "ix_audit_log_report_id",
                table: "audit_log",
                column: "report_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_notifications_target_id",
                table: "notifications",
                column: "target_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_reports_reporter_id",
                table: "reports",
                column: "reporter_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_reports_target_member_id",
                table: "reports",
                column: "target_member_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_reports_target_user_id",
                table: "reports",
                column: "target_user_id"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "audit_log");

            migrationBuilder.DropTable(name: "notifications");

            migrationBuilder.DropTable(name: "reports");

            migrationBuilder.AlterDatabase().OldAnnotation("Npgsql:PostgresExtension:hstore", ",,");
        }
    }
}
