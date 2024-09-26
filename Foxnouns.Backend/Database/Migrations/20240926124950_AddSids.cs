using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using NodaTime;

#nullable disable

namespace Foxnouns.Backend.Database.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(DatabaseContext))]
    [Migration("20240926124950_AddSids")]
    public partial class AddSids : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "sid",
                table: "users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Instant>(
                name: "last_sid_reroll",
                table: "users",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now() - '1 hour'::interval");

            migrationBuilder.AddColumn<string>(
                name: "sid",
                table: "members",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_sid",
                table: "users",
                column: "sid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_members_sid",
                table: "members",
                column: "sid",
                unique: true);

            migrationBuilder.Sql(@"create function generate_sid(len int) returns text as $$
    select string_agg(substr('abcdefghijklmnopqrstuvwxyz', ceil(random() * 26)::integer, 1), '') from generate_series(1, len)
$$ language sql volatile;
");
            migrationBuilder.Sql(@"create function find_free_user_sid() returns text as $$
declare new_sid text;
begin
    loop
        new_sid := generate_sid(5);
        if not exists (select 1 from users where sid = new_sid) then return new_sid; end if;
    end loop;
end
$$ language plpgsql volatile;
");
            migrationBuilder.Sql(@"create function find_free_member_sid() returns text as $$
declare new_sid text;
begin
    loop
        new_sid := generate_sid(6);
        if not exists (select 1 from members where sid = new_sid) then return new_sid; end if;
    end loop;
end
$$ language plpgsql volatile;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("drop function find_free_member_sid;");
            migrationBuilder.Sql("drop function find_free_user_sid;");
            migrationBuilder.Sql("drop function generate_sid;");

            migrationBuilder.DropIndex(
                name: "ix_users_sid",
                table: "users");

            migrationBuilder.DropIndex(
                name: "ix_members_sid",
                table: "members");

            migrationBuilder.DropColumn(
                name: "sid",
                table: "users");

            migrationBuilder.DropColumn(
                name: "last_sid_reroll",
                table: "users");

            migrationBuilder.DropColumn(
                name: "sid",
                table: "members");
        }
    }
}
