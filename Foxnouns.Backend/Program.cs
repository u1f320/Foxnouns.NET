using Foxnouns.Backend;
using Foxnouns.Backend.Database;
using Serilog;
using Foxnouns.Backend.Extensions;
using Foxnouns.Backend.Middleware;
using Foxnouns.Backend.Services;
using Foxnouns.Backend.Utils;
using Hangfire;
using Hangfire.Redis.StackExchange;
using Microsoft.AspNetCore.Mvc;
using Minio;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Sentry.Extensibility;
using Sentry.Hangfire;

// Read version information from .version in the repository root
await BuildInfo.ReadBuildInfo();

var builder = WebApplication.CreateBuilder(args);

var config = builder.AddConfiguration();

builder.AddSerilog();

builder.WebHost
    .UseSentry(opts =>
    {
        opts.Dsn = config.Logging.SentryUrl;
        opts.TracesSampleRate = config.Logging.SentryTracesSampleRate;
        opts.MaxRequestBodySize = RequestSize.Small;
    })
    .ConfigureKestrel(opts =>
    {
        // Requests are limited to a maximum of 2 MB.
        // No valid request body will ever come close to this limit,
        // but the limit is slightly higher to prevent valid requests from being rejected.
        opts.Limits.MaxRequestBodySize = 2 * 1024 * 1024;
    });

builder.Services
    .AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ContractResolver = new PatchRequestContractResolver
        {
            NamingStrategy = new SnakeCaseNamingStrategy()
        };
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = actionContext => new BadRequestObjectResult(
            new ApiError.AspBadRequest("Bad request", actionContext.ModelState).ToJson()
        );
    });

// Set the default converter to snake case as we use it in a couple places.
JsonConvert.DefaultSettings = () => new JsonSerializerSettings
{
    ContractResolver = new DefaultContractResolver
    {
        NamingStrategy = new SnakeCaseNamingStrategy()
    }
};

builder.Services
    .AddDbContext<DatabaseContext>()
    .AddCustomServices()
    .AddCustomMiddleware()
    .AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddMinio(c =>
        c.WithEndpoint(config.Storage.Endpoint)
            .WithCredentials(config.Storage.AccessKey, config.Storage.SecretKey)
            .Build());

builder.Services.AddHangfire(c => c.UseSentry().UseRedisStorage(config.Jobs.Redis, new RedisStorageOptions
{
    Prefix = "foxnouns_"
}))
    .AddHangfireServer(options => { options.WorkerCount = config.Jobs.Workers; });

var app = builder.Build();

await app.Initialize(args);

app.UseSerilogRequestLogging();
app.UseRouting();
// Not all environments will want tracing (from experience, it's expensive to use in production, even with a low sample rate),
// so it's locked behind a config option.
if (config.Logging.SentryTracing) app.UseSentryTracing();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();
app.UseCustomMiddleware();
app.MapControllers();

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    AppPath = null,
    AsyncAuthorization = [new HangfireDashboardAuthorizationFilter(app.Services)]
});

app.Urls.Clear();
app.Urls.Add(config.Address);

// Fire off the periodic tasks loop in the background
_ = new Timer(_ =>
{
    var __ = RunPeriodicTasksAsync();
}, null, TimeSpan.FromSeconds(30), TimeSpan.FromMinutes(1));

app.Run();
Log.CloseAndFlush();

return;

async Task RunPeriodicTasksAsync()
{
    await using var scope = app.Services.CreateAsyncScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger>();
    logger.Debug("Running periodic tasks");

    var keyCacheSvc = scope.ServiceProvider.GetRequiredService<KeyCacheService>();
    await keyCacheSvc.DeleteExpiredKeysAsync();
}