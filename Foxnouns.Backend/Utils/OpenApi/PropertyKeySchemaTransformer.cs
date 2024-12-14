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
using Foxnouns.Backend.Database;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;

namespace Foxnouns.Backend.Utils.OpenApi;

public class PropertyKeySchemaTransformer : IOpenApiSchemaTransformer
{
    private static readonly DefaultContractResolver SnakeCaseConverter =
        new() { NamingStrategy = new SnakeCaseNamingStrategy() };

    public Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken
    )
    {
        Dictionary<string, OpenApiSchema> newProperties = new();
        foreach (KeyValuePair<string, OpenApiSchema> property in schema.Properties)
        {
            newProperties[SnakeCaseConverter.GetResolvedPropertyName(property.Key)] =
                property.Value;
        }

        schema.Properties = newProperties;
        schema.Required = schema
            .Required.Select(SnakeCaseConverter.GetResolvedPropertyName)
            .ToHashSet();

        return Task.CompletedTask;
    }
}

public class ExampleFixingSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken
    )
    {
        if (context.JsonTypeInfo.Type == typeof(Snowflake))
        {
            schema.Type = "string";
            schema.Example = new OpenApiString("999999999999999999");
        }

        return Task.CompletedTask;
    }
}

public class DocumentTransformer(Config config) : IOpenApiDocumentTransformer
{
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken
    )
    {
        document.Info.Title = "pronouns.cc API";
        document.Info.Version = "2.0.0";

        document.Servers.Clear();
        document.Servers.Add(new OpenApiServer { Url = config.BaseUrl });
        return Task.CompletedTask;
    }
}
