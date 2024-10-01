using System.Net;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Foxnouns.Backend;

public class FoxnounsError(string message, Exception? inner = null) : Exception(message)
{
    public Exception? Inner => inner;

    public class DatabaseError(string message, Exception? inner = null)
        : FoxnounsError(message, inner);

    public class UnknownEntityError(Type entityType, Exception? inner = null)
        : DatabaseError($"Entity of type {entityType.Name} not found", inner);
}

public class ApiError(
    string message,
    HttpStatusCode? statusCode = null,
    ErrorCode? errorCode = null
) : FoxnounsError(message)
{
    public readonly HttpStatusCode StatusCode = statusCode ?? HttpStatusCode.InternalServerError;
    public readonly ErrorCode ErrorCode = errorCode ?? ErrorCode.InternalServerError;

    public class Unauthorized(string message, ErrorCode errorCode = ErrorCode.AuthenticationError)
        : ApiError(message, statusCode: HttpStatusCode.Unauthorized, errorCode: errorCode);

    public class Forbidden(
        string message,
        IEnumerable<string>? scopes = null,
        ErrorCode errorCode = ErrorCode.Forbidden
    ) : ApiError(message, statusCode: HttpStatusCode.Forbidden, errorCode: errorCode)
    {
        public readonly string[] Scopes = scopes?.ToArray() ?? [];
    }

    public class BadRequest(
        string message,
        IReadOnlyDictionary<string, IEnumerable<ValidationError>>? errors = null
    ) : ApiError(message, statusCode: HttpStatusCode.BadRequest)
    {
        public BadRequest(string message, string field, object actualValue)
            : this(
                "Error validating input",
                new Dictionary<string, IEnumerable<ValidationError>>
                {
                    { field, [ValidationError.GenericValidationError(message, actualValue)] },
                }
            ) { }

        public JObject ToJson()
        {
            var o = new JObject
            {
                { "status", (int)HttpStatusCode.BadRequest },
                { "message", Message },
                { "code", "BAD_REQUEST" },
            };
            if (errors == null)
                return o;

            var a = new JArray();
            foreach (var error in errors)
            {
                var errorObj = new JObject
                {
                    { "key", error.Key },
                    { "errors", JArray.FromObject(error.Value) },
                };
                a.Add(errorObj);
            }

            o.Add("errors", a);
            return o;
        }
    }

    /// <summary>
    /// A special version of BadRequest that ASP.NET generates when it encounters an invalid request.
    /// Any other methods should use <see cref="ApiError.BadRequest" /> instead.
    /// </summary>
    public class AspBadRequest(string message, ModelStateDictionary? modelState = null)
        : ApiError(message, statusCode: HttpStatusCode.BadRequest)
    {
        public JObject ToJson()
        {
            var o = new JObject
            {
                { "status", (int)HttpStatusCode.BadRequest },
                { "message", Message },
                { "code", "BAD_REQUEST" },
            };
            if (modelState == null)
                return o;

            var a = new JArray();
            foreach (var error in modelState.Where(e => e.Value is { Errors.Count: > 0 }))
            {
                var errorObj = new JObject
                {
                    { "key", error.Key },
                    {
                        "errors",
                        new JArray(
                            error.Value!.Errors.Select(e => new JObject
                            {
                                { "message", e.ErrorMessage },
                            })
                        )
                    },
                };
                a.Add(errorObj);
            }

            o.Add("errors", a);
            return o;
        }
    }

    public class NotFound(string message, ErrorCode? code = null)
        : ApiError(message, statusCode: HttpStatusCode.NotFound, errorCode: code);

    public class AuthenticationError(string message)
        : ApiError(message, statusCode: HttpStatusCode.BadRequest);
}

public enum ErrorCode
{
    InternalServerError,
    Forbidden,
    BadRequest,
    AuthenticationError,
    AuthenticationRequired,
    MissingScopes,
    GenericApiError,
    UserNotFound,
    MemberNotFound,
}

public class ValidationError
{
    public required string Message { get; init; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public int? MinLength { get; init; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public int? MaxLength { get; init; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public int? ActualLength { get; init; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public IEnumerable<object>? AllowedValues { get; init; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public object? ActualValue { get; init; }

    public static ValidationError LengthError(
        string message,
        int minLength,
        int maxLength,
        int actualLength
    )
    {
        return new ValidationError
        {
            Message = message,
            MinLength = minLength,
            MaxLength = maxLength,
            ActualLength = actualLength,
        };
    }

    public static ValidationError DisallowedValueError(
        string message,
        IEnumerable<object> allowedValues,
        object actualValue
    )
    {
        return new ValidationError
        {
            Message = message,
            AllowedValues = allowedValues,
            ActualValue = actualValue,
        };
    }

    public static ValidationError GenericValidationError(string message, object? actualValue)
    {
        return new ValidationError { Message = message, ActualValue = actualValue };
    }
}
