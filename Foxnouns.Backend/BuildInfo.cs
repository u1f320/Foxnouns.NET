namespace Foxnouns.Backend;

public static class BuildInfo
{
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
