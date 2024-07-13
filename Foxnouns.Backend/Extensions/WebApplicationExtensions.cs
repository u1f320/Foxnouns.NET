using App.Metrics;
using App.Metrics.AspNetCore;
using App.Metrics.Formatters.Prometheus;
using Foxnouns.Backend.Database;
using Foxnouns.Backend.Jobs;
using Foxnouns.Backend.Middleware;
using Foxnouns.Backend.Services;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Serilog;
using Serilog.Events;
using IClock = NodaTime.IClock;

namespace Foxnouns.Backend.Extensions;

public static class WebApplicationExtensions
{
    /// <summary>
    /// Adds Serilog to this service collection. This method also initializes Serilog, so it should be called as early as possible, before any log calls.
    /// </summary>
    public static WebApplicationBuilder AddSerilog(this WebApplicationBuilder builder)
    {
        var config = builder.Configuration.Get<Config>() ?? new();

        var logCfg = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .MinimumLevel.Is(config.Logging.LogEventLevel)
            // ASP.NET's built in request logs are extremely verbose, so we use Serilog's instead.
            // Serilog doesn't disable the built-in logs, so we do it here.
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command",
                config.Logging.LogQueries ? LogEventLevel.Information : LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore.Hosting", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore.Mvc", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore.Routing", LogEventLevel.Warning)
            .MinimumLevel.Override("Hangfire", LogEventLevel.Information)
            .WriteTo.Console();

        if (config.Logging.SeqLogUrl != null)
        {
            logCfg.WriteTo.Seq(config.Logging.SeqLogUrl, restrictedToMinimumLevel: LogEventLevel.Verbose);
        }

        // AddSerilog doesn't seem to add an ILogger to the service collection, so add that manually.
        builder.Services.AddSerilog().AddSingleton(Log.Logger = logCfg.CreateLogger());

        return builder;
    }

    public static Config AddConfiguration(this WebApplicationBuilder builder)
    {
        builder.Configuration.Sources.Clear();
        builder.Configuration.AddConfiguration();

        var config = builder.Configuration.Get<Config>() ?? new();
        builder.Services.AddSingleton(config);
        return config;
    }

    public static WebApplicationBuilder AddMetrics(this WebApplicationBuilder builder)
    {
        var config = builder.Configuration.Get<Config>() ?? new();
        var metrics = AppMetrics.CreateDefaultBuilder()
            .OutputMetrics.AsPrometheusPlainText()
            .Build();

        builder.Services.AddSingleton(metrics);
        builder.Services.AddSingleton<IMetrics>(metrics);

        builder.WebHost
            .ConfigureMetrics(metrics)
            .UseMetrics(opts =>
            {
                opts.EndpointOptions = options =>
                {
                    // Metrics must listen on a separate port for security reasons. If no metrics port is set, disable the endpoint entirely.
                    options.MetricsEndpointEnabled = config.Logging.MetricsPort != null;
                    options.EnvironmentInfoEndpointEnabled = config.Logging.MetricsPort != null;
                    options.MetricsTextEndpointEnabled = false;
                    options.MetricsEndpointOutputFormatter = metrics.OutputMetricsFormatters
                        .OfType<MetricsPrometheusTextOutputFormatter>().First();
                };
            })
            .UseMetricsWebTracking()
            .ConfigureAppMetricsHostingConfiguration(opts => { opts.AllEndpointsPort = config.Logging.MetricsPort; });

        return builder;
    }

    public static IConfigurationBuilder AddConfiguration(this IConfigurationBuilder builder)
    {
        var file = Environment.GetEnvironmentVariable("FOXNOUNS_CONFIG_FILE") ?? "config.ini";

        return builder
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddIniFile(file, optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();
    }

    public static IServiceCollection AddCustomServices(this IServiceCollection services) => services
        .AddSingleton<IClock>(SystemClock.Instance)
        .AddSnowflakeGenerator()
        .AddScoped<UserRendererService>()
        .AddScoped<MemberRendererService>()
        .AddScoped<AuthService>()
        .AddScoped<KeyCacheService>()
        .AddScoped<RemoteAuthService>()
        // Background job classes
        .AddTransient<AvatarUpdateJob>();

    public static IServiceCollection AddCustomMiddleware(this IServiceCollection services) => services
        .AddScoped<ErrorHandlerMiddleware>()
        .AddScoped<AuthenticationMiddleware>()
        .AddScoped<AuthorizationMiddleware>();

    public static IApplicationBuilder UseCustomMiddleware(this IApplicationBuilder app) => app
        .UseMiddleware<ErrorHandlerMiddleware>()
        .UseMiddleware<AuthenticationMiddleware>()
        .UseMiddleware<AuthorizationMiddleware>();

    public static async Task Initialize(this WebApplication app, string[] args)
    {
        await BuildInfo.ReadBuildInfo();

        await using var scope = app.Services.CreateAsyncScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger>().ForContext<WebApplication>();
        var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

        logger.Information("Starting Foxnouns.NET {Version} ({Hash})", BuildInfo.Version, BuildInfo.Hash);

        var pendingMigrations = (await db.Database.GetPendingMigrationsAsync()).ToList();
        if (args.Contains("--migrate") || args.Contains("--migrate-and-start"))
        {
            if (pendingMigrations.Count == 0)
            {
                logger.Information("Migrations requested but no migrations are required");
            }
            else
            {
                logger.Information("Migrating database to the latest version");
                await db.Database.MigrateAsync();
                logger.Information("Successfully migrated database");
            }

            if (!args.Contains("--migrate-and-start")) Environment.Exit(0);
        }
        else if (pendingMigrations.Count > 0)
        {
            logger.Fatal(
                "There are {Count} pending migrations, run server with --migrate or --migrate-and-start to run migrations.",
                pendingMigrations.Count);
            Environment.Exit(1);
        }

        logger.Information("Initializing frontend OAuth application");
        _ = await db.GetFrontendApplicationAsync();
    }
}