// ReSharper disable NotAccessedPositionalProperty.Global
namespace Foxnouns.Backend.Dto;

public record MetaResponse(
    string Repository,
    string Version,
    string Hash,
    int Members,
    UserInfoResponse Users,
    LimitsResponse Limits
);

public record UserInfoResponse(int Total, int ActiveMonth, int ActiveWeek, int ActiveDay);

public record LimitsResponse(
    int MemberCount,
    int BioLength,
    int CustomPreferences,
    int MaxAuthMethods,
    int MaxFlags
);
