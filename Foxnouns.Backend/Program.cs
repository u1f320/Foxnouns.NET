using Foxnouns.Backend;
using Foxnouns.Backend.Database;
using Serilog;
using Foxnouns.Backend.Extensions;
using Foxnouns.Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NodaTime;

// Read version information from .version in the repository root
await BuildInfo.ReadBuildInfo();

var builder = WebApplication.CreateBuilder(args);

var config = builder.AddConfiguration();

builder.AddSerilog();

builder.Services
    .AddControllers()
    .AddNewtonsoftJson(options =>
        options.SerializerSettings.ContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new SnakeCaseNamingStrategy()
        })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = actionContext => new BadRequestObjectResult(
            new ApiError.BadRequest("Bad request", actionContext.ModelState).ToJson()
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
    .AddSwaggerGen();

var app = builder.Build();

await app.Initialize(args);

app.UseSerilogRequestLogging();
app.UseRouting();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();
app.UseCustomMiddleware();
app.MapControllers();

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