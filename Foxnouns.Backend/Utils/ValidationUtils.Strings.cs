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

public static partial class ValidationUtils
{
    public const int MaximumReportContextLength = 512;

    public static ValidationError? ValidateReportContext(string? context) =>
        context?.Length > MaximumReportContextLength
            ? ValidationError.GenericValidationError("Avatar is too large", null)
            : null;

    public const int MinimumPasswordLength = 12;
    public const int MaximumPasswordLength = 1024;

    public static ValidationError? ValidatePassword(string password) =>
        password.Length switch
        {
            < MinimumPasswordLength => ValidationError.LengthError(
                "Password is too short",
                MinimumPasswordLength,
                MaximumPasswordLength,
                password.Length
            ),
            > MaximumPasswordLength => ValidationError.LengthError(
                "Password is too long",
                MinimumPasswordLength,
                MaximumPasswordLength,
                password.Length
            ),
            _ => null,
        };
}
