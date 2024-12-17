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
using System.Diagnostics.CodeAnalysis;
using EntityFramework.Exceptions.PostgreSQL;
using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Npgsql;

namespace Foxnouns.Backend.Database;

public class DatabaseContext(DbContextOptions options) : DbContext(options)
{
    private static string GenerateConnectionString(Config.DatabaseConfig config) =>
        new NpgsqlConnectionStringBuilder(config.Url)
        {
            Pooling = config.EnablePooling ?? true,
            Timeout = config.Timeout ?? 5,
            MaxPoolSize = config.MaxPoolSize ?? 50,
            MinPoolSize = 0,
            ConnectionPruningInterval = 10,
            ConnectionIdleLifetime = 10,
        }.ConnectionString;

    public static NpgsqlDataSource BuildDataSource(Config config)
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(
            GenerateConnectionString(config.Database)
        );
        dataSourceBuilder.UseNodaTime();
        dataSourceBuilder.UseJsonNet();
        return dataSourceBuilder.Build();
    }

    public static DbContextOptionsBuilder BuildOptions(
        DbContextOptionsBuilder options,
        NpgsqlDataSource dataSource,
        ILoggerFactory? loggerFactory
    ) =>
        options
            .ConfigureWarnings(c => c.Ignore(CoreEventId.SaveChangesFailed))
            .UseNpgsql(dataSource, o => o.UseNodaTime())
            .UseLoggerFactory(loggerFactory)
            .UseSnakeCaseNamingConvention()
            .UseExceptionProcessor();

    public DbSet<User> Users { get; init; } = null!;
    public DbSet<Member> Members { get; init; } = null!;
    public DbSet<AuthMethod> AuthMethods { get; init; } = null!;
    public DbSet<FediverseApplication> FediverseApplications { get; init; } = null!;
    public DbSet<Token> Tokens { get; init; } = null!;
    public DbSet<Application> Applications { get; init; } = null!;
    public DbSet<TemporaryKey> TemporaryKeys { get; init; } = null!;
    public DbSet<DataExport> DataExports { get; init; } = null!;

    public DbSet<PrideFlag> PrideFlags { get; init; } = null!;
    public DbSet<UserFlag> UserFlags { get; init; } = null!;
    public DbSet<MemberFlag> MemberFlags { get; init; } = null!;

    public DbSet<Report> Reports { get; init; } = null!;
    public DbSet<AuditLogEntry> AuditLog { get; init; } = null!;
    public DbSet<Notification> Notifications { get; init; } = null!;

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Snowflakes are stored as longs
        configurationBuilder.Properties<Snowflake>().HaveConversion<Snowflake.ValueConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();
        modelBuilder.Entity<User>().HasIndex(u => u.Sid).IsUnique();
        modelBuilder.Entity<Member>().HasIndex(m => new { m.UserId, m.Name }).IsUnique();
        modelBuilder.Entity<Member>().HasIndex(m => m.Sid).IsUnique();
        modelBuilder.Entity<TemporaryKey>().HasIndex(k => k.Key).IsUnique();
        modelBuilder.Entity<DataExport>().HasIndex(d => d.Filename).IsUnique();

        // Two indexes on auth_methods, one for fediverse auth and one for all other types.
        modelBuilder
            .Entity<AuthMethod>()
            .HasIndex(m => new
            {
                m.AuthType,
                m.RemoteId,
                m.FediverseApplicationId,
            })
            .HasFilter("fediverse_application_id IS NOT NULL")
            .IsUnique();

        modelBuilder
            .Entity<AuthMethod>()
            .HasIndex(m => new { m.AuthType, m.RemoteId })
            .HasFilter("fediverse_application_id IS NULL")
            .IsUnique();

        modelBuilder.Entity<User>().Property(u => u.Sid).HasDefaultValueSql("find_free_user_sid()");
        modelBuilder.Entity<User>().Property(u => u.Fields).HasColumnType("jsonb");
        modelBuilder.Entity<User>().Property(u => u.Names).HasColumnType("jsonb");
        modelBuilder.Entity<User>().Property(u => u.Pronouns).HasColumnType("jsonb");
        modelBuilder.Entity<User>().Property(u => u.CustomPreferences).HasColumnType("jsonb");
        modelBuilder.Entity<User>().Property(u => u.Settings).HasColumnType("jsonb");

        modelBuilder
            .Entity<Member>()
            .Property(m => m.Sid)
            .HasDefaultValueSql("find_free_member_sid()");
        modelBuilder.Entity<Member>().Property(m => m.Fields).HasColumnType("jsonb");
        modelBuilder.Entity<Member>().Property(m => m.Names).HasColumnType("jsonb");
        modelBuilder.Entity<Member>().Property(m => m.Pronouns).HasColumnType("jsonb");

        modelBuilder.Entity<UserFlag>().Navigation(f => f.PrideFlag).AutoInclude();
        modelBuilder.Entity<MemberFlag>().Navigation(f => f.PrideFlag).AutoInclude();

        modelBuilder
            .HasDbFunction(typeof(DatabaseContext).GetMethod(nameof(FindFreeUserSid))!)
            .HasName("find_free_user_sid");

        modelBuilder
            .HasDbFunction(typeof(DatabaseContext).GetMethod(nameof(FindFreeMemberSid))!)
            .HasName("find_free_member_sid");
    }

    /// <summary>
    /// Dummy method that calls <c>find_free_user_sid()</c> when used in an EF Core query.
    /// </summary>
    public string FindFreeUserSid() => throw new NotSupportedException();

    /// <summary>
    /// Dummy method that calls <c>find_free_member_sid()</c> when used in an EF Core query.
    /// </summary>
    public string FindFreeMemberSid() => throw new NotSupportedException();
}

[SuppressMessage(
    "ReSharper",
    "UnusedType.Global",
    Justification = "Used by EF Core's migration generator"
)]
public class DesignTimeDatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
{
    public DatabaseContext CreateDbContext(string[] args)
    {
        // Read the configuration file
        Config config =
            new ConfigurationBuilder()
                .AddConfiguration()
                .Build()
                // Get the configuration as our config class
                .Get<Config>() ?? new Config();

        NpgsqlDataSource dataSource = DatabaseContext.BuildDataSource(config);

        DbContextOptions options = DatabaseContext
            .BuildOptions(new DbContextOptionsBuilder(), dataSource, null)
            .Options;

        return new DatabaseContext(options);
    }
}
