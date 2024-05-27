using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace Foxnouns.Backend.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Serilog to this service collection. This method also initializes Serilog, so it should be called as early as possible, before any log calls.
    /// </summary>
    public static WebApplicationBuilder AddSerilog(this WebApplicationBuilder builder, LogEventLevel level)
    {
        var config = builder.Configuration.Get<Config>() ?? new();

        var logCfg = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .MinimumLevel.Is(level)
            // ASP.NET's built in request logs are extremely verbose, so we use Serilog's instead.
            // Serilog doesn't disable the built-in logs, so we do it here.
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.AspNetCore.Hosting", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore.Mvc", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore.Routing", LogEventLevel.Warning)
            .WriteTo.Console(theme: AnsiConsoleTheme.Code);

        if (config.SeqLogUrl != null)
        {
            logCfg.WriteTo.Seq(config.SeqLogUrl, restrictedToMinimumLevel: LogEventLevel.Verbose);
        }

        Log.Logger = logCfg.CreateLogger();

        // AddSerilog doesn't seem to add an ILogger to the service collection, so add that manually.
        builder.Services.AddSerilog().AddSingleton(Log.Logger);

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

    public static IConfigurationBuilder AddConfiguration(this IConfigurationBuilder builder)
    {
        var file = Environment.GetEnvironmentVariable("FOXNOUNS_CONFIG_FILE") ?? "config.ini";

        return builder
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddIniFile(file, optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();
    }
}
