using System.Diagnostics.CodeAnalysis;
using EntityFramework.Exceptions.PostgreSQL;
using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Npgsql;

namespace Foxnouns.Backend.Database;

public class DatabaseContext : DbContext
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly ILoggerFactory? _loggerFactory;

    public DbSet<User> Users { get; set; }
    public DbSet<Member> Members { get; set; }
    public DbSet<AuthMethod> AuthMethods { get; set; }
    public DbSet<FediverseApplication> FediverseApplications { get; set; }
    public DbSet<Token> Tokens { get; set; }
    public DbSet<Application> Applications { get; set; }
    public DbSet<TemporaryKey> TemporaryKeys { get; set; }

    public DatabaseContext(Config config, ILoggerFactory? loggerFactory)
    {
        var connString = new NpgsqlConnectionStringBuilder(config.Database.Url)
        {
            Pooling = config.Database.EnablePooling ?? true,
            Timeout = config.Database.Timeout ?? 5,
            MaxPoolSize = config.Database.MaxPoolSize ?? 50,
            MinPoolSize = 0,
            ConnectionPruningInterval = 10,
            ConnectionIdleLifetime = 10,
        }.ConnectionString;

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connString);
        dataSourceBuilder.UseNodaTime();
        dataSourceBuilder.UseJsonNet();
        _dataSource = dataSourceBuilder.Build();
        _loggerFactory = loggerFactory;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder
            .ConfigureWarnings(c =>
                c.Ignore(CoreEventId.ManyServiceProvidersCreatedWarning)
                    .Ignore(CoreEventId.SaveChangesFailed))
            .UseNpgsql(_dataSource, o => o.UseNodaTime())
            .UseSnakeCaseNamingConvention()
            .UseLoggerFactory(_loggerFactory)
            .UseExceptionProcessor();

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

        modelBuilder.Entity<User>().Property(u => u.Sid).HasDefaultValueSql("find_free_user_sid()");
        modelBuilder.Entity<User>().Property(u => u.Fields).HasColumnType("jsonb");
        modelBuilder.Entity<User>().Property(u => u.Names).HasColumnType("jsonb");
        modelBuilder.Entity<User>().Property(u => u.Pronouns).HasColumnType("jsonb");
        modelBuilder.Entity<User>().Property(u => u.CustomPreferences).HasColumnType("jsonb");
        modelBuilder.Entity<User>().Property(u => u.Settings).HasColumnType("jsonb");

        modelBuilder.Entity<Member>().Property(m => m.Sid).HasDefaultValueSql("find_free_member_sid()");
        modelBuilder.Entity<Member>().Property(m => m.Fields).HasColumnType("jsonb");
        modelBuilder.Entity<Member>().Property(m => m.Names).HasColumnType("jsonb");
        modelBuilder.Entity<Member>().Property(m => m.Pronouns).HasColumnType("jsonb");

        modelBuilder.HasDbFunction(typeof(DatabaseContext).GetMethod(nameof(FindFreeUserSid))!)
            .HasName("find_free_user_sid");

        modelBuilder.HasDbFunction(typeof(DatabaseContext).GetMethod(nameof(FindFreeMemberSid))!)
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

[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Used by EF Core's migration generator")]
public class DesignTimeDatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
{
    public DatabaseContext CreateDbContext(string[] args)
    {
        // Read the configuration file
        var config = new ConfigurationBuilder()
            .AddConfiguration()
            .Build()
            // Get the configuration as our config class
            .Get<Config>() ?? new();

        return new DatabaseContext(config, null);
    }
}