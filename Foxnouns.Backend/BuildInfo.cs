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
namespace Foxnouns.Backend;

public static class BuildInfo
{
    public const string Repository = "https://codeberg.org/pronounscc/pronouns.cc";

    public static string Hash { get; private set; } = "(unknown)";
    public static string Version { get; private set; } = "(unknown)";

    public static async Task ReadBuildInfo()
    {
        await using Stream? stream = typeof(BuildInfo).Assembly.GetManifestResourceStream(
            "version"
        );
        if (stream == null)
            return;

        using var reader = new StreamReader(stream);
        string[] data = (await reader.ReadToEndAsync()).Trim().Split("\n");
        if (data.Length < 3)
            return;

        Hash = data[0];
        bool dirty = data[2] == "dirty";

        string[] versionData = data[1].Split("-");
        if (versionData.Length < 3)
            return;
        Version = versionData[0];
        if (versionData[1] != "0" || dirty)
            Version += $"+{versionData[2]}";
        if (dirty)
            Version += ".dirty";
    }
}
