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
    private static string GenerateConnectionString(Config.DatabaseConfig config)
    {
        return new NpgsqlConnectionStringBuilder(config.Url)
        {
            Pooling = config.EnablePooling ?? true,
            Timeout = config.Timeout ?? 5,
            MaxPoolSize = config.MaxPoolSize ?? 50,
            MinPoolSize = 0,
            ConnectionPruningInterval = 10,
            ConnectionIdleLifetime = 10,
        }.ConnectionString;
    }

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

    public DbSet<User> Users { get; init; }
    public DbSet<Member> Members { get; init; }
    public DbSet<AuthMethod> AuthMethods { get; init; }
    public DbSet<FediverseApplication> FediverseApplications { get; init; }
    public DbSet<Token> Tokens { get; init; }
    public DbSet<Application> Applications { get; init; }
    public DbSet<TemporaryKey> TemporaryKeys { get; init; }
    public DbSet<DataExport> DataExports { get; init; }

    public DbSet<PrideFlag> PrideFlags { get; init; }
    public DbSet<UserFlag> UserFlags { get; init; }
    public DbSet<MemberFlag> MemberFlags { get; init; }

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
        modelBuilder.Entity<DataExport>().HasIndex(d => d.Filename).IsUnique();

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
        var config =
            new ConfigurationBuilder()
                .AddConfiguration()
                .Build()
                // Get the configuration as our config class
                .Get<Config>() ?? new();

        var dataSource = DatabaseContext.BuildDataSource(config);

        var options = DatabaseContext
            .BuildOptions(new DbContextOptionsBuilder(), dataSource, null)
            .Options;

        return new DatabaseContext(options);
    }
}
