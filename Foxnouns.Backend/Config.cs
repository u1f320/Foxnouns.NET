using Serilog.Events;

namespace Foxnouns.Backend;

public class Config
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 3000;
    public string BaseUrl { get; set; } = null!;

    public string Address => $"http://{Host}:{Port}";

    public string? SeqLogUrl { get; set; }
    public LogEventLevel LogEventLevel { get; set; } = LogEventLevel.Debug;

    public DatabaseConfig Database { get; set; } = new();

    public class DatabaseConfig
    {
        public string Url { get; set; } = string.Empty;
        public int? Timeout { get; set; }
        public int? MaxPoolSize { get; set; }
    }
}