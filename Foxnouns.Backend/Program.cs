using Foxnouns.Backend;
using Foxnouns.Backend.Database;
using Serilog;
using Foxnouns.Backend.Extensions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

var config = builder.AddConfiguration();

builder.AddSerilog(config.LogEventLevel);

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