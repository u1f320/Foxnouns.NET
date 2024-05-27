using Foxnouns.Backend.Database;
using Serilog;
using Foxnouns.Backend.Extensions;
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
        });

builder.Services
    .AddDbContext<DatabaseContext>()
    .AddSnowflakeGenerator()
    .AddEndpointsApiExplorer()
    .AddSwaggerGen();

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseRouting();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();
app.MapControllers();

app.Urls.Clear();
app.Urls.Add(config.Address);

app.Run();

Log.CloseAndFlush();
