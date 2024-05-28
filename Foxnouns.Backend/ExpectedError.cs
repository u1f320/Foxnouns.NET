using System.Net;
using Foxnouns.Backend.Middleware;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json.Linq;

namespace Foxnouns.Backend;

public class FoxnounsError(string message, Exception? inner = null) : Exception(message)
{
    public Exception? Inner => inner;

    public class DatabaseError(string message, Exception? inner = null) : FoxnounsError(message, inner);

    public class UnknownEntityError(Type entityType, Exception? inner = null)
        : DatabaseError($"Entity of type {entityType.Name} not found", inner);
}

public class ApiError(string message, HttpStatusCode? statusCode = null, ErrorCode? errorCode = null)
    : FoxnounsError(message)
{
    public readonly HttpStatusCode StatusCode = statusCode ?? HttpStatusCode.InternalServerError;
    public readonly ErrorCode ErrorCode = errorCode ?? ErrorCode.InternalServerError;

    public class Unauthorized(string message) : ApiError(message, statusCode: HttpStatusCode.Unauthorized);

    public class Forbidden(string message, IEnumerable<string>? scopes = null)
        : ApiError(message, statusCode: HttpStatusCode.Forbidden)
    {
        public readonly string[] Scopes = scopes?.ToArray() ?? [];
    }

    public class BadRequest(string message, ModelStateDictionary? modelState = null)
        : ApiError(message, statusCode: HttpStatusCode.BadRequest)
    {
        public JObject ToJson()
        {
            var o = new JObject
            {
                { "status", (int)HttpStatusCode.BadRequest },
                { "code", ErrorCode.BadRequest.ToString() }
            };
            if (modelState == null) return o;

            var a = new JArray();
            foreach (var error in modelState.Where(e => e.Value is { Errors.Count: > 0 }))
            {
                var errorObj = new JObject
                {
                    { "key", error.Key },
                    {
                        "errors",
                        new JArray(error.Value!.Errors.Select(e => new JObject { { "message", e.ErrorMessage } }))
                    }
                };

                a.Add(errorObj);
            }

            o.Add("errors", a);
            return o;
        }
    }

    public class NotFound(string message, ErrorCode? code = null)
        : ApiError(message, statusCode: HttpStatusCode.NotFound, errorCode: code);

    public class AuthenticationError(string message) : ApiError(message, statusCode: HttpStatusCode.BadRequest);
}

public enum ErrorCode
{
    InternalServerError,
    Forbidden,
    BadRequest,
    AuthenticationError,
    GenericApiError,
    UserNotFound,
}