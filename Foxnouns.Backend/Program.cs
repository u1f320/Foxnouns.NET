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
using System.Text.Json;
using System.Text.Json.Serialization;
using Foxnouns.Backend;
using Foxnouns.Backend.Extensions;
using Foxnouns.Backend.Services;
using Foxnouns.Backend.Utils;
using Foxnouns.Backend.Utils.OpenApi;
using Hangfire;
using Hangfire.Redis.StackExchange;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Prometheus;
using Sentry.Extensibility;
using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

Config config = builder.AddConfiguration();

builder.AddSerilog();

builder
    .WebHost.UseSentry(opts =>
    {
        opts.Dsn = config.Logging.SentryUrl ?? "";
        opts.TracesSampleRate = config.Logging.SentryTracesSampleRate;
        opts.MaxRequestBodySize = RequestSize.Small;
    })
    .ConfigureKestrel(opts =>
    {
        // Requests are limited to a maximum of 2 MB.
        // No valid request body will ever come close to this limit,
        // but the limit is slightly higher to prevent valid requests from being rejected.
        opts.Limits.MaxRequestBodySize = 2 * 1024 * 1024;
    })
    .UseUrls(config.Address);

builder
    .Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseUpper)
        );
    })
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ContractResolver = new PatchRequestContractResolver
        {
            NamingStrategy = new SnakeCaseNamingStrategy(),
        };
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        // the type isn't needed but without it, rider keeps complaining for no reason (it compiles just fine)
        options.InvalidModelStateResponseFactory = (ActionContext actionContext) =>
            new BadRequestObjectResult(
                new ApiError.AspBadRequest("Bad request", actionContext.ModelState).ToJson()
            );
    });

builder
    .Services.AddHangfire(
        (services, c) =>
        {
            c.UseRedisStorage(
                services.GetRequiredService<KeyCacheService>().Multiplexer,
                new RedisStorageOptions { Prefix = "foxnouns_net:" }
            );
        }
    )
    .AddHangfireServer();

builder.Services.AddOpenApi(
    "v2",
    options =>
    {
        options.AddSchemaTransformer<PropertyKeySchemaTransformer>();
        options.AddSchemaTransformer<ExampleFixingSchemaTransformer>();
        options.AddDocumentTransformer(new DocumentTransformer(config));
    }
);

// Set the default converter to snake case as we use it in a couple places.
JsonConvert.DefaultSettings = () =>
    new JsonSerializerSettings
    {
        ContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new SnakeCaseNamingStrategy(),
        },
    };

builder.AddServices(config).AddCustomMiddleware();

WebApplication app = builder.Build();

await app.Initialize(args);

app.UseSerilogRequestLogging();
app.UseRouting();

// Not all environments will want tracing (from experience, it's expensive to use in production, even with a low sample rate),
// so it's locked behind a config option.
if (config.Logging.SentryTracing)
    app.UseSentryTracing();
app.UseCors();
app.UseCustomMiddleware();
app.MapControllers();
app.UseHangfireDashboard();

// TODO: I can't figure out why this doesn't work yet
// TODO: Manually write API docs in the meantime
// app.MapOpenApi("/api-docs/openapi/{documentName}.json");
// app.MapScalarApiReference(
//     "/api-docs/",
//     options =>
//     {
//         options.Title = "pronouns.cc API";
//         options.OpenApiRoutePattern = "/api-docs/openapi/{documentName}.json";
//     }
// );

// Make sure metrics are updated whenever Prometheus scrapes them
Metrics.DefaultRegistry.AddBeforeCollectCallback(async ct =>
    await app.Services.GetRequiredService<MetricsCollectionService>().CollectMetricsAsync(ct)
);

app.Run();
Log.CloseAndFlush();
