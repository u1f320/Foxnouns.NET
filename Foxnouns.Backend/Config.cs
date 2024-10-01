using Serilog.Events;

namespace Foxnouns.Backend;

public class Config
{
    public string Host { get; init; } = "localhost";
    public int Port { get; init; } = 3000;
    public string BaseUrl { get; set; } = null!;
    public string MediaBaseUrl { get; set; } = null!;

    public string Address => $"http://{Host}:{Port}";
    public string MetricsAddress => $"http://{Host}:{Logging.MetricsPort}";

    public LoggingConfig Logging { get; init; } = new();
    public DatabaseConfig Database { get; init; } = new();
    public StorageConfig Storage { get; init; } = new();
    public EmailAuthConfig EmailAuth { get; init; } = new();
    public DiscordAuthConfig DiscordAuth { get; init; } = new();
    public GoogleAuthConfig GoogleAuth { get; init; } = new();
    public TumblrAuthConfig TumblrAuth { get; init; } = new();

    public class LoggingConfig
    {
        public LogEventLevel LogEventLevel { get; init; } = LogEventLevel.Debug;
        public string? SeqLogUrl { get; init; }
        public string? SentryUrl { get; init; }
        public bool SentryTracing { get; init; } = false;
        public double SentryTracesSampleRate { get; init; } = 0.0;
        public bool LogQueries { get; init; } = false;
        public bool EnableMetrics { get; init; } = false;
        public ushort MetricsPort { get; init; } = 5001;
    }

    public class DatabaseConfig
    {
        public string Url { get; init; } = string.Empty;
        public bool? EnablePooling { get; init; }
        public int? Timeout { get; init; }
        public int? MaxPoolSize { get; init; }
    }

    public class StorageConfig
    {
        public string Endpoint { get; init; } = string.Empty;
        public string AccessKey { get; init; } = string.Empty;
        public string SecretKey { get; init; } = string.Empty;
        public string Bucket { get; init; } = string.Empty;
    }

    public class EmailAuthConfig
    {
        public bool Enabled => From != null;
        public string? From { get; init; }
    }

    public class DiscordAuthConfig
    {
        public bool Enabled => ClientId != null && ClientSecret != null;

        public string? ClientId { get; init; }
        public string? ClientSecret { get; init; }
    }

    public class GoogleAuthConfig
    {
        public bool Enabled => ClientId != null && ClientSecret != null;

        public string? ClientId { get; init; }
        public string? ClientSecret { get; init; }
    }

    public class TumblrAuthConfig
    {
        public bool Enabled => ClientId != null && ClientSecret != null;

        public string? ClientId { get; init; }
        public string? ClientSecret { get; init; }
    }
}
