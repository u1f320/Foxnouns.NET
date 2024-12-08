using Npgsql;
using Serilog;

namespace Foxnouns.Backend.Database;

public static class DatabaseServiceExtensions
{
    public static IServiceCollection AddFoxnounsDatabase(
        this IServiceCollection serviceCollection,
        Config config
    )
    {
        NpgsqlDataSource dataSource = DatabaseContext.BuildDataSource(config);
        ILoggerFactory loggerFactory = new LoggerFactory().AddSerilog(dispose: false);

        serviceCollection.AddDbContext<DatabaseContext>(options =>
            DatabaseContext.BuildOptions(options, dataSource, loggerFactory)
        );

        return serviceCollection;
    }
}
