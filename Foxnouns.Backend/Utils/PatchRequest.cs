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
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Foxnouns.Backend.Utils;

/// <summary>
/// <para>A base class used for PATCH requests which stores information on whether a key is explicitly set to null or not passed at all.</para>
/// <para>
/// HasProperty() should not be used for properties that cannot be set to null--a null value should be treated
/// as an unset value in those cases.
/// </para>
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
    protected override JsonProperty CreateProperty(
        MemberInfo member,
        MemberSerialization memberSerialization
    )
    {
        JsonProperty prop = base.CreateProperty(member, memberSerialization);

        prop.SetIsSpecified += (o, _) =>
        {
            if (o is not PatchRequest patchRequest)
                return;
            patchRequest.SetHasProperty(prop.UnderlyingName!);
        };

        return prop;
    }
}
