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
using Foxnouns.Backend.Database.Models;

namespace Foxnouns.Backend.Services.V1;

public static class V1Utils
{
    public static string TranslateStatus(
        string status,
        Dictionary<Snowflake, User.CustomPreference> customPreferences
    )
    {
        if (!Snowflake.TryParse(status, out Snowflake? sf))
            return status;

        return customPreferences.TryGetValue(sf.Value, out User.CustomPreference? cf)
            ? cf.LegacyId.ToString()
            : "unknown";
    }
}
