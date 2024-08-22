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
            Timeout = config.Database.Timeout ?? 5,
            MaxPoolSize = config.Database.MaxPoolSize ?? 50,
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
        modelBuilder.Entity<Member>().HasIndex(m => new { m.UserId, m.Name }).IsUnique();
        modelBuilder.Entity<TemporaryKey>().HasIndex(k => k.Key).IsUnique();

        modelBuilder.Entity<User>().Property(u => u.Fields).HasColumnType("jsonb");
        modelBuilder.Entity<User>().Property(u => u.Names).HasColumnType("jsonb");
        modelBuilder.Entity<User>().Property(u => u.Pronouns).HasColumnType("jsonb");
        modelBuilder.Entity<User>().Property(u => u.CustomPreferences).HasColumnType("jsonb");

        modelBuilder.Entity<Member>().Property(m => m.Fields).HasColumnType("jsonb");
        modelBuilder.Entity<Member>().Property(m => m.Names).HasColumnType("jsonb");
        modelBuilder.Entity<Member>().Property(m => m.Pronouns).HasColumnType("jsonb");
    }
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