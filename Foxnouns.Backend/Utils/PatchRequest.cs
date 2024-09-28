using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Foxnouns.Backend.Utils;

/// <summary>
/// A base class used for PATCH requests which stores information on whether a key is explicitly set to null or not passed at all.
///
/// HasProperty() should not be used for properties that cannot be set to null--a null value should be treated
/// as an unset value in those cases.
/// </summary>
public abstract class PatchRequest
{
    private readonly HashSet<string> _properties = [];
    public bool HasProperty(string propertyName) => _properties.Contains(propertyName);
    public void SetHasProperty(string propertyName) => _properties.Add(propertyName);
}

/// <summary>
/// A custom contract resolver to reduce the boilerplate needed to use <see cref="PatchRequest" />.
/// Based on this StackOverflow answer: https://stackoverflow.com/a/58748036
/// </summary>
public class PatchRequestContractResolver : DefaultContractResolver
{
    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        var prop = base.CreateProperty(member, memberSerialization);

        prop.SetIsSpecified += (o, _) =>
        {
            if (o is not PatchRequest patchRequest) return;
            patchRequest.SetHasProperty(prop.UnderlyingName!);
        };

        return prop;
    }
}