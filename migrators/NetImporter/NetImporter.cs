using Foxnouns.Backend;
using Foxnouns.Backend.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NodaTime;
using NodaTime.Serialization.JsonNet;
using Serilog;
using Serilog.Events;

namespace NetImporter;

internal static class NetImporter
{
    public static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override(
                "Microsoft.EntityFrameworkCore.Database.Command",
                LogEventLevel.Information
            )
            .WriteTo.Console()
            .CreateLogger();

        switch (args.Length)
        {
            case < 2:
                Console.WriteLine("Not enough arguments. Usage: <type> <file>");
                return;
            case > 2:
                Console.WriteLine("Too many arguments. Usage: <type> <file>");
                return;
        }

        switch (args[0].ToLowerInvariant())
        {
            case "users":
                await Users.ImportUsers(args[1]);
                break;
            default:
                Console.WriteLine("Invalid type. Valid types are: users");
                break;
        }
    }

    internal static async Task<DatabaseContext> GetContextAsync()
    {
        var connString = Environment.GetEnvironmentVariable("DATABASE");
        if (connString == null)
            throw new Exception("$DATABASE not set, must be an ADO.NET connection string");

        var loggerFactory = new LoggerFactory().AddSerilog(Log.Logger);
        var config = new Config { Database = new Config.DatabaseConfig { Url = connString } };

        var dataSource = DatabaseContext.BuildDataSource(config);
        var options = DatabaseContext
            .BuildOptions(new DbContextOptionsBuilder(), dataSource, loggerFactory)
            .Options;
        var db = new DatabaseContext(options);

        if ((await db.Database.GetPendingMigrationsAsync()).Any())
        {
            Log.Fatal("Database needs to be migrated first");
        }

        return db;
    }

    private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
    {
        ContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new SnakeCaseNamingStrategy(),
        },
    }.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);

    internal static Input<T> ReadFromFile<T>(string path)
    {
        var data = File.ReadAllText(path);
        return JsonConvert.DeserializeObject<Input<T>>(data, Settings)
            ?? throw new Exception("Invalid input file");
    }
}

internal record Input<T>(List<T> Output, List<string> Skipped);
