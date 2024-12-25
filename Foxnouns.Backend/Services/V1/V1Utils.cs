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
