using System.Globalization;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Foxnouns.Backend.Utils;

/// <summary>
/// A custom StringEnumConverter that converts enum members to SCREAMING_SNAKE_CASE, rather than CamelCase as is the default.
/// Newtonsoft.Json doesn't provide a screaming snake case naming strategy, so we just wrap the normal snake case one and convert it to uppercase.
/// </summary>
public class ScreamingSnakeCaseEnumConverter() : StringEnumConverter(new ScreamingSnakeCaseNamingStrategy(), false)
{
    private class ScreamingSnakeCaseNamingStrategy : SnakeCaseNamingStrategy
    {
        protected override string ResolvePropertyName(string name) =>
            base.ResolvePropertyName(name).ToUpper(CultureInfo.InvariantCulture);
    }
}