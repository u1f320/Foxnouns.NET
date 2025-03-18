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

// ReSharper disable UnusedAutoPropertyAccessor.Global
using Serilog.Events;

namespace Foxnouns.Backend;

public class Config
{
    public string Host { get; init; } = "localhost";
    public int Port { get; init; } = 3000;
    public string BaseUrl { get; init; } = null!;
    public string MediaBaseUrl { get; init; } = null!;

    public string Address => $"http://{Host}:{Port}";

    public LoggingConfig Logging { get; init; } = new();
    public DatabaseConfig Database { get; init; } = new();
    public StorageConfig Storage { get; init; } = new();
    public LimitsConfig Limits { get; init; } = new();
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
        public string Redis { get; init; } = string.Empty;
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

    public class LimitsConfig
    {
        public int MaxMemberCount { get; init; } = 1000;

        public int MaxFields { get; init; } = 25;
        public int MaxFieldNameLength { get; init; } = 100;
        public int MaxFieldEntryTextLength { get; init; } = 100;
        public int MaxFieldEntries { get; init; } = 100;

        public int MaxUsernameLength { get; init; } = 40;
        public int MaxMemberNameLength { get; init; } = 100;
        public int MaxDisplayNameLength { get; init; } = 100;
        public int MaxLinks { get; init; } = 25;
        public int MaxLinkLength { get; init; } = 256;
        public int MaxBioLength { get; init; } = 1024;
        public int MaxAvatarLength { get; init; } = 1_500_000;
    }
}
