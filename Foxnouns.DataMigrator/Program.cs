using System.Globalization;
using Foxnouns.Backend;
using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Extensions;
using Foxnouns.DataMigrator.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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

        Config config =
            new ConfigurationBuilder()
                .AddConfiguration()
                .Build()
                // Get the configuration as our config class
                .Get<Config>() ?? new Config();

        NpgsqlConnection conn = await GoDatabase.GetConnectionAsync();
        // just reuse the design time factory so we don't have to copy this
        DatabaseContext context = new DesignTimeDatabaseContextFactory().CreateDbContext(args);

        await context.Database.MigrateAsync();

        Log.Information("Migrating applications");
        Dictionary<int, Snowflake> appIds = await MigrateAppsAsync(conn, context);

        Log.Information("Migrating users");
        List<GoUser> users = await Queries.GetUsersAsync(conn);
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
                }
            );
        }

        return appIds;
    }
}
