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
namespace Foxnouns.Backend.Utils;

/// <summary>
/// Static methods for validating user input (mostly making sure it's not too short or too long)
/// </summary>
public static partial class ValidationUtils
{
    public static void Validate(IEnumerable<(string, ValidationError?)> errors)
    {
        errors = errors.Where(e => e.Item2 != null).ToList();
        if (!errors.Any())
            return;

        var errorDict = new Dictionary<string, IEnumerable<ValidationError>>();
        foreach ((string, ValidationError?) error in errors)
        {
            if (errorDict.TryGetValue(error.Item1, out IEnumerable<ValidationError>? value))
                errorDict[error.Item1] = value.Append(error.Item2!);
            errorDict.Add(error.Item1, [error.Item2!]);
        }

        throw new ApiError.BadRequest("Error validating input", errorDict);
    }
}
