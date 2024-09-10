using Foxnouns.Backend;
using Serilog;
using Foxnouns.Backend.Extensions;
using Foxnouns.Backend.Services;
using Foxnouns.Backend.Utils;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Prometheus;
using Sentry.Extensibility;

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

builder.AddServices(config)
    .AddCustomMiddleware()
    .AddEndpointsApiExplorer()
    .AddSwaggerGen();

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

app.Urls.Clear();
app.Urls.Add(config.Address);

// Make sure metrics are updated whenever Prometheus scrapes them
Metrics.DefaultRegistry.AddBeforeCollectCallback(async ct =>
    await app.Services.GetRequiredService<MetricsCollectionService>().CollectMetricsAsync(ct));

app.Run();
Log.CloseAndFlush();