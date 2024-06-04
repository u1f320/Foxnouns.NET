using Foxnouns.Backend;
using Foxnouns.Backend.Database;
using Serilog;
using Foxnouns.Backend.Extensions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

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

app.Run();

Log.CloseAndFlush();