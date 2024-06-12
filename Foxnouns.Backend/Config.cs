using Serilog.Events;

namespace Foxnouns.Backend;

public class Config
{
    public string Host { get; init; } = "localhost";
    public int Port { get; init; } = 3000;
    public string BaseUrl { get; init; } = null!;

    public string Address => $"http://{Host}:{Port}";

    public string? SeqLogUrl { get; init; }
    public LogEventLevel LogEventLevel { get; init; } = LogEventLevel.Debug;

    public DatabaseConfig Database { get; init; } = new();
    public DiscordAuthConfig DiscordAuth { get; init; } = new();

    public class DatabaseConfig
    {
        public string Url { get; init; } = string.Empty;
        public int? Timeout { get; init; }
        public int? MaxPoolSize { get; init; }
    }

    public class DiscordAuthConfig
    {
        public string? ClientId { get; init; }
        public string? ClientSecret { get; init; }
    }
}