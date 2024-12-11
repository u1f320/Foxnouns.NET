// Copyright (C) 2023-present sam/u1f320 (vulpine.solutions)
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
using Coravel;
using Coravel.Queuing.Interfaces;
using Foxnouns.Backend.Database;
using Foxnouns.Backend.Jobs;
using Foxnouns.Backend.Middleware;
using Foxnouns.Backend.Services;
using Foxnouns.Backend.Services.Auth;
using Microsoft.EntityFrameworkCore;
using Minio;
using NodaTime;
using Prometheus;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using FediverseAuthService = Foxnouns.Backend.Services.Auth.FediverseAuthService;
using IClock = NodaTime.IClock;

namespace Foxnouns.Backend.Extensions;

public static class WebApplicationExtensions
{
    /// <summary>
    /// Adds Serilog to this service collection. This method also initializes Serilog, so it should be called as early as possible, before any log calls.
    /// </summary>
    public static WebApplicationBuilder AddSerilog(this WebApplicationBuilder builder)
    {
        Config config = builder.Configuration.Get<Config>() ?? new Config();

        LoggerConfiguration logCfg = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .MinimumLevel.Is(config.Logging.LogEventLevel)
            // ASP.NET's built in request logs are extremely verbose, so we use Serilog's instead.
            // Serilog doesn't disable the built-in logs, so we do it here.
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override(
                "Microsoft.EntityFrameworkCore.Database.Command",
                config.Logging.LogQueries ? LogEventLevel.Information : LogEventLevel.Fatal
            )
            .MinimumLevel.Override("Microsoft.AspNetCore.Hosting", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore.Mvc", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore.Routing", LogEventLevel.Warning)
            .WriteTo.Console(theme: AnsiConsoleTheme.Sixteen);

        if (config.Logging.SeqLogUrl != null)
        {
            logCfg.WriteTo.Seq(config.Logging.SeqLogUrl, LogEventLevel.Verbose);
        }

        // AddSerilog doesn't seem to add an ILogger to the service collection, so add that manually.
        builder.Services.AddSerilog().AddSingleton(Log.Logger = logCfg.CreateLogger());

        return builder;
    }

    public static Config AddConfiguration(this WebApplicationBuilder builder)
    {
        builder.Configuration.Sources.Clear();
        builder.Configuration.AddConfiguration();

        Config config = builder.Configuration.Get<Config>() ?? new Config();
        builder.Services.AddSingleton(config);
        return config;
    }

    public static IConfigurationBuilder AddConfiguration(this IConfigurationBuilder builder)
    {
        string file = Environment.GetEnvironmentVariable("FOXNOUNS_CONFIG_FILE") ?? "config.ini";

        return builder
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appSettings.json", true)
            .AddIniFile(file, false, true)
            .AddEnvironmentVariables();
    }

    /// <summary>
    /// Adds required services to the WebApplicationBuilder.
    /// This should only add services that are not ASP.NET-related (i.e. no middleware).
    /// </summary>
    public static IServiceCollection AddServices(this WebApplicationBuilder builder, Config config)
    {
        builder.Host.ConfigureServices(
            (ctx, services) =>
            {
                services
                    .AddQueue()
                    .AddSmtpMailer(ctx.Configuration)
                    .AddFoxnounsDatabase(config)
                    .AddMetricServer(o => o.Port = config.Logging.MetricsPort)
                    .AddMinio(c =>
                        c.WithEndpoint(config.Storage.Endpoint)
                            .WithCredentials(config.Storage.AccessKey, config.Storage.SecretKey)
                            .Build()
                    )
                    .AddSingleton<MetricsCollectionService>()
                    .AddSingleton<IClock>(SystemClock.Instance)
                    .AddSnowflakeGenerator()
                    .AddSingleton<MailService>()
                    .AddSingleton<EmailRateLimiter>()
                    .AddScoped<UserRendererService>()
                    .AddScoped<MemberRendererService>()
                    .AddScoped<AuthService>()
                    .AddScoped<KeyCacheService>()
                    .AddScoped<RemoteAuthService>()
                    .AddScoped<FediverseAuthService>()
                    .AddScoped<ObjectStorageService>()
                    .AddTransient<DataCleanupService>()
                    // Background services
                    .AddHostedService<PeriodicTasksService>()
                    // Transient jobs
                    .AddTransient<MemberAvatarUpdateInvocable>()
                    .AddTransient<UserAvatarUpdateInvocable>()
                    .AddTransient<CreateFlagInvocable>()
                    .AddTransient<CreateDataExportInvocable>();

                if (!config.Logging.EnableMetrics)
                    services.AddHostedService<BackgroundMetricsCollectionService>();
            }
        );

        return builder.Services;
    }

    public static IServiceCollection AddCustomMiddleware(this IServiceCollection services) =>
        services
            .AddScoped<ErrorHandlerMiddleware>()
            .AddScoped<AuthenticationMiddleware>()
            .AddScoped<AuthorizationMiddleware>();

    public static IApplicationBuilder UseCustomMiddleware(this IApplicationBuilder app) =>
        app.UseMiddleware<ErrorHandlerMiddleware>()
            .UseMiddleware<AuthenticationMiddleware>()
            .UseMiddleware<AuthorizationMiddleware>();

    public static async Task Initialize(this WebApplication app, string[] args)
    {
        // Read version information from .version in the repository root
        await BuildInfo.ReadBuildInfo();

        app.Services.ConfigureQueue()
            .LogQueuedTaskProgress(app.Services.GetRequiredService<ILogger<IQueue>>());

        await using AsyncServiceScope scope = app.Services.CreateAsyncScope();

        // The types of these variables are obvious from the methods being called to create them
        // ReSharper disable SuggestVarOrType_SimpleTypes
        var logger = scope
            .ServiceProvider.GetRequiredService<ILogger>()
            .ForContext<WebApplication>();
        var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        // ReSharper restore SuggestVarOrType_SimpleTypes

        logger.Information(
            "Starting Foxnouns.NET {Version} ({Hash})",
            BuildInfo.Version,
            BuildInfo.Hash
        );

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

            if (!args.Contains("--migrate-and-start"))
                Environment.Exit(0);
        }
        else if (pendingMigrations.Count > 0)
        {
            logger.Fatal(
                "There are {Count} pending migrations, run server with --migrate or --migrate-and-start to run migrations.",
                pendingMigrations.Count
            );
            Environment.Exit(1);
        }

        logger.Information("Initializing frontend OAuth application");
        _ = await db.GetFrontendApplicationAsync();
    }
}
