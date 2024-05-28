using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Foxnouns.Backend.Middleware;

public class ErrorHandlerMiddleware(ILogger baseLogger) : IMiddleware
{
    public async Task InvokeAsync(HttpContext ctx, RequestDelegate next)
    {
        try
        {
            await next(ctx);
        }
        catch (Exception e)
        {
            var type = e.TargetSite?.DeclaringType ?? typeof(ErrorHandlerMiddleware);
            var typeName = e.TargetSite?.DeclaringType?.FullName ?? "<unknown>";
            var logger = baseLogger.ForContext(type);

            if (ctx.Response.HasStarted)
            {
                logger.Error(e, "Error in {ClassName} ({Path}) after response started being sent", typeName,
                    ctx.Request.Path);
                return;
            }

            if (e is ApiError ae)
            {
                ctx.Response.StatusCode = (int)ae.StatusCode;
                ctx.Response.Headers.RequestId = ctx.TraceIdentifier;
                ctx.Response.ContentType = "application/json; charset=utf-8";
                if (ae is ApiError.Forbidden fe)
                {
                    await ctx.Response.WriteAsync(JsonConvert.SerializeObject(new HttpApiError
                    {
                        Status = (int)fe.StatusCode,
                        Code = ErrorCode.Forbidden,
                        Message = fe.Message,
                        Scopes = fe.Scopes.Length > 0 ? fe.Scopes : null
                    }));
                    return;
                }

                await ctx.Response.WriteAsync(JsonConvert.SerializeObject(new HttpApiError
                {
                    Status = (int)ae.StatusCode,
                    Code = ErrorCode.GenericApiError,
                    Message = ae.Message,
                }));
                return;
            }

            if (e is FoxnounsError fce)
            {
                logger.Error(fce.Inner ?? fce, "Exception in {ClassName} ({Path})", typeName, ctx.Request.Path);
            }
            else
            {
                logger.Error(e, "Exception in {ClassName} ({Path})", typeName, ctx.Request.Path);
            }

            ctx.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            ctx.Response.Headers.RequestId = ctx.TraceIdentifier;
            ctx.Response.ContentType = "application/json; charset=utf-8";
            await ctx.Response.WriteAsync(JsonConvert.SerializeObject(new HttpApiError
            {
                Status = (int)HttpStatusCode.InternalServerError,
                Code = ErrorCode.InternalServerError,
                Message = "Internal server error",
            }));
        }
    }
}

public record HttpApiError
{
    public required int Status { get; init; }
    [JsonConverter(typeof(StringEnumConverter))]
    public required ErrorCode Code { get; init; }
    public required string Message { get; init; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string[]? Scopes { get; init; }
}

public enum ErrorCode
{
    InternalServerError,
    Forbidden,
    BadRequest,
    AuthenticationError,
    GenericApiError,
}