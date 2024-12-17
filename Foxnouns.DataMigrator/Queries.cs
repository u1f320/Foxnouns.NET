using Coravel.Mailer.Mail.Helpers;
using Dapper;
using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;
using Foxnouns.DataMigrator.Models;
using NodaTime.Extensions;
using Npgsql;

namespace Foxnouns.DataMigrator;

public static class Queries
{
    public static async Task<List<GoFediverseApp>> GetFediverseAppsAsync(NpgsqlConnection conn) =>
        (await conn.QueryAsync<GoFediverseApp>("select * from fediverse_apps")).ToList();

    public static async Task<List<GoUser>> GetUsersAsync(NpgsqlConnection conn, Snowflake minId) =>
        (
            await conn.QueryAsync<GoUser>(
                "select * from users where snowflake_id > @Id order by snowflake_id limit 1000",
                new { Id = minId.Value }
            )
        ).ToList();

    public static async Task<List<GoUserField>> GetUserFieldsAsync(NpgsqlConnection conn) =>
        (await conn.QueryAsync<GoUserField>("select * from user_fields order by id")).ToList();

    public static async Task<List<GoMemberField>> GetMemberFieldsAsync(NpgsqlConnection conn) =>
        (await conn.QueryAsync<GoMemberField>("select * from member_fields order by id")).ToList();

    public static async Task<List<GoProfileFlag>> GetUserProfileFlagsAsync(NpgsqlConnection conn) =>
        (await conn.QueryAsync<GoProfileFlag>("select * from user_flags order by id")).ToList();

    public static async Task<List<GoProfileFlag>> GetMemberProfileFlagsAsync(
        NpgsqlConnection conn
    ) => (await conn.QueryAsync<GoProfileFlag>("select * from member_flags order by id")).ToList();

    public static async Task<List<GoPrideFlag>> GetUserFlagsAsync(NpgsqlConnection conn) =>
        (await conn.QueryAsync<GoPrideFlag>("select * from pride_flags order by id")).ToList();
}
