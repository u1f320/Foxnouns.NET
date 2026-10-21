using System.Globalization;
using Foxnouns.Backend;
using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Extensions;
using Foxnouns.DataMigrator.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Npgsql;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace Foxnouns.DataMigrator;

internal class Program
{
    public static async Task Main(string[] args)
    {
        // Create logger and get configuration
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console(theme: AnsiConsoleTheme.Sixteen)
            .CreateLogger();

        var minUserId = new Snowflake(0);
        if (args.Length > 0)
            minUserId = ulong.Parse(args[0]);

        Log.Information("Starting migration from user ID {MinUserId}", minUserId);

        Config config =
            new ConfigurationBuilder()
                .AddConfiguration()
                .Build()
                // Get the configuration as our config class
                .Get<Config>()
            ?? new Config();

        NpgsqlConnection conn = await GoDatabase.GetConnectionAsync();
        // just reuse the design time factory so we don't have to copy this
        DatabaseContext context = new DesignTimeDatabaseContextFactory().CreateDbContext(args);

        await context.Database.MigrateAsync();

        Dictionary<int, Snowflake> appIds;
        if (minUserId == new Snowflake(0))
        {
            Log.Information("Migrating applications");
            appIds = await MigrateAppsAsync(conn, context);

            string appJson = JsonConvert.SerializeObject(appIds);
            await File.WriteAllTextAsync("apps.json", appJson);
        }
        else
        {
            Log.Information(
                "Not the first migration, reading application IDs from {Filename}",
                "apps.json"
            );

            string appJson = await File.ReadAllTextAsync("apps.json");
            appIds =
                JsonConvert.DeserializeObject<Dictionary<int, Snowflake>>(appJson)
                ?? throw new Exception("invalid apps.json file");
        }

        Log.Information("Migrating users");
        List<GoUser> users = await Queries.GetUsersAsync(conn, minUserId);
        List<GoUserField> userFields = await Queries.GetUserFieldsAsync(conn);
        List<GoMemberField> memberFields = await Queries.GetMemberFieldsAsync(conn);
        List<GoPrideFlag> prideFlags = await Queries.GetUserFlagsAsync(conn);
        List<GoProfileFlag> userFlags = await Queries.GetUserProfileFlagsAsync(conn);
        List<GoProfileFlag> memberFlags = await Queries.GetMemberProfileFlagsAsync(conn);
        Log.Information("Migrating {Count} users", users.Count);
        foreach ((GoUser user, int i) in users.Select((user, i) => (user, i)))
        {
            Log.Debug(
                "Migrating user #{Index}/{Count}: {Id}/{SnowflakeId}",
                i,
                users.Count,
                user.Id,
                user.SnowflakeId
            );
            await new UserMigrator(
                conn,
                context,
                user,
                appIds,
                userFields,
                memberFields,
                prideFlags,
                userFlags,
                memberFlags
            ).MigrateAsync();
        }

        await context.SaveChangesAsync();
        Log.Information("Migration complete!");
        Log.Information(
            "Migrated {Count} users, last user was {UserId}. Complete? {Complete}",
            users.Count,
            users.Last().SnowflakeId,
            users.Count != 1000
        );
    }

    private static async Task<Dictionary<int, Snowflake>> MigrateAppsAsync(
        NpgsqlConnection conn,
        DatabaseContext context
    )
    {
        List<GoFediverseApp> goApps = await Queries.GetFediverseAppsAsync(conn);
        var appIds = new Dictionary<int, Snowflake>();
        foreach (GoFediverseApp app in goApps)
        {
            Log.Debug("Migrating application for {Domain}", app.Instance);
            Snowflake id = SnowflakeGenerator.Instance.GenerateSnowflake();
            appIds[app.Id] = id;
            context.FediverseApplications.Add(
                new FediverseApplication
                {
                    Id = id,
                    Domain = app.Instance.ToLower(CultureInfo.InvariantCulture),
                    ClientId = app.ClientId,
                    ClientSecret = app.ClientSecret,
                    InstanceType = app.TypeToEnum(),
                    ForceRefresh = true,
                }
            );
        }

        return appIds;
    }
}
