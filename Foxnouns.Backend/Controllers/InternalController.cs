using System.Text.RegularExpressions;
using Foxnouns.Backend.Database;
using Foxnouns.Backend.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing.Template;

namespace Foxnouns.Backend.Controllers;

[ApiController]
[Route("/api/internal")]
public partial class InternalController(DatabaseContext db) : ControllerBase
{
    [GeneratedRegex(@"(\{\w+\})")]
    private static partial Regex PathVarRegex();

    [GeneratedRegex(@"\{id\}")]
    private static partial Regex IdCountRegex();

    private static string GetCleanedTemplate(string template)
    {
        if (template.StartsWith("api/v2"))
            template = template["api/v2".Length..];
        template = PathVarRegex()
            .Replace(template, "{id}") // Replace all path variables (almost always IDs) with `{id}`
            .Replace("@me", "{id}"); // Also replace hardcoded `@me` with `{id}`

        // If there's at least one path parameter, we only return the *first* part of the path.
        if (template.Contains("{id}"))
        {
            // However, if the path starts with /users/{id} *and* there's another path parameter (such as a member ID)
            // we ignore the leading /users/{id}. This is because a lot of routes are scoped by user, but should have
            // separate rate limits from other user-scoped routes.
            if (template.StartsWith("/users/{id}/") && IdCountRegex().Count(template) >= 2)
                template = template["/users/{id}".Length..];

            return template.Split("{id}")[0] + "{id}";
        }

        return template;
    }

    [HttpPost("request-data")]
    public async Task<IActionResult> GetRequestDataAsync([FromBody] RequestDataRequest req)
    {
        RouteEndpoint? endpoint = GetEndpoint(HttpContext, req.Path, req.Method);
        if (endpoint == null)
            throw new ApiError.BadRequest("Path/method combination is invalid");

        ControllerActionDescriptor? actionDescriptor =
            endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();
        string? template = actionDescriptor?.AttributeRouteInfo?.Template;
        if (template == null)
            throw new FoxnounsError("Template value was null on valid endpoint");
        template = GetCleanedTemplate(template);

        // If no token was supplied, or it isn't valid base 64, return a null user ID (limiting by IP)
        if (!AuthUtils.TryParseToken(req.Token, out byte[]? rawToken))
            return Ok(new RequestDataResponse(null, template));

        Snowflake? userId = await db.GetTokenUserId(rawToken);
        return Ok(new RequestDataResponse(userId, template));
    }

    public record RequestDataRequest(string? Token, string Method, string Path);

    public record RequestDataResponse(Snowflake? UserId, string Template);

    private static RouteEndpoint? GetEndpoint(
        HttpContext httpContext,
        string url,
        string requestMethod
    )
    {
        EndpointDataSource? endpointDataSource =
            httpContext.RequestServices.GetService<EndpointDataSource>();
        if (endpointDataSource == null)
            return null;
        IEnumerable<RouteEndpoint> endpoints = endpointDataSource.Endpoints.OfType<RouteEndpoint>();

        foreach (RouteEndpoint? endpoint in endpoints)
        {
            if (endpoint.RoutePattern.RawText == null)
                continue;

            var templateMatcher = new TemplateMatcher(
                TemplateParser.Parse(endpoint.RoutePattern.RawText),
                new RouteValueDictionary()
            );
            if (!templateMatcher.TryMatch(url, new RouteValueDictionary()))
                continue;
            HttpMethodAttribute? httpMethodAttribute =
                endpoint.Metadata.GetMetadata<HttpMethodAttribute>();
            if (
                httpMethodAttribute != null
                && !httpMethodAttribute.HttpMethods.Any(x =>
                    x.Equals(requestMethod, StringComparison.OrdinalIgnoreCase)
                )
            )
                continue;
            return endpoint;
        }

        return null;
    }
}
