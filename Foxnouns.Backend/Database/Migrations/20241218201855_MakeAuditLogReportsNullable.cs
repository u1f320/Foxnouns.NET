using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foxnouns.Backend.Database.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(DatabaseContext))]
    [Migration("20241218201855_MakeAuditLogReportsNullable")]
    public partial class MakeAuditLogReportsNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_audit_log_reports_report_id",
                table: "audit_log"
            );

            migrationBuilder.DropIndex(name: "ix_audit_log_report_id", table: "audit_log");

            migrationBuilder.CreateIndex(
                name: "ix_audit_log_report_id",
                table: "audit_log",
                column: "report_id",
                unique: true
            );

            migrationBuilder.AddForeignKey(
                name: "fk_audit_log_reports_report_id",
                table: "audit_log",
                column: "report_id",
                principalTable: "reports",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_audit_log_reports_report_id",
                table: "audit_log"
            );

            migrationBuilder.DropIndex(name: "ix_audit_log_report_id", table: "audit_log");

            migrationBuilder.CreateIndex(
                name: "ix_audit_log_report_id",
                table: "audit_log",
                column: "report_id"
            );

            migrationBuilder.AddForeignKey(
                name: "fk_audit_log_reports_report_id",
                table: "audit_log",
                column: "report_id",
                principalTable: "reports",
                principalColumn: "id"
            );
        }
    }
}
