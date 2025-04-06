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

// ReSharper disable NotAccessedPositionalProperty.Global
using Foxnouns.Backend.Database;

namespace Foxnouns.Backend.Dto;

public record MetaResponse(
    string Repository,
    string Version,
    string Hash,
    int Members,
    UserInfoResponse Users,
    LimitsResponse Limits,
    MetaNoticeResponse? Notice
);

public record MetaNoticeResponse(Snowflake Id, string Message);

public record UserInfoResponse(int Total, int ActiveMonth, int ActiveWeek, int ActiveDay);

public record LimitsResponse(
    int MemberCount,
    int BioLength,
    int CustomPreferences,
    int MaxAuthMethods,
    int MaxFlags
);
