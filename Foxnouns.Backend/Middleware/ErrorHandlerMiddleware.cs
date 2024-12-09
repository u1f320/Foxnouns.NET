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
using System.Net;
using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Utils;
using Newtonsoft.Json;

namespace Foxnouns.Backend.Middleware;

public class ErrorHandlerMiddleware(ILogger baseLogger, IHub sentry) : IMiddleware
{
    public async Task InvokeAsync(HttpContext ctx, RequestDelegate next)
    {
        try
        {
            await next(ctx);
        }
        catch (Exception e)
        {
            Type type = e.TargetSite?.DeclaringType ?? typeof(ErrorHandlerMiddleware);
            string typeName = e.TargetSite?.DeclaringType?.FullName ?? "<unknown>";
            ILogger logger = baseLogger.ForContext(type);

            if (ctx.Response.HasStarted)
            {
                logger.Error(
                    e,
                    "Error in {ClassName} ({Path}) after response started being sent",
                    typeName,
                    ctx.Request.Path
                );

                sentry.CaptureException(
                    e,
                    scope =>
                    {
                        User? user = ctx.GetUser();
                        if (user != null)
                        {
                            scope.User = new SentryUser
                            {
                                Id = user.Id.ToString(),
                                Username = user.Username,
                            };
                        }
                    }
                );

                return;
            }

            if (e is ApiError ae)
            {
                ctx.Response.StatusCode = (int)ae.StatusCode;
                ctx.Response.Headers.RequestId = ctx.TraceIdentifier;
                ctx.Response.ContentType = "application/json; charset=utf-8";
                if (ae is ApiError.Forbidden fe)
                {
                    await ctx.Response.WriteAsync(
                        JsonConvert.SerializeObject(
                            new HttpApiError
                            {
                                Status = (int)fe.StatusCode,
                                Code = ErrorCode.Forbidden,
                                Message = fe.Message,
                                Scopes = fe.Scopes.Length > 0 ? fe.Scopes : null,
                            }
                        )
                    );
                    return;
                }

                if (ae is ApiError.BadRequest br)
                {
                    await ctx.Response.WriteAsync(br.ToJson().ToString());
                    return;
                }

                await ctx.Response.WriteAsync(
                    JsonConvert.SerializeObject(
                        new HttpApiError
                        {
                            Status = (int)ae.StatusCode,
                            Code = ae.ErrorCode,
                            Message = ae.Message,
                        }
                    )
                );
                return;
            }

            if (e is FoxnounsError fce)
            {
                logger.Error(
                    fce.Inner ?? fce,
                    "Exception in {ClassName} ({Path})",
                    typeName,
                    ctx.Request.Path
                );
            }
            else
            {
                logger.Error(e, "Exception in {ClassName} ({Path})", typeName, ctx.Request.Path);
            }

            SentryId errorId = sentry.CaptureException(
                e,
                scope =>
                {
                    User? user = ctx.GetUser();
                    if (user != null)
                    {
                        scope.User = new SentryUser
                        {
                            Id = user.Id.ToString(),
                            Username = user.Username,
                        };
                    }
                }
            );

            ctx.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            ctx.Response.Headers.RequestId = ctx.TraceIdentifier;
            ctx.Response.ContentType = "application/json; charset=utf-8";
            await ctx.Response.WriteAsync(
                JsonConvert.SerializeObject(
                    new HttpApiError
                    {
                        Status = (int)HttpStatusCode.InternalServerError,
                        Code = ErrorCode.InternalServerError,
                        ErrorId = errorId.ToString(),
                        Message = "Internal server error",
                    }
                )
            );
        }
    }
}

public record HttpApiError
{
    public required int Status { get; init; }

    [JsonConverter(typeof(ScreamingSnakeCaseEnumConverter))]
    public required ErrorCode Code { get; init; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string? ErrorId { get; init; }

    public required string Message { get; init; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string[]? Scopes { get; init; }
}
